
using System.Security.Claims;
using System.Text.Json;
using CourseForge.Application;
using CourseForge.Domain;
using CourseForge.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseForge.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(
    AppDbContext db,
    IPaymentGateway gateway) : ControllerBase
{
    private Guid Me()
    {
        return Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [Authorize]
    [HttpPost("courses/{id:guid}/order")]
    public async Task<IActionResult> Order(
        Guid id,
        CancellationToken ct)
    {
        var course = await db.Courses
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Published);

        if (course == null)
        {
            return NotFound();
        }

        if (course.PriceInr <= 0)
        {
            return BadRequest("This course is free");
        }

        var userId = Me();

        var alreadyEnrolled = await db.Enrollments.AnyAsync(x =>
            x.CourseId == id &&
            x.UserId == userId);

        if (alreadyEnrolled)
        {
            return Conflict("Already enrolled");
        }

        var purchase = new Purchase
        {
            UserId = userId,
            CourseId = id,
            AmountInr = course.PriceInr
        };

        var result = await gateway.CreateOrderAsync(
            course.PriceInr,
            purchase.Id.ToString("N"),
            ct);

        purchase.RazorpayOrderId = result.OrderId;

        db.Purchases.Add(purchase);

        await db.SaveChangesAsync(ct);

        return Ok(new
        {
            orderId = result.OrderId,
            keyId = result.KeyId,
            amount = (long)(course.PriceInr * 100),
            currency = "INR",
            courseTitle = course.Title
        });
    }

    [Authorize]
    [HttpPost("verify")]
    public async Task<IActionResult> Verify(
        RazorpayVerifyRequest request)
    {
        var purchase = await db.Purchases
            .FirstOrDefaultAsync(x =>
                x.RazorpayOrderId == request.OrderId &&
                x.CourseId == request.CourseId &&
                x.UserId == Me());

        if (purchase == null)
        {
            return NotFound();
        }

        var isValid = gateway.Verify(
            request.OrderId,
            request.PaymentId,
            request.Signature);

        if (!isValid)
        {
            return BadRequest("Invalid Razorpay signature");
        }

        if (purchase.Status == "Paid")
        {
            return Ok(new
            {
                enrolled = true
            });
        }

        if (purchase.Status != "Pending")
        {
            return Conflict("Purchase not pending");
        }

        purchase.RazorpayPaymentId = request.PaymentId;
        purchase.Status = "Paid";

        var alreadyEnrolled = await db.Enrollments.AnyAsync(x =>
            x.UserId == purchase.UserId &&
            x.CourseId == purchase.CourseId);

        if (!alreadyEnrolled)
        {
            db.Enrollments.Add(new Enrollment
            {
                CourseId = purchase.CourseId,
                UserId = purchase.UserId
            });
        }

        await db.SaveChangesAsync();

        return Ok(new
        {
            enrolled = true
        });
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);

        var raw = await reader.ReadToEndAsync();

        var signature = Request.Headers[
            "X-Razorpay-Signature"].ToString();

        if (!gateway.VerifyWebhook(raw, signature))
        {
            return Unauthorized();
        }

        using var document = JsonDocument.Parse(raw);

        var eventType = document.RootElement
            .GetProperty("event")
            .GetString();

        if (eventType != "payment.captured" &&
            eventType != "order.paid")
        {
            return Ok();
        }

        JsonElement entity;

        try
        {
            entity = eventType == "order.paid"
                ? document.RootElement
                    .GetProperty("payload")
                    .GetProperty("order")
                    .GetProperty("entity")
                : document.RootElement
                    .GetProperty("payload")
                    .GetProperty("payment")
                    .GetProperty("entity");
        }
        catch (KeyNotFoundException)
        {
            return BadRequest();
        }

        var orderId = eventType == "order.paid"
            ? entity.GetProperty("id").GetString()
            : entity.GetProperty("order_id").GetString();

        var purchase = await db.Purchases
            .SingleOrDefaultAsync(x =>
                x.RazorpayOrderId == orderId);

        if (purchase == null ||
            purchase.Status == "Paid")
        {
            return Ok();
        }

        var amount = entity.TryGetProperty(
            "amount",
            out var amountElement)
            ? amountElement.GetInt64()
            : 0;

        if (amount != (long)(purchase.AmountInr * 100))
        {
            return BadRequest("Amount mismatch");
        }

        purchase.Status = "Paid";

        if (eventType == "payment.captured")
        {
            purchase.RazorpayPaymentId = entity
                .GetProperty("id")
                .GetString();
        }

        var alreadyEnrolled = await db.Enrollments.AnyAsync(x =>
            x.UserId == purchase.UserId &&
            x.CourseId == purchase.CourseId);

        if (!alreadyEnrolled)
        {
            db.Enrollments.Add(new Enrollment
            {
                UserId = purchase.UserId,
                CourseId = purchase.CourseId
            });
        }

        await db.SaveChangesAsync();

        return Ok();
    }
}

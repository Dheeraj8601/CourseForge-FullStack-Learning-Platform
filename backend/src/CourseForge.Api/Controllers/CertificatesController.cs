
using System.Security.Claims;
using CourseForge.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseForge.Api.Controllers;

[ApiController]
[Route("api/certificates")]
public class CertificatesController(AppDbContext db) : ControllerBase
{
    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var id = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var certificates = await (
            from cert in db.Certificates
            join course in db.Courses
                on cert.CourseId equals course.Id
            where cert.UserId == id
            select new
            {
                cert.Id,
                cert.Code,
                course.Title,
                cert.IssuedUtc
            }
        ).ToListAsync();

        return Ok(certificates);
    }

    [Authorize]
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var user = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var cert = await db.Certificates.FindAsync(id);

        if (cert == null ||
            (cert.UserId != user && !User.IsInRole("Admin")))
        {
            return NotFound();
        }

        var student = await db.Users.FindAsync(cert.UserId);
        var course = await db.Courses.FindAsync(cert.CourseId);

        if (student == null || course == null)
        {
            return NotFound();
        }

        var name = System.Net.WebUtility.HtmlEncode(student.Name);
        var title = System.Net.WebUtility.HtmlEncode(course.Title);

        var xml = $"""
            <svg xmlns="http://www.w3.org/2000/svg"
                 width="1200"
                 height="850"
                 viewBox="0 0 1200 850">

                <rect width="1200"
                      height="850"
                      fill="#f8fafc"/>

                <rect x="30"
                      y="30"
                      width="1140"
                      height="790"
                      rx="20"
                      fill="white"
                      stroke="#d3dcf0"
                      stroke-width="6"/>

                <text x="600"
                      y="180"
                      text-anchor="middle"
                      fill="#4f46e5"
                      font-family="Arial"
                      font-size="34"
                      font-weight="bold">
                    COURSEFORGE ACADEMY
                </text>

                <text x="600"
                      y="300"
                      text-anchor="middle"
                      fill="#0f172a"
                      font-family="Georgia"
                      font-size="62">
                    Certificate of Completion
                </text>

                <text x="600"
                      y="390"
                      text-anchor="middle"
                      fill="#64748b"
                      font-family="Arial"
                      font-size="25">
                    Proudly presented to
                </text>

                <text x="600"
                      y="475"
                      text-anchor="middle"
                      fill="#0f172a"
                      font-family="Georgia"
                      font-size="50">
                    {name}
                </text>

                <text x="600"
                      y="545"
                      text-anchor="middle"
                      fill="#64748b"
                      font-family="Arial"
                      font-size="24">
                    for successfully completing
                </text>

                <text x="600"
                      y="615"
                      text-anchor="middle"
                      fill="#4f46e5"
                      font-family="Georgia"
                      font-size="40">
                    {title}
                </text>

                <text x="600"
                      y="730"
                      text-anchor="middle"
                      fill="#64748b"
                      font-family="Arial"
                      font-size="18">
                    Issued {cert.IssuedUtc:dd MMM yyyy}
                    · Verification code {cert.Code}
                </text>

            </svg>
            """;

        return File(
            System.Text.Encoding.UTF8.GetBytes(xml),
            "image/svg+xml",
            $"certificate-{cert.Code}.svg");
    }

    [AllowAnonymous]
    [HttpGet("verify/{code}")]
    public async Task<IActionResult> Verify(string code)
    {
        var c = await db.Certificates
            .FirstOrDefaultAsync(x => x.Code == code);

        if (c == null)
        {
            return NotFound();
        }

        var u = await db.Users.FindAsync(c.UserId);
        var course = await db.Courses.FindAsync(c.CourseId);

        return Ok(new
        {
            valid = true,
            student = u?.Name,
            course = course?.Title,
            c.IssuedUtc,
            c.Code
        });
    }
}

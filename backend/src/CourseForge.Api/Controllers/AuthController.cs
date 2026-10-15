
using CourseForge.Application;
using CourseForge.Domain;
using CourseForge.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseForge.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext db,
    IPasswordHasher<User> hasher,
    I​TokenService tokens,
    IGoogleIdentityVerifier google) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name) ||
            !new System.Net.Mail.MailAddress(r.Email).Address.Equals(
                r.Email,
                StringComparison.OrdinalIgnoreCase) ||
            r.Password.Length < 8)
        {
            return BadRequest(
                "Valid name/email and password of at least 8 characters required");
        }

        var email = r.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(x => x.Email == email))
        {
            return Conflict("Account already exists");
        }

        var user = new User
        {
            Name = r.Name.Trim(),
            Email = email
        };

        user.PasswordHash = hasher.HashPassword(user, r.Password);

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("Account already exists");
        }

        return Ok(new AuthResponse(
            tokens.Create(user),
            user.Name,
            user.Email,
            user.Role));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        var user = await db.Users.SingleOrDefaultAsync(
            x => x.Email == r.Email.Trim().ToLower());

        if (user?.PasswordHash == null ||
            hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                r.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid credentials");
        }

        return Ok(new AuthResponse(
            tokens.Create(user),
            user.Name,
            user.Email,
            user.Role));
    }

    [HttpPost("google")]
    public async Task<IActionResult> Google(GoogleLoginRequest r)
    {
        (string sub, string email, string name) profile;

        try
        {
            profile = await google.VerifyAsync(r.Credential);
        }
        catch (Exception ex) when (
            ex is ArgumentException or
            UnauthorizedAccessException or
            System.Security.SecurityException)
        {
            return Unauthorized("Invalid Google credential");
        }

        var email = profile.email.ToLowerInvariant();

        var user = await db.Users.FirstOrDefaultAsync(
            x => x.GoogleSubject == profile.sub);

        if (user == null)
        {
            user = await db.Users.FirstOrDefaultAsync(
                x => x.Email == email);

            if (user != null && user.GoogleSubject == null)
            {
                return Conflict(
                    "Email already registered. Sign in with password first; " +
                    "automatic account linking is disabled.");
            }

            if (user == null)
            {
                user = new User
                {
                    Name = profile.name,
                    Email = email,
                    GoogleSubject = profile.sub
                };

                db.Users.Add(user);

                await db.SaveChangesAsync();
            }
        }

        return Ok(new AuthResponse(
            tokens.Create(user),
            user.Name,
            user.Email,
            user.Role));
    }
}

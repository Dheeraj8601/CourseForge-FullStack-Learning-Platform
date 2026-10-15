using CourseForge.Domain;
namespace CourseForge.Application;
public record RegisterRequest(string Name,string Email,string Password);
public record LoginRequest(string Email,string Password);
public record GoogleLoginRequest(string Credential);
public record CourseInput(string Title,string Summary,string Category,string CoverUrl,decimal PriceInr,bool Published,bool SkipQuizzes,int PassingPercent);
public record LessonInput(string Title,string BodyMarkdown,int Position);
public record QuestionInput(string Prompt,List<string> Options,int CorrectOption,string Explanation);
public record QuizSubmission(Dictionary<Guid,int> Answers);
public record AuthResponse(string Token,string Name,string Email,string Role);
public record RazorpayVerifyRequest(Guid CourseId,string OrderId,string PaymentId,string Signature);
public interface ITokenService { string Create(User user); }
public interface IGoogleIdentityVerifier { Task<(string Subject,string Email,string Name)> VerifyAsync(string credential); }
public interface IPaymentGateway { Task<(string OrderId,string KeyId)> CreateOrderAsync(decimal amount,string receipt,CancellationToken ct); bool Verify(string orderId,string paymentId,string signature); bool VerifyWebhook(string payload,string signature); }

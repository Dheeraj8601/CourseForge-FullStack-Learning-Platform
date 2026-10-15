# CourseForge — reading-first learning platform

A responsive React 19 + ASP.NET Core 10 Web API + SQL Server starter product. Learners read lessons in order, take retryable quizzes, unlock the next page on passing, and receive a downloadable SVG certificate. Administrators create courses, reading pages and quizzes; courses can be free or purchased using Razorpay Orders. Login supports email/password with JWT and Google OAuth sign-in (Google Identity Services, verified on server).

> **Status:** working-source starter implementation, **not production audited**. The execution environment that generated this archive did not have the .NET SDK installed, so backend compilation, EF migrations and end-to-end Razorpay/Google testing could not be performed. Add real credentials and run the validation steps below before accepting payments. The frontend build was verified with Vite if noted in the delivered response.

## Project layout

```
frontend/                          React SPA (feature screens and centralized API client)
backend/CourseForge.slnx          Visual Studio / dotnet solution (all four projects)
backend/src/CourseForge.Domain/     Domain entities
backend/src/CourseForge.Application/ Contracts and integration abstractions
backend/src/CourseForge.Infrastructure/ EF Core SQL Server, JWT, Google verification, Razorpay
backend/src/CourseForge.Api/        API controllers, auth, DI, middleware
backend/create-admin.sql           Promote an existing account to Admin
```

## Quick start

1. Install **.NET 10 SDK**, SQL Server / SQL Server Express, Node 20+ and optionally `dotnet-ef`:
   ```bash
   dotnet tool install --global dotnet-ef --version 10.0.0
   ```
2. Copy `backend/src/CourseForge.Api/appsettings.Local.example.json` to `backend/src/CourseForge.Api/appsettings.Local.json`. Set `ConnectionStrings:Default`, replace `Jwt:Key` with a securely generated random secret of at least 32 bytes. **Never commit secrets.** For production use secrets manager / environment variables.
3. From `backend/src/CourseForge.Api`:
   ```bash
   dotnet restore
   dotnet ef migrations add InitialCreate --project ../CourseForge.Infrastructure --startup-project . --output-dir Migrations
   dotnet ef database update --project ../CourseForge.Infrastructure --startup-project .
   dotnet run
   ```
   Local swagger: `http://localhost:5080/swagger`.
4. From `frontend`: copy `.env.example` to `.env`; then:
   ```bash
   npm install
   npm run dev
   ```
   Open `http://localhost:5173`.
5. Register a normal account, then use `backend/create-admin.sql` in SQL Server Management Studio to promote **that** account. Log out and log back in; JWT role is issued when you log in. Admin studio appears in navigation.
6. Admin studio: Create course → Add lesson → Add quiz questions to the lesson. For the OSI Model create 7 ordered lessons, then questions for each. Set **Skip quizzes** on a course to allow completion without question answers (learners still click Complete/Submit for each page).
7. Learners choose course → free enroll or Razorpay checkout → read each ordered lesson → submit a quiz → retry on failure → get certificate once all pages have passed.

## Secrets and integrations

### Google OAuth / Identity Services
- Google Cloud Console: create OAuth **Web application** client, add `http://localhost:5173` to Authorized JavaScript origins.
- Set same Web Client ID in `frontend/.env` as `VITE_GOOGLE_CLIENT_ID` and in backend `Google:ClientId`.
- The browser sends Google ID credential to `POST /api/auth/google`. Backend verifies token audience/signature with Google library and issues its own JWT.
- To prevent unsafe automatic account linking, a Google sign-in email already registered via password is rejected unless linked through a verified account-linking flow (not part of starter).

### Razorpay
- Create Razorpay account, enable **Test Mode**, copy Test Key ID / secret to backend `Razorpay:KeyId` and `Razorpay:KeySecret`. **Never put KeySecret into frontend.**
- `POST /api/payments/courses/{id}/order` creates a server-side Razorpay order based on DB price. Frontend launches Razorpay Checkout using returned public `keyId`, amount and order ID.
- `POST /api/payments/verify` validates Razorpay HMAC server-side before enrolling. For asynchronous payment reconciliation, configure webhook at `https://YOUR_API_HOST/api/payments/webhook`, subscribing to `payment.captured` and `order.paid`, and set `Razorpay:WebhookSecret`.
- The webhook signature is validated against the **raw payload**, with idempotent enrollment and price check. A production deployment should additionally implement Razorpay payments API reconciliation for suspicious or pending transactions, verified payment currency, persistent webhook event log, refunds and reconciliation jobs.

## Primary endpoints

| Action | Method / route | Access |
|---|---|---|
| Register/login/Google | POST `/api/auth/register`, `/login`, `/google` | Public |
| Browse courses | GET `/api/courses` | Public |
| Detail | GET `/api/courses/{id}` | Public |
| Enroll free | POST `/api/courses/{id}/enroll` | JWT |
| Read page | GET `/api/courses/{id}/learn/{lessonId}` | Enrolled JWT |
| Grade page quiz | POST `/api/courses/{id}/learn/{lessonId}/submit` | Enrolled JWT |
| My learning | GET `/api/courses/my/learning` | JWT |
| Payment order/verify | POST `/api/payments/courses/{id}/order`, `/api/payments/verify` | JWT |
| Webhook | POST `/api/payments/webhook` | Signed Razorpay |
| Certificate download | GET `/api/certificates/{id}/download` | Owner/admin JWT |
| Certificate lookup | GET `/api/certificates/verify/{code}` | Public |
| Admin studio | `/api/admin/...` | Admin JWT |

## Important functional rules
- Quizzes are graded **on the server**. Correct option indices are never sent to the learner.
- The next lesson URL is blocked by the backend until the prior lesson is marked passed.
- Incorrect attempts increment and learners can retry without attempt limits.
- `SkipQuizzes=true` means marking a lesson complete with Submit; `0` questions also allows completion.
- Certificate issuance after all lessons are passed has a unique user/course index to prevent duplication.
- Paid enrollment is created only after Razorpay server-side signature verification or verified payment webhook; never trust frontend success alone.

## Production hardening checklist

- [ ] Run `dotnet build` and `dotnet ef database update`; add EF migration to source control.
- [ ] End-to-end browser and integration tests for Google sign-in, payment success/failures, webhook retries, authoring and certificate download.
- [ ] Host on HTTPS with trusted proxy and correct ASP.NET Core forwarding headers.
- [ ] Rotate secrets; use Azure Key Vault / other managed secret store; prevent logs from containing credentials or auth tokens.
- [ ] Add refresh tokens or short-session strategy, logout revocation, email verification, password reset, rate limiting and account lockout.
- [ ] Add robust payment reconciliation, refunds, webhook event journal and transaction-safe concurrency for high volume.
- [ ] Escape/sanitize untrusted markdown content as appropriate; moderate image URLs, content uploads, protect against phishing / XSS.
- [ ] Add admin content editing and removal in the UI (update endpoints for course and lesson are included), richer quiz/question management, draft previews, image storage and pagination.
- [ ] Certificate download is **SVG**, which can be opened/printed or converted to PDF; a native PDF exporter has not been implemented.
- [ ] Do not change quiz requirements midway through active paid cohorts without defining an explicit versioning policy.

## Notes on architecture
The backend separates domain, application contracts, infrastructure and transport controllers. It is a **clean-architecture-inspired starter**, not a fully strict clean architecture: controllers directly query EF context for simplicity. The frontend centralizes API/auth and separates screens, but is not fully feature-sliced yet. For a large production build, add application use cases, repository abstractions and frontend feature folders/tests.

## Open the .NET solution (.slnx)

The backend contains `CourseForge.slnx` referencing the Domain, Application, Infrastructure and API projects. Open `backend/CourseForge.slnx` in a compatible Visual Studio version, or run:

```bash
cd backend
dotnet restore CourseForge.slnx
dotnet build CourseForge.slnx
dotnet run --project src/CourseForge.Api/CourseForge.Api.csproj
```

Use the .NET 10 SDK and a Visual Studio version that supports the XML `.slnx` format.
"# CourseForge-FullStack-Learning-Platform" 

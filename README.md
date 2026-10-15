# CourseForge – Full-Stack E-Learning Platform

**CourseForge** is a responsive, full-stack e-learning platform built using **React.js, ASP.NET Core Web API, Clean Architecture, and SQL Server**.

Unlike traditional video-based learning platforms, CourseForge focuses on **structured, text-based learning**, allowing students to learn through detailed lessons, complete quizzes, track their progress, and earn certificates.

The platform includes secure authentication, course management, Razorpay payment integration, and role-based access for administrators and students.

## 🚀 Features

### 👨‍🎓 Student Features

- Browse and explore available courses.
- Register and log in securely.
- Sign in using Google authentication.
- Enroll in free courses.
- Purchase paid courses using Razorpay.
- Learn through structured Markdown-based lessons.
- Access lessons sequentially.
- Complete quizzes after individual lessons.
- Retry quizzes after unsuccessful attempts.
- Unlock subsequent lessons after passing.
- Track course completion and learning progress.
- Download certificates after successfully completing courses.
- Verify certificates using unique verification codes.

### 👨‍💼 Admin Features

- Role-based admin authentication.
- Create and manage courses.
- Configure course titles, descriptions, categories, and prices.
- Publish courses for students.
- Add and organize lessons in a specific order.
- Create multiple-choice quizzes for lessons.
- Configure quiz passing percentages.
- Enable or disable quiz requirements.
- Manage course learning content using Markdown.

## 🛠️ Technology Stack

| Layer | Technology |
|---|---|
| Frontend | React.js, JavaScript, Vite |
| Backend | ASP.NET Core 10 Web API |
| Programming Language | C# |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Architecture | Clean Architecture |
| Authentication | JWT, Google Authentication |
| Authorization | Role-Based Access Control |
| Payment Gateway | Razorpay |
| API Documentation | Swagger / OpenAPI |
| Learning Content | Markdown |
| Certificates | SVG Certificate Generation |

## 🏗️ Clean Architecture

CourseForge follows a layered architecture to maintain separation of concerns, improve maintainability, and support future scalability.

### Backend Architecture

```text
CourseForge.Backend
│
├── CourseForge.Domain
│   └── Domain Entities
│
├── CourseForge.Application
│   └── Interfaces and Application Contracts
│
├── CourseForge.Infrastructure
│   ├── Entity Framework Core
│   ├── Database Context
│   ├── JWT Token Service
│   ├── Google Authentication
│   └── Razorpay Integration
│
└── CourseForge.Api
    ├── Controllers
    ├── Dependency Injection
    ├── Authentication Configuration
    └── API Endpoints
```

### Frontend Architecture

The React frontend provides a responsive interface for browsing courses, authentication, lesson reading, quiz participation, student progress tracking, and course administration.

It communicates with the ASP.NET Core backend through REST APIs.

## 📚 Course Learning Workflow

CourseForge provides a sequential, page-based learning experience.

```text
Browse Courses
      |
      v
Select Course
      |
      v
Enroll / Purchase
      |
      v
Read Lesson 1
      |
      v
Attempt Quiz
      |
      +---- Failed ----> Retry Quiz
      |
      v
     Passed
      |
      v
Unlock Next Lesson
      |
      v
Complete All Lessons
      |
      v
Generate Certificate
      |
      v
Download Certificate
```

Administrators can configure courses to skip quiz requirements when needed.

## 📝 Quiz and Progress Tracking

Each course contains multiple lessons arranged in a defined order.

- Lessons can contain multiple-choice questions.
- Students must achieve the configured passing percentage.
- Failed quizzes can be attempted again.
- Successful completion unlocks the next lesson.
- Quiz attempts and lesson completion are stored in SQL Server.
- Course progress is calculated from completed lessons.
- Certificates are issued after all required lessons are completed.

## 🔐 Authentication and Authorization

CourseForge supports two authentication methods:

**JWT Authentication**

Students and administrators can register or log in using email and password. The backend generates JWT access tokens for authenticated requests.

**Google Authentication**

Users can sign in using Google credentials, which are verified by the ASP.NET Core backend.

Role-based authorization separates student functionality from administrative operations.

## 💳 Razorpay Payment Integration

CourseForge supports free and paid courses.

For paid courses, Razorpay is integrated into the enrollment workflow.

The payment integration includes:

- Razorpay order creation.
- Payment signature verification.
- Webhook signature verification.
- Purchase record management.
- Enrollment after successful payment processing.

Production deployment requires additional payment-status validation, idempotency safeguards, and end-to-end testing.

## 🎓 Certificate Generation

After completing all required lessons, students receive a certificate of completion.

Certificates include:

- Student name.
- Completed course title.
- Certificate issue date.
- Unique certificate verification code.

Certificates can be downloaded in SVG format and verified through a public API endpoint.

## 🗄️ Database Entities

The application uses Entity Framework Core with SQL Server.

Core entities include:

| Entity | Purpose |
|---|---|
| User | Stores student and administrator accounts |
| Course | Stores course information and configuration |
| Lesson | Stores ordered Markdown learning content |
| Question | Stores multiple-choice quiz questions |
| Enrollment | Tracks student course enrollments |
| LessonProgress | Tracks quiz attempts and lesson completion |
| Purchase | Stores Razorpay purchase information |
| Certificate | Stores issued certificates and verification codes |

## 🌟 Project Highlights

- Full-stack development with React and ASP.NET Core.
- Clean Architecture with separate application layers.
- RESTful API development.
- SQL Server database integration using EF Core.
- JWT authentication and role-based authorization.
- Google sign-in integration.
- Razorpay payment gateway integration.
- Dynamic Markdown-based course content.
- Sequential lesson unlocking.
- Server-side quiz validation.
- Persistent learning progress tracking.
- Automated certificate generation.
- Responsive student and admin interfaces.

## 🔮 Future Enhancements

- PDF certificate generation.
- Course reviews and ratings.
- Student discussion forums.
- Course search and advanced filtering.
- Instructor accounts and dashboards.
- Email notifications.
- Refresh-token authentication.
- Learning analytics.
- Course wishlists.
- Cloud deployment and CI/CD automation.

---

**CourseForge** — *Learn at your own pace. Practice your knowledge. Earn your achievement.*

Built with React.js, ASP.NET Core Web API, Entity Framework Core, and SQL Server.

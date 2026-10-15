import React, { useState, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Routes, Route, Navigate, Link, useNavigate, useParams } from 'react-router-dom';
import { GoogleOAuthProvider, GoogleLogin } from '@react-oauth/google';
import { BookOpen, GraduationCap, ArrowRight, ShieldCheck, Menu, X, CheckCircle2, Clock3, LockKeyhole, Plus, LogOut, ChevronRight, FileText, LayoutDashboard, Award, Search, Sparkles, Layers, ArrowLeft, CreditCard, Check } from 'lucide-react';
import ReactMarkdown from 'react-markdown';
import { api, session, errorText } from './lib/api';
import './style.css';


function App() {
  const [user, setUser] = useState(session.get());

  useEffect(() => {
    const handleAuthChange = () => {
      setUser(session.get());
    };

    window.addEventListener("cf-auth", handleAuthChange);

    return () => {
      window.removeEventListener("cf-auth", handleAuthChange);
    };
  }, []);

  return (
    <BrowserRouter>
      <Header user={user} />

      <main>
        <Routes>
          {/* Public Routes */}
          <Route path="/" element={<Home />} />

          <Route path="/login" element={<Auth />} />

          <Route path="/courses/:id" element={<Course />} />

          {/* Protected Learning Route */}
          <Route
            path="/learn/:id/:lesson"
            element={
              <Protected>
                <Learn />
              </Protected>
            }
          />

          {/* Student Dashboard */}
          <Route
            path="/dashboard"
            element={
              <Protected>
                <Dashboard />
              </Protected>
            }
          />

          {/* Certificates */}
          <Route
            path="/certificates"
            element={
              <Protected>
                <Certificates />
              </Protected>
            }
          />

          {/* Admin Dashboard */}
          <Route
            path="/admin"
            element={
              <Protected admin>
                <Admin />
              </Protected>
            }
          />

          {/* Fallback Route */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>

      <Footer />
    </BrowserRouter>
  );
}



 function Header({ user }) {
  const [menu, setMenu] = useState(false);
  const nav = useNavigate();

  return (
    <header className="header">
      <div className="container nav">

        {/* Brand Logo */}
        <Link to="/" className="brand">
          <div className="brandmark">
            <BookOpen size={22} />
          </div>

          course<span>forge</span>
          <span className="branddot">.</span>
        </Link>

        {/* Navigation Links */}
        <nav className={menu ? "navlinks open" : "navlinks"}>

          <Link
            to="/"
            onClick={() => setMenu(false)}
          >
            Explore courses
          </Link>

          {user && (
            <Link
              to="/dashboard"
              onClick={() => setMenu(false)}
            >
              My learning
            </Link>
          )}

          {user && (
            <Link
              to="/certificates"
              onClick={() => setMenu(false)}
            >
              Certificates
            </Link>
          )}

          {user?.role === "Admin" && (
            <Link
              to="/admin"
              onClick={() => setMenu(false)}
            >
              Admin studio
            </Link>
          )}

        </nav>

        {/* Right Side Navigation */}
        <div className="navright">

          {user ? (
            <>
              {/* Logged-in User Greeting */}
              <span className="greeting">
                Hi, {user.name?.split(" ")[0]}
              </span>

              {/* Logout Button */}
              <button
                type="button"
                className="iconbutton"
                title="Log out"
                onClick={() => {
                  session.clear();
                  nav("/");
                }}
              >
                <LogOut size={18} />
              </button>
            </>
          ) : (
            <>
              {/* Login */}
              <Link
                className="signin"
                to="/login"
              >
                Log in
              </Link>

              {/* Get Started */}
              <Link
                className="btn btn-dark btn-small"
                to="/login"
              >
                Get started
                <ArrowRight size={15} />
              </Link>
            </>
          )}

          {/* Mobile Menu Toggle */}
          <button
            type="button"
            className="iconbutton mobile-menu"
            aria-label={menu ? "Close menu" : "Open menu"}
            aria-expanded={menu}
            onClick={() => setMenu(!menu)}
          >
            {menu ? <X /> : <Menu />}
          </button>

        </div>
      </div>
    </header>
  );
}


 function Footer() {
  return (
    <footer className="footer">
      <div className="container footerin">

        {/* Brand Logo */}
        <div className="brand">
          <div className="brandmark">
            <BookOpen size={18} />
          </div>

          course<span>forge</span>
          <span className="branddot">.</span>
        </div>

        {/* Footer Tagline */}
        <span>
          Learn deeply. Prove what you know.
        </span>

        {/* Copyright */}
        <span>
          © {new Date().getFullYear()} CourseForge
        </span>

      </div>
    </footer>
  );
}



 const sample = [
  {
    id: "s1",
    title: "Understanding the OSI Model",
    summary:
      "From physical cables to application protocols, master all seven layers through hands-on reading.",
    category: "Networking",
    priceInr: 0,
    lessonCount: 7,
    coverUrl: "",
  },
  {
    id: "s2",
    title: "SQL Server Essentials",
    summary:
      "Write better SQL with a guided path from joins to indexes and transactions.",
    category: "Databases",
    priceInr: 499,
    lessonCount: 12,
    coverUrl: "",
  },
  {
    id: "s3",
    title: "ASP.NET Core API Mastery",
    summary:
      "Build secure, scalable APIs with modern architectural patterns.",
    category: "Development",
    priceInr: 799,
    lessonCount: 18,
    coverUrl: "",
  },
];


function Home() {
  const [courses, setCourses] = useState([]);
  const [search, setSearch] = useState("");
  const [cat, setCat] = useState("All");

  useEffect(() => {
    api
      .get("/courses")
      .then((r) => setCourses(r.data))
      .catch(() => {});
  }, []);

  const shown = courses.filter(
    (c) =>
      (cat === "All" || c.category === cat) &&
      (c.title + " " + c.summary)
        .toLowerCase()
        .includes(search.toLowerCase())
  );

  const categories = [
    "All",
    ...new Set(courses.map((c) => c.category)),
  ];

  return (
    <>
      <section className="hero">
        <div className="container hero-grid">
          <div>
            <div className="eyebrow">
              <Sparkles size={14} />
              A smarter way to learn
            </div>

            <h1>
              Small lessons.
              <br />
              <em>Big breakthroughs.</em>
            </h1>

            <p>
              Skip endless videos. Learn through beautifully written lessons,
              test your knowledge with bite-sized quizzes, and earn
              certificates that celebrate your growth.
            </p>

            <div className="hero-actions">
              <a className="btn btn-primary" href="#explore">
                Explore courses
                <ArrowRight size={18} />
              </a>

              <div className="socialproof">
                <div className="avatars">
                  <b>A</b>
                  <b>M</b>
                  <b>S</b>
                </div>

                <span>Learn on your terms</span>
              </div>
            </div>

            <div className="hero-statrow">
              <span>
                <CheckCircle2 size={17} />
                Bite-sized lessons
              </span>

              <span>
                <CheckCircle2 size={17} />
                Knowledge checks
              </span>

              <span>
                <CheckCircle2 size={17} />
                Completion certificates
              </span>
            </div>
          </div>

          <div className="hero-visual">
            <div className="float-chip chip-one">
              <span className="chip-icon green">
                <Check size={18} />
              </span>

              <span>
                <b>Lesson complete!</b>
                <small>Keep up the momentum</small>
              </span>
            </div>

            <div className="lesson-preview">
              <div className="preview-head">
                <span className="preview-icon">
                  <Layers size={24} />
                </span>

                <span>
                  <small>NETWORKING FUNDAMENTALS</small>
                  <b>Mastering the OSI Model</b>
                </span>

                <span className="preview-dots">•••</span>
              </div>

              <div className="progress-head">
                <span>Learning progress</span>
                <b>4 of 7 lessons</b>
              </div>

              <div className="progress">
                <span style={{ width: "57%" }} />
              </div>

              <div className="preview-sep" />

              <small className="mini-label">UP NEXT</small>

              <div className="preview-topic">
                <span className="topic-no">05</span>

                <span>
                  <b>Session Layer</b>
                  <small>Manage communication sessions</small>
                </span>

                <ChevronRight size={20} />
              </div>

              <div className="preview-topic muted">
                <span className="topic-no">06</span>

                <span>
                  <b>Presentation Layer</b>
                  <small>Translate and encrypt data</small>
                </span>

                <LockKeyhole size={17} />
              </div>

              <div className="preview-action">
                Continue learning
                <ArrowRight size={16} />
              </div>
            </div>

            <div className="float-chip chip-two">
              <span className="chip-icon yellow">
                <Award size={18} />
              </span>

              <span>
                <b>Earn certificates</b>
                <small>Show what you've learned</small>
              </span>
            </div>
          </div>
        </div>
      </section>

      <section className="features container">
        <div>
          <span className="featureicon purple">
            <BookOpen />
          </span>

          <h3>Read. Understand. Grow.</h3>

          <p>
            Focused written lessons help you learn without distractions.
          </p>
        </div>

        <div>
          <span className="featureicon peach">
            <ShieldCheck />
          </span>

          <h3>Practice until it clicks.</h3>

          <p>
            Quick quizzes reinforce knowledge before you move on.
          </p>
        </div>

        <div>
          <span className="featureicon mint">
            <Award />
          </span>

          <h3>Show your achievement.</h3>

          <p>
            Collect certificates that mark every milestone.
          </p>
        </div>
      </section>

      <section id="explore" className="catalog container">
        <div className="sectiontop">
          <div>
            <div className="eyebrow">
              THE COURSE LIBRARY
            </div>

            <h2>
              Find your next <em>aha!</em> moment.
            </h2>

            <p>
              Explore practical, self-paced courses made for curious minds.
            </p>
          </div>

          <div className="searchbox">
            <Search size={19} />

            <input
              placeholder="Search courses..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>

        <div className="filterrow">
          {categories.map((c) => (
            <button
              className={cat === c ? "filter active" : "filter"}
              onClick={() => setCat(c)}
              key={c}
            >
              {c}
            </button>
          ))}
        </div>

        <div className="course-grid">
          {shown.map((c, i) => (
            <CourseCard
              c={c}
              key={c.id}
              i={i}
            />
          ))}

          {!shown.length && (
            <div className="empty">
              No published courses found. An admin can create and
              publish one in Admin studio.
            </div>
          )}
        </div>
      </section>

      <section className="cta container">
        <div>
          <div className="eyebrow">
            YOUR NEXT CHAPTER STARTS HERE
          </div>

          <h2>
            A little learning every day
            <br />
            goes a long way.
          </h2>

          <p>
            Progress at your own pace and make every session count.
          </p>
        </div>

        <Link className="btn btn-white" to="/login">
          Start learning
          <ArrowRight size={18} />
        </Link>
      </section>
    </>
  );
}



function CourseCard({ c, i }) {
  return (
    <Link
      to={"/courses/" + c.id}
      className="course-card"
    >
      <div className={"card-art art-" + (i % 4)}>
        {c.coverUrl ? (
          <img src={c.coverUrl} />
        ) : (
          <>
            <div className="art-circle" />

            <div className="art-tile">
              <BookOpen size={40} />
            </div>
          </>
        )}

        <span className="card-badge">
          {c.category}
        </span>
      </div>

      <div className="card-content">
        <div className="course-meta">
          <span>
            <FileText size={15} />
            {c.lessonCount} lessons
          </span>

          <span>
            <Clock3 size={15} />
            Self-paced
          </span>
        </div>

        <h3>{c.title}</h3>

        <p>{c.summary}</p>

        <div className="card-foot">
          <strong>
            {c.priceInr === 0
              ? "Free"
              : "₹" + c.priceInr}
          </strong>

          <span>
            Explore course
            <ArrowRight size={16} />
          </span>
        </div>
      </div>
    </Link>
  );
}



function Auth() {
  const [mode, setMode] = useState("login");
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const nav = useNavigate();

  async function auth(e) {
    e.preventDefault();
    setBusy(true);
    setError("");

    try {
      const res = await api.post(
        "/auth/" + (mode === "login" ? "login" : "register"),
        mode === "login"
          ? { email, password }
          : { name, email, password }
      );

      session.set(res.data);
      nav("/dashboard");
    } catch (e) {
      setError(errorText(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="auth-bg">
      <div className="auth-panel">
        <div className="auth-icon">
          <GraduationCap size={32} />
        </div>

        <h1>
          {mode === "login" ? "Welcome back" : "Create your account"}
        </h1>

        <p>
          {mode === "login"
            ? "Your next lesson is waiting."
            : "Make learning your favorite habit."}
        </p>

        <div className="auth-tabs">
          <button
            className={mode === "login" ? "selected" : ""}
            onClick={() => setMode("login")}
          >
            Log in
          </button>

          <button
            className={mode === "register" ? "selected" : ""}
            onClick={() => setMode("register")}
          >
            Sign up
          </button>
        </div>

        {error && <div className="alert">{error}</div>}

        <form onSubmit={auth} className="form">
          {mode === "register" && (
            <label>
              Full name
              <input
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Your name"
              />
            </label>
          )}

          <label>
            Email address
            <input
              required
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
            />
          </label>

          <label>
            Password
            <input
              required
              type="password"
              minLength={mode === "register" ? 8 : 1}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
            />
          </label>

          <button
            disabled={busy}
            className="btn btn-primary full"
          >
            {busy
              ? "Please wait..."
              : mode === "login"
                ? "Log in"
                : "Create account"}

            <ArrowRight size={17} />
          </button>
        </form>

        <div className="or">
          <span>or continue with</span>
        </div>

        {import.meta.env.VITE_GOOGLE_CLIENT_ID ? (
          <div className="google">
            <GoogleLogin
              onSuccess={async (response) => {
                try {
                  let r = await api.post("/auth/google", {
                    credential: response.credential,
                  });

                  session.set(r.data);
                  nav("/dashboard");
                } catch (e) {
                  setError(errorText(e));
                }
              }}
              onError={() => setError("Google login failed")}
            />
          </div>
        ) : (
          <p className="config-hint">
            Google sign-in appears after configuring
            VITE_GOOGLE_CLIENT_ID.
          </p>
        )}

        <p className="auth-note">
          Learn with confidence, one page at a time.
        </p>
      </div>
    </div>
  );
}


function Protected({ children, admin = false }) {
  const user = session.get();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (admin && user.role !== "Admin") {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
}



function Course() {
  const { id } = useParams();
  const nav = useNavigate();

  const [c, setC] = useState(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const refresh = () =>
    api
      .get("/courses/" + id)
      .then((r) => setC(r.data))
      .catch((e) => setError(errorText(e)));

  useEffect(refresh, [id]);

  async function buy() {
    if (!session.get()) {
      return nav("/login");
    }

    setBusy(true);
    setError("");

    try {
      if (c.priceInr === 0) {
        await api.post("/courses/" + id + "/enroll");
        refresh();
        return;
      }

      const res = (
        await api.post("/payments/courses/" + id + "/order")
      ).data;

      if (!window.Razorpay) {
        throw Error("Razorpay checkout script not loaded");
      }

      const rz = new window.Razorpay({
        key: res.keyId,
        amount: res.amount,
        currency: "INR",
        name: "CourseForge",
        description: res.courseTitle,
        order_id: res.orderId,

        handler: async (response) => {
          try {
            await api.post("/payments/verify", {
              courseId: id,
              orderId: response.razorpay_order_id,
              paymentId: response.razorpay_payment_id,
              signature: response.razorpay_signature,
            });

            refresh();
          } catch (e) {
            setError(errorText(e));
          }
        },

        prefill: {
          email: session.get()?.email,
        },

        theme: {
          color: "#5c55e8",
        },
      });

      rz.on("payment.failed", () =>
        setError("Payment did not complete. Please retry.")
      );

      rz.open();
    } catch (e) {
      setError(errorText(e));
    } finally {
      setBusy(false);
    }
  }

  if (!c) {
    return (
      <div className="container loading">
        {error || "Loading course..."}
      </div>
    );
  }

  return (
    <div className="container detail">
      <Link to="/" className="back">
        <ArrowLeft size={16} />
        All courses
      </Link>

      <div className="detail-grid">
        <div>
          <span className="eyebrow">
            {c.category} / COURSE
          </span>

          <h1>{c.title}</h1>

          <p className="detail-summary">
            {c.summary}
          </p>

          <div className="detail-meta">
            <span>
              <FileText size={18} />
              {c.lessons.length} written lessons
            </span>

            <span>
              <ShieldCheck size={18} />
              {c.skipQuizzes
                ? "No quizzes required"
                : c.passingPercent + "% quiz score to progress"}
            </span>

            <span>
              <Award size={18} />
              Certificate on completion
            </span>
          </div>

          <div className="syllabus">
            <h2>What you'll learn</h2>

            <p>
              Move through each page in order. Your progress
              is saved automatically after passing each
              knowledge check.
            </p>

            {c.lessons.map((l, i) => (
              <div
                className="syllabus-item"
                key={l.id}
              >
                <span>
                  {String(i + 1).padStart(2, "0")}
                </span>

                <b>{l.title}</b>

                <FileText size={17} />
              </div>
            ))}

            {!c.lessons.length && (
              <div className="empty">
                Lessons are being prepared.
              </div>
            )}
          </div>
        </div>

        <aside className="enroll-card">
          <div className="enroll-art">
            <GraduationCap size={70} />
          </div>

          <div className="enroll-body">
            <span className="eyebrow">
              YOUR LEARNING JOURNEY
            </span>

            <h2>
              {c.priceInr === 0
                ? "Free"
                : "₹" + c.priceInr}
            </h2>

            {error && (
              <p className="alert">
                {error}
              </p>
            )}

            {c.enrolled ? (
              <Link
                className="btn btn-primary full"
                to={
                  c.lessons.length
                    ? `/learn/${id}/${c.lessons[0].id}`
                    : "/dashboard"
                }
              >
                Continue learning
                <ArrowRight size={17} />
              </Link>
            ) : (
              <button
                onClick={buy}
                disabled={busy}
                className="btn btn-primary full"
              >
                {busy
                  ? "Processing..."
                  : c.priceInr === 0
                    ? "Enroll for free"
                    : "Buy course"}

                <ArrowRight size={17} />
              </button>
            )}

            <div className="enroll-benefit">
              <CheckCircle2 size={17} />
              Lifetime access
            </div>

            <div className="enroll-benefit">
              <CheckCircle2 size={17} />
              Learn on any device
            </div>

            <div className="enroll-benefit">
              <CheckCircle2 size={17} />
              Certificate upon completion
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}




function Learn() {
  const { id, lesson } = useParams();

  const [l, setL] = useState(null);
  const [answers, setAnswers] = useState({});
  const [result, setResult] = useState(null);
  const [err, setErr] = useState("");
  const [busy, setBusy] = useState(false);

  const nav = useNavigate();

  useEffect(() => {
    setL(null);
    setResult(null);
    setAnswers({});

    api
      .get(`/courses/${id}/learn/${lesson}`)
      .then((r) => setL(r.data))
      .catch((e) => setErr(errorText(e)));
  }, [id, lesson]);

  async function submit() {
    setBusy(true);
    setErr("");

    try {
      const res = await api.post(
        `/courses/${id}/learn/${lesson}/submit`,
        { answers }
      );

      setResult(res.data);
    } catch (e) {
      setErr(errorText(e));
    } finally {
      setBusy(false);
    }
  }

  if (!l) {
    return (
      <div className="container loading">
        {err || "Loading lesson..."}
      </div>
    );
  }

  const done = l.passed || result?.passed;

  return (
    <div className="container learning">
      <div className="learn-bar">
        <Link to={"/courses/" + id} className="back">
          <ArrowLeft size={16} />
          Back to course
        </Link>

        <span>Lesson {l.position}</span>
      </div>

      <div className="learn-layout">
        <article className="lesson-article">
          <div className="eyebrow">
            READ • LEARN • PRACTICE
          </div>

          <h1>{l.title}</h1>

          <div className="markdown">
            <ReactMarkdown>
              {l.bodyMarkdown}
            </ReactMarkdown>
          </div>
        </article>

        <aside className="quiz-panel">
          <div className="quiz-head">
            <span className="featureicon purple">
              <ShieldCheck />
            </span>

            <h2>Knowledge check</h2>

            <p>
              Complete this quiz to unlock the next lesson.
              You can retry as often as needed.
            </p>
          </div>

          {l.questions.map((q, i) => (
            <div className="question" key={q.id}>
              <h3>
                {i + 1}. {q.prompt}
              </h3>

              {q.options?.map((o, j) => (
                <label
                  className={
                    "option " +
                    (answers[q.id] === j ? "chosen" : "")
                  }
                  key={j}
                >
                  <input
                    type="radio"
                    name={q.id}
                    checked={answers[q.id] === j}
                    disabled={!!done}
                    onChange={() =>
                      setAnswers({
                        ...answers,
                        [q.id]: j,
                      })
                    }
                  />

                  {o}
                </label>
              ))}
            </div>
          ))}

          {!l.questions.length && (
            <p className="config-hint">
              This lesson does not have any quiz questions.
            </p>
          )}

          {err && (
            <div className="alert">
              {err}
            </div>
          )}

          {result && (
            <div
              className={
                result.passed ? "successbox" : "alert"
              }
            >
              {result.passed
                ? "Passed! Great work."
                : "Not quite yet — give it another try."}{" "}
              {result.percent}% score · Attempt {result.attempts}
            </div>
          )}

          {done ? (
            <>
              {l.nextLessonId ? (
                <Link
                  className="btn btn-primary full"
                  to={`/learn/${id}/${l.nextLessonId}`}
                >
                  Next lesson
                  <ArrowRight size={17} />
                </Link>
              ) : (
                <Link
                  className="btn btn-primary full"
                  to="/certificates"
                >
                  View your certificate
                  <Award size={17} />
                </Link>
              )}
            </>
          ) : (
            <button
              className="btn btn-primary full"
              disabled={
                busy ||
                (l.questions.length > 0 &&
                  Object.keys(answers).length <
                    l.questions.length)
              }
              onClick={submit}
            >
              {busy ? "Checking..." : "Submit quiz"}
              <ArrowRight size={17} />
            </button>
          )}

          <span className="attempts">
            Attempts so far: {result?.attempts ?? l.attempts}
          </span>
        </aside>
      </div>
    </div>
  );
}


 
function Dashboard() {
  const [data, setData] = useState([]);

  useEffect(() => {
    api
      .get("/courses/my/learning")
      .then((r) => setData(r.data))
      .catch(() => {});
  }, []);

  return (
    <div className="container dash">
      <div className="eyebrow">
        YOUR LEARNING SPACE
      </div>

      <h1>
        Keep your momentum going <span>✦</span>
      </h1>

      <p>
        Every little lesson brings you closer to your goals.
      </p>

      <div className="sectiontop">
        <h2>My courses</h2>

        <Link className="back" to="/">
          Explore more
          <ArrowRight size={16} />
        </Link>
      </div>

      <div className="course-grid">
        {data.map((c, i) => (
          <Link
            className="course-card dash-card"
            key={c.id}
            to={"/courses/" + c.id}
          >
            <div className={"card-art art-" + (i % 4)}>
              <BookOpen size={46} />
            </div>

            <div className="card-content">
              <span className="eyebrow">
                {c.category}
              </span>

              <h3>{c.title}</h3>

              <div className="progress-head">
                <span>
                  {c.passed} / {c.total} lessons
                </span>

                <b>
                  {c.total
                    ? Math.round((c.passed * 100) / c.total)
                    : 0}
                  %
                </b>
              </div>

              <div className="progress">
                <span
                  style={{
                    width:
                      (c.total
                        ? (c.passed * 100) / c.total
                        : 0) + "%",
                  }}
                />
              </div>

              <div className="card-foot">
                <span>
                  {c.completed
                    ? "Completed"
                    : "Continue learning"}
                </span>

                <ArrowRight size={16} />
              </div>
            </div>
          </Link>
        ))}

        {!data.length && (
          <div className="empty">
            <BookOpen size={28} />

            <h3>Your library is waiting</h3>

            <p>
              Enroll in a course to start your learning journey.
            </p>

            <Link className="btn btn-primary" to="/">
              Browse courses
              <ArrowRight size={17} />
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}



function Certificates() {
  const [data, setData] = useState([]);

  useEffect(() => {
    api
      .get("/certificates/mine")
      .then((r) => setData(r.data))
      .catch(() => {});
  }, []);

  async function download(id) {
    try {
      const r = await api.get(
        `/certificates/${id}/download`,
        {
          responseType: "blob",
        }
      );

      const url = URL.createObjectURL(r.data);
      const a = document.createElement("a");

      a.href = url;
      a.download = `courseforge-certificate-${id}.svg`;
      a.click();

      URL.revokeObjectURL(url);
    } catch (e) {
      alert(errorText(e));
    }
  }

  return (
    <div className="container dash">
      <div className="eyebrow">
        YOUR ACHIEVEMENTS
      </div>

      <h1>
        Certificates <span>✦</span>
      </h1>

      <p>
        A record of what you've learned and accomplished.
      </p>

      <div className="cert-grid">
        {data.map((c) => (
          <div className="cert" key={c.id}>
            <Award size={42} />

            <div>
              <span className="eyebrow">
                CERTIFICATE OF COMPLETION
              </span>

              <h2>{c.title}</h2>

              <p>
                Issued{" "}
                {new Date(c.issuedUtc).toLocaleDateString()}
                {" · "}
                {c.code}
              </p>

              <button
                className="btn btn-primary btn-small"
                onClick={() => download(c.id)}
              >
                Download certificate
                <ArrowRight size={16} />
              </button>
            </div>
          </div>
        ))}

        {!data.length && (
          <div className="empty">
            Your certificates will appear here after you
            finish a course.
          </div>
        )}
      </div>
    </div>
  );
}



function Admin() {
  const [view, setView] = useState("overview");
  const [courses, setCourses] = useState([]);
  const [stats, setStats] = useState({});
  const [selected, setSelected] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const [course, setCourse] = useState({
    title: "",
    summary: "",
    category: "Technology",
    coverUrl: "",
    priceInr: 0,
    published: true,
    skipQuizzes: false,
    passingPercent: 70,
  });

  const [lesson, setLesson] = useState({
    title: "",
    bodyMarkdown: "",
    position: 1,
  });

  const [question, setQuestion] = useState({
    lessonId: "",
    prompt: "",
    options: "",
    correctOption: 0,
    explanation: "",
  });

  const load = () => {
    api.get("/admin/courses").then((r) => setCourses(r.data));
    api.get("/admin/stats").then((r) => setStats(r.data));
  };

  useEffect(load, []);

  async function save(fn) {
    setError("");
    setMessage("");

    try {
      await fn();
      setMessage("Saved successfully");
      load();
    } catch (e) {
      setError(errorText(e));
    }
  }

  return (
    <div className="container admin">
      <div className="admin-head">
        <div>
          <div className="eyebrow">CREATOR STUDIO</div>
          <h1>Admin dashboard</h1>
          <p>Build a learning experience worth finishing.</p>
        </div>

        <span className="admin-pill">
          <ShieldCheck size={17} />
          Administrator
        </span>
      </div>

      <div className="admin-tabs">
        {[
          ["overview", "Overview"],
          ["course", "Create course"],
          ["lesson", "Add lesson"],
          ["question", "Add quiz question"],
        ].map(([k, v]) => (
          <button
            className={view === k ? "active" : ""}
            onClick={() => setView(k)}
            key={k}
          >
            {v}
          </button>
        ))}
      </div>

      {message && (
        <div className="successbox">{message}</div>
      )}

      {error && (
        <div className="alert">{error}</div>
      )}

      {view === "overview" && (
        <>
          <div className="stat-grid">
            {[
              ["Courses", stats.courses, BookOpen],
              ["Learners", stats.students, GraduationCap],
              ["Enrollments", stats.enrollments, Layers],
              ["Certificates", stats.certificates, Award],
            ].map(([name, n, Icon]) => (
              <div className="stat" key={name}>
                <Icon />
                <span>{name}</span>
                <strong>{n ?? "—"}</strong>
              </div>
            ))}
          </div>

          <div className="admin-box">
            <h2>Courses</h2>

            {courses.map((c) => (
              <div className="admin-course" key={c.id}>
                <span>
                  <b>{c.title}</b>

                  <small>
                    {c.lessons} lessons ·{" "}
                    {c.published ? "Published" : "Draft"}
                  </small>
                </span>

                <span>₹{c.priceInr}</span>

                <button
                  className="btn btn-outline btn-small"
                  onClick={() => {
                    setSelected(c.id);
                    setView("lesson");
                  }}
                >
                  Add lesson
                  <Plus size={15} />
                </button>
              </div>
            ))}
          </div>
        </>
      )}

      {view === "course" && (
        <div className="admin-box admin-form">
          <h2>Create a course</h2>

          <p>
            Start with a title, description, and course access rules.
          </p>

          <div className="form">
            <label>
              Course title
              <input
                value={course.title}
                onChange={(e) =>
                  setCourse({
                    ...course,
                    title: e.target.value,
                  })
                }
                placeholder="e.g. Understanding the OSI Model"
              />
            </label>

            <label>
              Description
              <textarea
                rows="4"
                value={course.summary}
                onChange={(e) =>
                  setCourse({
                    ...course,
                    summary: e.target.value,
                  })
                }
              />
            </label>

            <div className="two">
              <label>
                Category
                <input
                  value={course.category}
                  onChange={(e) =>
                    setCourse({
                      ...course,
                      category: e.target.value,
                    })
                  }
                />
              </label>

              <label>
                Price (INR, 0 = free)
                <input
                  type="number"
                  min="0"
                  value={course.priceInr}
                  onChange={(e) =>
                    setCourse({
                      ...course,
                      priceInr: Number(e.target.value),
                    })
                  }
                />
              </label>
            </div>

            <label>
              Cover image URL (optional)
              <input
                value={course.coverUrl}
                onChange={(e) =>
                  setCourse({
                    ...course,
                    coverUrl: e.target.value,
                  })
                }
              />
            </label>

            <label>
              Quiz passing percentage
              <input
                type="number"
                min="1"
                max="100"
                value={course.passingPercent}
                onChange={(e) =>
                  setCourse({
                    ...course,
                    passingPercent: Number(e.target.value),
                  })
                }
              />
            </label>

            <label className="checkrow">
              <input
                type="checkbox"
                checked={course.skipQuizzes}
                onChange={(e) =>
                  setCourse({
                    ...course,
                    skipQuizzes: e.target.checked,
                  })
                }
              />
              Allow learners to skip quizzes
            </label>

            <label className="checkrow">
              <input
                type="checkbox"
                checked={course.published}
                onChange={(e) =>
                  setCourse({
                    ...course,
                    published: e.target.checked,
                  })
                }
              />
              Published and visible
            </label>

            <button
              className="btn btn-primary"
              onClick={() =>
                save(async () => {
                  let r = await api.post("/admin/courses", course);

                  setSelected(r.data.id);
                  setView("lesson");
                })
              }
            >
              Create course
              <ArrowRight size={16} />
            </button>
          </div>
        </div>
      )}

      {view === "lesson" && (
        <div className="admin-box admin-form">
          <h2>Add a reading lesson</h2>

          <p>
            Write lesson content using Markdown: headings, bullet
            points, and code examples.
          </p>

          <div className="form">
            <label>
              Course
              <select
                value={selected}
                onChange={(e) => setSelected(e.target.value)}
              >
                <option value="">Select course</option>

                {courses.map((c) => (
                  <option value={c.id} key={c.id}>
                    {c.title}
                  </option>
                ))}
              </select>
            </label>

            <label>
              Lesson title
              <input
                value={lesson.title}
                onChange={(e) =>
                  setLesson({
                    ...lesson,
                    title: e.target.value,
                  })
                }
              />
            </label>

            <label>
              Position
              <input
                type="number"
                min="1"
                value={lesson.position}
                onChange={(e) =>
                  setLesson({
                    ...lesson,
                    position: Number(e.target.value),
                  })
                }
              />
            </label>

            <label>
              Content (Markdown)
              <textarea
                className="editor"
                value={lesson.bodyMarkdown}
                onChange={(e) =>
                  setLesson({
                    ...lesson,
                    bodyMarkdown: e.target.value,
                  })
                }
                placeholder={
                  "# The Physical Layer\n\nThe physical layer sends **bits** across a medium..."
                }
              />
            </label>

            <button
              className="btn btn-primary"
              disabled={!selected}
              onClick={() =>
                save(async () => {
                  let r = await api.post(
                    `/admin/courses/${selected}/lessons`,
                    lesson
                  );

                  setQuestion({
                    ...question,
                    lessonId: r.data.id,
                  });

                  setLesson({
                    title: "",
                    bodyMarkdown: "",
                    position: lesson.position + 1,
                  });

                  setView("question");
                })
              }
            >
              Save lesson
              <ArrowRight size={16} />
            </button>
          </div>
        </div>
      )}

      {view === "question" && (
        <div className="admin-box admin-form">
          <h2>Add a quiz question</h2>

          <p>
            Add multiple-choice questions. The correct answer
            index starts at 0.
          </p>

          <div className="form">
            <label>
              Lesson ID (saved when a lesson is created)
              <input
                value={question.lessonId}
                onChange={(e) =>
                  setQuestion({
                    ...question,
                    lessonId: e.target.value,
                  })
                }
                placeholder="GUID of lesson"
              />
            </label>

            <label>
              Question
              <input
                value={question.prompt}
                onChange={(e) =>
                  setQuestion({
                    ...question,
                    prompt: e.target.value,
                  })
                }
              />
            </label>

            <label>
              Answer options (one per line)
              <textarea
                rows="5"
                value={question.options}
                onChange={(e) =>
                  setQuestion({
                    ...question,
                    options: e.target.value,
                  })
                }
                placeholder={
                  "Physical layer\nData link layer\nNetwork layer\nTransport layer"
                }
              />
            </label>

            <label>
              Correct option index (0 = first)
              <input
                type="number"
                min="0"
                value={question.correctOption}
                onChange={(e) =>
                  setQuestion({
                    ...question,
                    correctOption: Number(e.target.value),
                  })
                }
              />
            </label>

            <label>
              Explanation
              <input
                value={question.explanation}
                onChange={(e) =>
                  setQuestion({
                    ...question,
                    explanation: e.target.value,
                  })
                }
              />
            </label>

            <button
              className="btn btn-primary"
              onClick={() =>
                save(async () => {
                  await api.post(
                    `/admin/lessons/${question.lessonId}/questions`,
                    {
                      ...question,
                      options: question.options
                        .split("\n")
                        .map((x) => x.trim())
                        .filter(Boolean),
                    }
                  );

                  setQuestion({
                    ...question,
                    prompt: "",
                    options: "",
                    correctOption: 0,
                    explanation: "",
                  });
                })
              }
            >
              Save question
              <Plus size={16} />
            </button>
          </div>
        </div>
      )}
    </div>
  );
}



createRoot(document.getElementById("root")).render(
  <GoogleOAuthProvider
    clientId={
      import.meta.env.VITE_GOOGLE_CLIENT_ID || "not-configured"
    }
  >
    <App />
  </GoogleOAuthProvider>
);

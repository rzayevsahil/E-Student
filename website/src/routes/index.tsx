import { createFileRoute } from "@tanstack/react-router";
import logo from "@/assets/logo.png";
import { Reveal } from "@/components/Reveal";
import { WindowFrame } from "@/components/app-ui/WindowFrame";
import { SearchScreen } from "@/components/app-ui/SearchScreen";
import { PomodoroScreen } from "@/components/app-ui/PomodoroScreen";

const DOWNLOAD_URL = "https://github.com/rzayevsahil/E-Student/releases/latest";
const GITHUB_URL = "https://github.com/rzayevsahil/E-Student";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "E-Student — Find your study materials in seconds" },
      {
        name: "description",
        content:
          "E-Student is a free Windows desktop app that searches inside your PDF, Word and Excel study materials and keeps you focused with a Pomodoro timer.",
      },
      { property: "og:title", content: "E-Student — Find your study materials in seconds" },
      {
        property: "og:description",
        content:
          "Search across all your PDF, Word and Excel documents from one place. Download E-Student for Windows.",
      },
    ],
  }),
  component: Index,
});

function WindowsIcon({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 88 88" aria-hidden className={className} fill="currentColor">
      <path d="M0 12.4 35.7 7.5v34.4H0zM40 6.9 87.6 0v41.6H40zM35.7 46.1v34.5L0 75.7V46.1zM87.6 46.1V88L40 81.2V46.1z" />
    </svg>
  );
}

function GithubIcon({ className = "" }: { className?: string }) {
  return (
    <svg viewBox="0 0 24 24" aria-hidden className={className} fill="currentColor">
      <path d="M12 .5C5.7.5.6 5.6.6 11.9c0 5 3.3 9.3 7.8 10.8.6.1.8-.2.8-.6v-2c-3.2.7-3.9-1.5-3.9-1.5-.5-1.3-1.3-1.7-1.3-1.7-1-.7.1-.7.1-.7 1.1.1 1.7 1.2 1.7 1.2 1 1.8 2.7 1.2 3.4.9.1-.7.4-1.2.7-1.5-2.6-.3-5.3-1.3-5.3-5.7 0-1.3.4-2.3 1.2-3.1-.1-.3-.5-1.5.1-3.1 0 0 1-.3 3.2 1.2a11 11 0 0 1 5.8 0c2.2-1.5 3.2-1.2 3.2-1.2.6 1.6.2 2.8.1 3.1.8.8 1.2 1.8 1.2 3.1 0 4.4-2.7 5.4-5.3 5.7.4.4.8 1.1.8 2.2v3.3c0 .4.2.7.8.6a11.4 11.4 0 0 0 7.8-10.8C23.4 5.6 18.3.5 12 .5Z" />
    </svg>
  );
}

function Btn({
  href,
  variant = "primary",
  children,
  size = "md",
}: {
  href: string;
  variant?: "primary" | "outline" | "ghost";
  children: React.ReactNode;
  size?: "md" | "lg";
}) {
  const styles = {
    primary:
      "bg-brand text-accent-foreground shadow-[0_8px_20px_-8px_var(--brand)] hover:brightness-[1.06] hover:-translate-y-0.5",
    outline:
      "border border-navy/20 bg-card text-navy hover:border-navy/40 hover:-translate-y-0.5",
    ghost: "border border-white/25 text-white hover:bg-white/10 hover:-translate-y-0.5",
  }[variant];
  const pad = size === "lg" ? "px-7 py-4 text-base" : "px-5 py-3 text-sm";
  return (
    <a
      href={href}
      target="_blank"
      rel="noreferrer"
      className={`inline-flex items-center justify-center gap-2.5 rounded-md font-semibold transition-all duration-200 ${pad} ${styles}`}
    >
      {children}
    </a>
  );
}

function Nav() {
  return (
    <header className="sticky top-0 z-50 border-b border-border/80 bg-background/85 backdrop-blur">
      <div className="mx-auto flex h-16 w-full max-w-6xl items-center gap-4 px-5">
        <a href="#top" className="flex items-center gap-2">
          <img src={logo} alt="E-Student logo" className="h-9 w-9 object-contain" />
          <span className="font-display text-lg font-extrabold text-navy">E-Student</span>
        </a>
        <nav className="ml-auto hidden items-center gap-7 text-sm font-medium text-navy-500 sm:flex">
          <a href="#features" className="transition-colors hover:text-brand">
            Features
          </a>
          <a href="#screenshots" className="transition-colors hover:text-brand">
            Screenshots
          </a>
          <a href="#download" className="transition-colors hover:text-brand">
            Download
          </a>
        </nav>
        <div className="ml-auto sm:ml-0">
          <Btn href={DOWNLOAD_URL}>Download</Btn>
        </div>
      </div>
    </header>
  );
}

function Hero() {
  return (
    <section id="top" className="relative overflow-hidden">
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0 -z-10"
        style={{
          background:
            "radial-gradient(900px 420px at 82% -6%, var(--sky-soft), transparent 70%), radial-gradient(700px 380px at 4% 6%, var(--brand-soft), transparent 68%)",
        }}
      />
      <div className="mx-auto grid w-full max-w-6xl items-center gap-12 px-5 py-16 lg:grid-cols-[minmax(0,0.92fr)_minmax(0,1.08fr)] lg:py-24">
        <Reveal>
          <p className="inline-flex items-center gap-2 rounded-full border border-navy/12 bg-card px-3 py-1.5 text-[11px] font-bold tracking-[0.14em] text-navy-500">
            <WindowsIcon className="h-3 w-3 text-sky" />
            WINDOWS DESKTOP APPLICATION
          </p>
          <h1 className="mt-5 text-4xl leading-[1.08] font-extrabold text-navy sm:text-5xl lg:text-[3.4rem]">
            Find your study materials{" "}
            <span className="relative whitespace-nowrap text-brand">in seconds.</span>
          </h1>
          <p className="mt-5 max-w-xl text-lg leading-relaxed text-navy-500">
            E-Student makes it easy to search through your PDF, Word and Excel documents and keep
            your study sessions focused.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Btn href={DOWNLOAD_URL} size="lg">
              <WindowsIcon className="h-4 w-4" />
              Download for Windows
            </Btn>
            <Btn href={GITHUB_URL} variant="outline" size="lg">
              <GithubIcon className="h-4 w-4" />
              View on GitHub
            </Btn>
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            Free · Latest version v2.2.6 · Installer for Windows 10 &amp; 11
          </p>
        </Reveal>

        <Reveal delay={120} className="min-w-0">
          <WindowFrame className="lg:scale-[1.03] lg:origin-left">
            <SearchScreen />
          </WindowFrame>
        </Reveal>
      </div>
    </section>
  );
}

const intro = [
  {
    title: "Find information quickly",
    body: "Type a word and see exactly which document and page it appears on.",
  },
  {
    title: "Search across your documents",
    body: "PDF, Word and Excel files are all searched together, in one place.",
  },
  {
    title: "Keep materials organized",
    body: "Load your lecture notes once and keep them ready for every study session.",
  },
  {
    title: "Stay focused with Pomodoro",
    body: "Work in focused sessions with a built-in timer and break reminders.",
  },
];

function Intro() {
  return (
    <section className="border-y border-border bg-card">
      <div className="mx-auto w-full max-w-6xl px-5 py-16 lg:py-20">
        <Reveal className="max-w-2xl">
          <h2 className="text-3xl font-extrabold text-navy sm:text-4xl">
            Everything you need to study smarter.
          </h2>
          <p className="mt-4 text-lg text-navy-500">
            E-Student keeps all of your course material in one window — searchable, organized and
            ready when you need it.
          </p>
        </Reveal>
        <div className="mt-10 grid gap-px overflow-hidden rounded-xl border border-border bg-border sm:grid-cols-2 lg:grid-cols-4">
          {intro.map((item, i) => (
            <Reveal key={item.title} delay={i * 80}>
              <div className="h-full bg-card p-6 transition-colors hover:bg-surface-2">
                <span className="font-display text-sm font-bold text-brand">
                  0{i + 1}
                </span>
                <h3 className="mt-3 text-base font-bold text-navy">{item.title}</h3>
                <p className="mt-2 text-sm leading-relaxed text-navy-500">{item.body}</p>
              </div>
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  );
}

function FeatureRow({
  label,
  title,
  body,
  points,
  visual,
  flip,
}: {
  label: string;
  title: string;
  body: string;
  points: string[];
  visual: React.ReactNode;
  flip?: boolean;
}) {
  return (
    <div className="grid items-center gap-10 lg:grid-cols-2 lg:gap-16">
      <Reveal className={flip ? "lg:order-2" : ""}>
        <p className="text-xs font-bold tracking-[0.16em] text-brand">{label}</p>
        <h3 className="mt-3 text-2xl font-extrabold text-navy sm:text-3xl">{title}</h3>
        <p className="mt-4 text-base leading-relaxed text-navy-500">{body}</p>
        <ul className="mt-6 space-y-3">
          {points.map((p) => (
            <li key={p} className="flex gap-3 text-sm text-navy-500">
              <span className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-brand-soft text-[11px] font-bold text-brand">
                ✓
              </span>
              {p}
            </li>
          ))}
        </ul>
      </Reveal>
      <Reveal delay={100} className={`min-w-0 ${flip ? "lg:order-1" : ""}`}>
        {visual}
      </Reveal>
    </div>
  );
}

function Features() {
  return (
    <section id="features" className="mx-auto w-full max-w-6xl space-y-20 px-5 py-16 lg:py-24">
      <FeatureRow
        label="DOCUMENT SEARCH"
        title="Search through your documents."
        body="Add your PDF, Word and Excel files once, then type what you're looking for. E-Student searches inside every document and shows you the file and the page where your words appear."
        points={[
          "Works with PDF, Word (.doc, .docx) and Excel (.xls, .xlsx) files",
          "Results appear as you type — no separate search button",
          "Double-click a result to open the file on the right page",
        ]}
        visual={
          <WindowFrame>
            <SearchScreen query="regresyon" />
          </WindowFrame>
        }
      />
      <FeatureRow
        flip
        label="FAST & CONVENIENT"
        title="Stop opening documents one by one."
        body="No more scrolling through a dozen lecture notes to find a single definition. Everything you loaded is searched at once, and results come back instantly — even across thousands of pages."
        points={[
          "One search covers all of your loaded study material",
          "Documents are remembered, so the next search starts instantly",
          "Jump straight to the exact page instead of skimming",
        ]}
        visual={
          <WindowFrame>
            <SearchScreen query="türev formülü" />
          </WindowFrame>
        }
      />
      <FeatureRow
        label="POMODORO"
        title="Stay focused on what matters."
        body="A focus timer lives right next to your notes. Study in 25-minute sessions, take short breaks, and see how many rounds you've finished — all without leaving the app."
        points={[
          "Focus sessions with short and long breaks",
          "Skip or reset a session whenever your plan changes",
          "Keep track of the pomodoros you completed today",
        ]}
        visual={
          <WindowFrame>
            <PomodoroScreen />
          </WindowFrame>
        }
      />
    </section>
  );
}

function Screenshots() {
  return (
    <section id="screenshots" className="bg-navy">
      <div className="mx-auto w-full max-w-6xl px-5 py-16 lg:py-24">
        <Reveal className="max-w-2xl">
          <p className="text-xs font-bold tracking-[0.16em] text-brand">SCREENSHOTS</p>
          <h2 className="mt-3 text-3xl font-extrabold text-white sm:text-4xl">
            See E-Student in action.
          </h2>
          <p className="mt-4 text-lg text-white/70">
            A clean, familiar Windows interface — a sidebar for navigation, your files on the left
            and your results on the right.
          </p>
        </Reveal>

        <Reveal delay={80} className="mt-10 min-w-0">
          <WindowFrame>
            <SearchScreen query="anayasa" />
          </WindowFrame>
        </Reveal>

        <div className="mt-6 grid min-w-0 gap-6 lg:grid-cols-2">
          <Reveal delay={120} className="min-w-0">
            <WindowFrame>
              <PomodoroScreen />
            </WindowFrame>
          </Reveal>
          <Reveal delay={200} className="min-w-0">
            <WindowFrame>
              <SearchScreen query="laboratuvar" />
            </WindowFrame>
          </Reveal>
        </div>
      </div>
    </section>
  );
}

function Download() {
  return (
    <section id="download" className="border-b border-border bg-card">
      <div className="mx-auto w-full max-w-6xl px-5 py-20">
        <Reveal className="mx-auto max-w-2xl text-center">
          <img src={logo} alt="E-Student logo" className="mx-auto h-16 w-16 object-contain" />
          <h2 className="mt-6 text-3xl font-extrabold text-navy sm:text-4xl">
            Ready to study smarter?
          </h2>
          <p className="mt-4 text-lg text-navy-500">
            Download E-Student and keep your study materials searchable and organized.
          </p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <Btn href={DOWNLOAD_URL} size="lg">
              <WindowsIcon className="h-4 w-4" />
              Download for Windows
            </Btn>
            <Btn href={GITHUB_URL} variant="outline" size="lg">
              <GithubIcon className="h-4 w-4" />
              View on GitHub
            </Btn>
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            Guided installer · Automatic updates · Free to use
          </p>
        </Reveal>
      </div>
    </section>
  );
}

function Footer() {
  return (
    <footer className="bg-navy text-background">
      <div className="mx-auto grid w-full max-w-6xl gap-10 px-5 py-16 sm:grid-cols-2 lg:grid-cols-4">
        <div className="lg:col-span-2">
          <div className="flex items-center gap-2.5">
            <img src={logo} alt="" className="h-9 w-9 object-contain" />
            <span className="font-display text-lg font-extrabold">E-Student</span>
          </div>
          <p className="mt-3 max-w-sm text-sm leading-relaxed text-background/70">
            Search. Organize. Focus. A lightweight Windows study companion for finding your
            documents fast and keeping your focus sessions on track.
          </p>
          <a
            href={DOWNLOAD_URL}
            target="_blank"
            rel="noreferrer"
            className="mt-6 inline-flex items-center gap-2 rounded-full bg-brand px-5 py-2.5 text-sm font-semibold text-navy transition-transform hover:-translate-y-0.5"
          >
            <WindowsIcon className="h-4 w-4" />
            Download for Windows
          </a>
        </div>

        <div>
          <h3 className="text-xs font-bold uppercase tracking-widest text-background/50">
            Explore
          </h3>
          <nav className="mt-4 flex flex-col gap-3 text-sm text-background/80">
            <a href="#features" className="w-fit hover:text-brand">
              Features
            </a>
            <a href="#screenshots" className="w-fit hover:text-brand">
              Screenshots
            </a>
            <a href="#download" className="w-fit hover:text-brand">
              Download
            </a>
            <a
              href={GITHUB_URL}
              target="_blank"
              rel="noreferrer"
              className="w-fit hover:text-brand"
            >
              GitHub
            </a>
          </nav>
        </div>

        <div>
          <h3 className="text-xs font-bold uppercase tracking-widest text-background/50">
            Developer
          </h3>
          <div className="mt-4 rounded-2xl border border-background/15 bg-background/5 p-4">
            <p className="font-display text-base font-bold">Sahil Rzayev</p>
            <p className="mt-1 text-sm text-background/70">
              Creator &amp; maintainer of E-Student
            </p>
            <div className="mt-4 flex flex-col gap-2 text-sm">
              <a
                href="https://github.com/rzayevsahil"
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-2 text-background/80 hover:text-brand"
              >
                <GithubIcon className="h-4 w-4" />
                @rzayevsahil
              </a>
              <a
                href={`${GITHUB_URL}/issues`}
                target="_blank"
                rel="noreferrer"
                className="text-background/80 hover:text-brand"
              >
                Send feedback
              </a>
            </div>
          </div>
        </div>
      </div>
      <div className="border-t border-background/10">
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-2 px-5 py-6 text-xs text-background/60 sm:flex-row sm:items-center sm:justify-between">
          <p>© 2026 E-Student. All rights reserved.</p>
          <p>Made with care for students · Free to use</p>
        </div>
      </div>
    </footer>
  );
}

function Index() {
  return (
    <div className="min-h-screen overflow-x-hidden bg-background">
      <Nav />
      <main>
        <Hero />
        <Intro />
        <Features />
        <Screenshots />
        <Download />
      </main>
      <Footer />
    </div>
  );
}

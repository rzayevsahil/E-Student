import { createFileRoute } from "@tanstack/react-router";
import logo from "@/assets/logo.png";
import { Reveal } from "@/components/Reveal";
import { WindowFrame } from "@/components/app-ui/WindowFrame";
import { SearchScreen } from "@/components/app-ui/SearchScreen";
import { PomodoroScreen } from "@/components/app-ui/PomodoroScreen";
import { useLanguage } from "@/lib/i18n";
import { useLatestRelease } from "@/hooks/useLatestRelease";

const DOWNLOAD_URL = "https://github.com/rzayevsahil/E-Student/releases/latest/download/E-Student.exe";
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
  className = "",
}: {
  href: string;
  variant?: "primary" | "outline" | "ghost";
  children: React.ReactNode;
  size?: "md" | "lg";
  className?: string;
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
      className={`inline-flex items-center justify-center gap-2.5 rounded-md font-semibold transition-all duration-200 ${pad} ${styles} ${className}`}
    >
      {children}
    </a>
  );
}

function Nav() {
  const { t } = useLanguage();

  return (
    <header className="sticky top-0 z-50 border-b border-border/80 bg-background/85 backdrop-blur">
      <div className="mx-auto grid h-16 w-full max-w-6xl grid-cols-2 items-center px-5 md:grid-cols-[210px_1fr_auto]">
        <a href="#top" className="flex items-center gap-2.5">
          <img src={logo} alt="E-Student logo" className="h-11 w-11 object-contain" />
          <span className="font-display text-xl font-extrabold text-navy">E-Student</span>
        </a>
        
        <nav className="hidden items-center justify-center gap-7 text-sm font-medium text-navy-500 md:flex">
          <a href="#features" className="transition-colors hover:text-brand">
            {t.nav.features}
          </a>
          <a href="#screenshots" className="transition-colors hover:text-brand">
            {t.nav.screenshots}
          </a>
          <a href="#download" className="transition-colors hover:text-brand">
            {t.nav.download}
          </a>
        </nav>

        <div className="flex items-center justify-end gap-3 justify-self-end">
          <LanguageSwitcher />
          <Btn href={DOWNLOAD_URL} className="min-w-[92px] text-center">
            {t.nav.download}
          </Btn>
        </div>
      </div>
    </header>
  );
}

function Hero() {
  const { t } = useLanguage();
  const version = useLatestRelease();

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
      <div className="mx-auto grid w-full max-w-6xl items-center gap-10 px-5 py-14 lg:grid-cols-2 lg:gap-12 lg:py-20">
        <Reveal className="min-w-0">
          <p className="inline-flex items-center gap-2 rounded-full border border-navy/12 bg-card px-3 py-1.5 text-[11px] font-bold tracking-[0.14em] text-navy-500">
            <WindowsIcon className="h-3 w-3 text-sky" />
            {t.hero.badge}
          </p>
          <h1 className="mt-5 text-3xl leading-[1.12] font-extrabold text-navy sm:text-4xl lg:text-[3.1rem]">
            {t.hero.titlePart1}
            <span className="relative text-brand">{t.hero.titlePart2}</span>
          </h1>
          <p className="mt-5 max-w-xl text-base leading-relaxed text-navy-500 sm:text-lg">
            {t.hero.description}
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Btn href={DOWNLOAD_URL} size="lg">
              <WindowsIcon className="h-4 w-4" />
              {t.hero.downloadBtn}
            </Btn>
            <Btn href={GITHUB_URL} variant="outline" size="lg">
              <GithubIcon className="h-4 w-4" />
              {t.hero.githubBtn}
            </Btn>
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            {t.hero.footerText}
          </p>
        </Reveal>

        <Reveal delay={120} className="min-w-0">
          <WindowFrame>
            <SearchScreen />
          </WindowFrame>
        </Reveal>
      </div>
    </section>
  );
}

function Intro() {
  const { t } = useLanguage();

  return (
    <section className="border-y border-border bg-card">
      <div className="mx-auto w-full max-w-6xl px-5 py-16 lg:py-20">
        <Reveal className="max-w-2xl">
          <h2 className="text-3xl font-extrabold text-navy sm:text-4xl">
            {t.intro.title}
          </h2>
          <p className="mt-4 text-lg text-navy-500">
            {t.intro.subtitle}
          </p>
        </Reveal>
        <div className="mt-10 grid gap-px overflow-hidden rounded-xl border border-border bg-border sm:grid-cols-2 lg:grid-cols-4">
          {t.intro.items.map((item, i) => (
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
  const { t } = useLanguage();

  return (
    <section id="features" className="mx-auto w-full max-w-6xl space-y-20 px-5 py-16 lg:py-24">
      <FeatureRow
        label={t.features.docSearchLabel}
        title={t.features.docSearchTitle}
        body={t.features.docSearchBody}
        points={t.features.docSearchPoints}
        visual={
          <WindowFrame>
            <SearchScreen query="regresyon" />
          </WindowFrame>
        }
      />
      <FeatureRow
        flip
        label={t.features.fastLabel}
        title={t.features.fastTitle}
        body={t.features.fastBody}
        points={t.features.fastPoints}
        visual={
          <WindowFrame>
            <SearchScreen query="türev formülü" />
          </WindowFrame>
        }
      />
      <FeatureRow
        label={t.features.pomodoroLabel}
        title={t.features.pomodoroTitle}
        body={t.features.pomodoroBody}
        points={t.features.pomodoroPoints}
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
  const { t } = useLanguage();

  return (
    <section id="screenshots" className="bg-navy">
      <div className="mx-auto w-full max-w-6xl px-5 py-16 lg:py-24">
        <Reveal className="max-w-2xl">
          <p className="text-xs font-bold tracking-[0.16em] text-brand">{t.screenshots.label}</p>
          <h2 className="mt-3 text-3xl font-extrabold text-white sm:text-4xl">
            {t.screenshots.title}
          </h2>
          <p className="mt-4 text-lg text-white/70">
            {t.screenshots.subtitle}
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
  const { t } = useLanguage();

  return (
    <section id="download" className="border-b border-border bg-card">
      <div className="mx-auto w-full max-w-6xl px-5 py-20">
        <Reveal className="mx-auto max-w-2xl text-center">
          <img src={logo} alt="E-Student logo" className="mx-auto h-28 w-28 object-contain" />
          <h2 className="mt-6 text-3xl font-extrabold text-navy sm:text-4xl">
            {t.download.title}
          </h2>
          <p className="mt-4 text-lg text-navy-500">
            {t.download.subtitle}
          </p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <Btn href={DOWNLOAD_URL} size="lg">
              <WindowsIcon className="h-4 w-4" />
              {t.download.downloadBtn}
            </Btn>
            <Btn href={GITHUB_URL} variant="outline" size="lg">
              <GithubIcon className="h-4 w-4" />
              {t.download.githubBtn}
            </Btn>
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            {t.download.footerText}
          </p>
        </Reveal>
      </div>
    </section>
  );
}

function Footer() {
  const { t } = useLanguage();

  return (
    <footer className="bg-navy text-background">
      <div className="mx-auto grid w-full max-w-6xl gap-10 px-5 py-16 sm:grid-cols-2 lg:grid-cols-4">
        <div className="lg:col-span-2">
          <div className="flex items-center gap-3">
            <img src={logo} alt="" className="h-16 w-16 object-contain" />
            <span className="font-display text-2xl font-extrabold">E-Student</span>
          </div>
          <p className="mt-3 max-w-sm text-sm leading-relaxed text-background/70">
            {t.footer.tagline}
          </p>
          <a
            href={DOWNLOAD_URL}
            target="_blank"
            rel="noreferrer"
            className="mt-6 inline-flex items-center gap-2 rounded-full bg-brand px-5 py-2.5 text-sm font-semibold text-navy transition-transform hover:-translate-y-0.5"
          >
            <WindowsIcon className="h-4 w-4" />
            {t.footer.downloadBtn}
          </a>
        </div>

        <div>
          <h3 className="text-xs font-bold uppercase tracking-widest text-background/50">
            {t.footer.explore}
          </h3>
          <nav className="mt-4 flex flex-col gap-3 text-sm text-background/80">
            <a href="#features" className="w-fit hover:text-brand">
              {t.nav.features}
            </a>
            <a href="#screenshots" className="w-fit hover:text-brand">
              {t.nav.screenshots}
            </a>
            <a href="#download" className="w-fit hover:text-brand">
              {t.nav.download}
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
            {t.footer.developer}
          </h3>
          <div className="mt-4 rounded-2xl border border-background/15 bg-background/5 p-4">
            <p className="font-display text-base font-bold">Sahil Rzayev</p>
            <p className="mt-1 text-sm text-background/70">
              {t.footer.developerTitle}
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
                {t.footer.feedback}
              </a>
            </div>
          </div>
        </div>
      </div>
      <div className="border-t border-background/10">
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-2 px-5 py-6 text-xs text-background/60 sm:flex-row sm:items-center sm:justify-between">
          <p>{t.footer.copyright}</p>
          <p>{t.footer.madeWithLove}</p>
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

import { useLanguage, Language } from "@/lib/i18n";

function FlagTR({ className = "h-3.5 w-5 rounded-2xs shadow-xs" }: { className?: string }) {
  return (
    <svg viewBox="0 0 1200 800" className={className} aria-hidden>
      <rect width="1200" height="800" fill="#E30A17" />
      <circle cx="425" cy="400" r="200" fill="#FFFFFF" />
      <circle cx="475" cy="400" r="160" fill="#E30A17" />
      <polygon
        fill="#FFFFFF"
        points="583.3,400 706.7,440.1 630.5,335.2 630.5,464.8 706.7,359.9"
      />
    </svg>
  );
}

function FlagAZ({ className = "h-3.5 w-5 rounded-2xs shadow-xs" }: { className?: string }) {
  return (
    <svg viewBox="0 0 1200 600" className={className} aria-hidden>
      <rect width="1200" height="200" fill="#0097C3" />
      <rect y="200" width="1200" height="200" fill="#E00034" />
      <rect y="400" width="1200" height="200" fill="#00AE65" />
      <circle cx="540" cy="300" r="90" fill="#FFFFFF" />
      <circle cx="562.5" cy="300" r="72" fill="#E00034" />
      <path
        fill="#FFFFFF"
        d="M660 300l-17.6 12.8 3.4-21.5-17.4-13.1 21.7-3.1 9.5-19.6 9.5 19.6 21.7 3.1-17.4 13.1 3.4 21.5z"
      />
    </svg>
  );
}

function FlagGB({ className = "h-3.5 w-5 rounded-2xs shadow-xs" }: { className?: string }) {
  return (
    <svg viewBox="0 0 60 30" className={className} aria-hidden>
      <clipPath id="gb-s">
        <path d="M0,0 v30 h60 v-30 z" />
      </clipPath>
      <clipPath id="gb-t">
        <path d="M30,15 H0 V0 z M30,15 V0 h30 z M30,15 H60 V30 z M30,15 V30 h-30 z" />
      </clipPath>
      <g clipPath="url(#gb-s)">
        <path d="M0,0 v30 h60 v-30 z" fill="#012169" />
        <path d="M0,0 L60,30 M60,0 L0,30" stroke="#fff" strokeWidth="6" />
        <path d="M0,0 L60,30 M60,0 L0,30" clipPath="url(#gb-t)" stroke="#C8102E" strokeWidth="4" />
        <path d="M30,0 v30 M0,15 h60" stroke="#fff" strokeWidth="10" />
        <path d="M30,0 v30 M0,15 h60" stroke="#C8102E" strokeWidth="6" />
      </g>
    </svg>
  );
}

const languages: { code: Language; label: string; flag: React.ReactNode }[] = [
  { code: "tr", label: "TR", flag: <FlagTR /> },
  { code: "az", label: "AZ", flag: <FlagAZ /> },
  { code: "en", label: "EN", flag: <FlagGB /> },
];

export function LanguageSwitcher() {
  const { lang, setLang } = useLanguage();

  return (
    <div className="inline-flex items-center gap-1 rounded-lg border border-navy/15 bg-card/80 p-1 shadow-sm backdrop-blur">
      {languages.map((item) => {
        const isActive = lang === item.code;
        return (
          <button
            key={item.code}
            type="button"
            onClick={() => setLang(item.code)}
            className={`flex items-center justify-center gap-1.5 rounded-md px-2.5 py-1 text-xs font-bold transition-all duration-150 ${
              isActive
                ? "bg-brand text-accent-foreground shadow-xs"
                : "text-navy-500 hover:bg-surface-2 hover:text-navy"
            }`}
            title={`Language: ${item.label}`}
          >
            {item.flag}
            <span>{item.label}</span>
          </button>
        );
      })}
    </div>
  );
}

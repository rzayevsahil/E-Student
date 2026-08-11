import { useLanguage, Language } from "@/lib/i18n";

const languages: { code: Language; label: string; flag: string }[] = [
  { code: "tr", label: "TR", flag: "🇹🇷" },
  { code: "az", label: "AZ", flag: "🇦🇿" },
  { code: "en", label: "EN", flag: "🇬🇧" },
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
            className={`flex items-center gap-1.5 rounded-md px-2.5 py-1 text-xs font-bold transition-all duration-150 ${
              isActive
                ? "bg-brand text-accent-foreground shadow-xs"
                : "text-navy-500 hover:bg-surface-2 hover:text-navy"
            }`}
            title={`Language: ${item.label}`}
          >
            <span>{item.flag}</span>
            <span>{item.label}</span>
          </button>
        );
      })}
    </div>
  );
}

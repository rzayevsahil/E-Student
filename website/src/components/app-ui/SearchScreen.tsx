import logo from "@/assets/logo.png";

const files = [
  { name: "Veri Yapıları - Hafta 5.pdf", ext: "PDF" },
  { name: "Mikroiktisat Ders Notu.docx", ext: "DOCX" },
  { name: "Laboratuvar Ölçümleri.xlsx", ext: "XLSX" },
  { name: "Anayasa Hukuku Özet.pdf", ext: "PDF" },
  { name: "İstatistik Formüller.docx", ext: "DOCX" },
];

const results = [
  { file: "Veri Yapıları - Hafta 5.pdf", page: "14" },
  { file: "İstatistik Formüller.docx", page: "3" },
  { file: "Anayasa Hukuku Özet.pdf", page: "27" },
  { file: "Laboratuvar Ölçümleri.xlsx", page: "Sayfa 2" },
  { file: "Mikroiktisat Ders Notu.docx", page: "9" },
];

export function Sidebar({ active }: { active: "search" | "pomodoro" }) {
  const base =
    "flex items-center gap-2 rounded-[5px] px-3 py-2.5 text-[13px] text-left transition-colors";
  return (
    <aside className="hidden w-[168px] shrink-0 flex-col border-r border-border bg-surface-2 sm:flex lg:w-[196px]">
      <div className="flex items-center justify-center border-b border-border bg-card px-3 py-3">
        <img src={logo} alt="E-Student application logo" className="h-12 w-12 object-contain" />
      </div>
      <div className="flex flex-col gap-1 p-2">
        <div
          className={`${base} ${active === "search"
            ? "bg-sky-soft font-semibold text-sky"
            : "text-navy-500 hover:bg-muted"
            }`}
        >
          <span aria-hidden>📄</span> Belge Arama
        </div>
        <div
          className={`${base} ${active === "pomodoro"
            ? "bg-sky-soft font-semibold text-sky"
            : "text-navy-500 hover:bg-muted"
            }`}
        >
          <span aria-hidden>🍅</span> Pomodoro
        </div>
      </div>
      <div className="mt-auto border-t border-border px-3 py-2 text-[10px] text-muted-foreground">
        v2.3.1
      </div>
    </aside>
  );
}

export function SearchScreen({ query = "regresyon" }: { query?: string }) {
  return (
    <div className="flex bg-card">
      <Sidebar active="search" />
      <div className="min-w-0 flex-1 p-3 sm:p-4">
        <h3 className="text-center text-[15px] font-bold text-navy sm:text-[17px]">
          Belge İçerik Arama Uygulaması
        </h3>
        <div className="mt-3 flex items-center gap-3">
          <span className="rounded-[3px] bg-sky px-3 py-1.5 text-[12px] font-medium text-white">
            Dosya Yükle
          </span>
          <span className="truncate text-[11px] text-muted-foreground">
            5 dosya yüklendi — arama hazır
          </span>
        </div>

        <div className="mt-3 grid gap-3 md:grid-cols-[minmax(0,190px)_minmax(0,1fr)]">
          <div className="hidden overflow-hidden rounded-[5px] border border-border md:block">
            <div className="bg-muted px-2.5 py-2 text-[12px] font-bold text-navy">
              Yüklenen Dosyalar
            </div>
            <ul className="divide-y divide-border">
              {files.map((f) => (
                <li key={f.name} className="flex items-start gap-2 px-2.5 py-2">
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-[11.5px] font-semibold text-navy">{f.name}</p>
                    <p className="text-[9.5px] text-muted-foreground">{f.ext} dosyası</p>
                    <p className="text-[9px] text-sky">📄 Çift tıklayarak aç</p>
                  </div>
                  <span className="text-[10px] text-destructive">✕</span>
                </li>
              ))}
            </ul>
          </div>

          <div className="min-w-0">
            <div className="rounded-[5px] border border-border p-2.5">
              <p className="mb-1.5 text-[12px] font-bold text-navy">Arama</p>
              <div className="flex h-11 items-center rounded-[3px] border border-border px-3 text-[15px] text-navy">
                {query}
                <span className="ml-0.5 inline-block h-4 w-px animate-pulse bg-navy" />
              </div>
              <p className="mt-1.5 text-[10px] text-muted-foreground">
                Yazdıkça otomatik arama yapılır
              </p>
            </div>

            <div className="mt-3 overflow-hidden rounded-[5px] border border-border">
              <div className="grid grid-cols-[minmax(0,1fr)_56px] gap-2 bg-muted px-2.5 py-1.5 text-[11px] font-bold text-navy">
                <span>Dosya</span>
                <span>Sayfa</span>
              </div>
              <ul className="divide-y divide-border">
                {results.map((r) => (
                  <li
                    key={r.file}
                    className="grid grid-cols-[minmax(0,1fr)_56px] gap-2 px-2.5 py-2 text-[11.5px] text-navy-500"
                  >
                    <span className="truncate">{r.file}</span>
                    <span>{r.page}</span>
                  </li>
                ))}
              </ul>
              <div className="bg-brand-soft px-2.5 py-2 text-[10.5px] text-navy-500">
                <span className="font-bold text-navy">💡 İpucu:</span> Sonuçlara çift tıklayarak
                dosyayı ilgili sayfadan açabilirsiniz
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
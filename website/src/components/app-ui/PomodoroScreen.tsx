import { Sidebar } from "./SearchScreen";

export function PomodoroScreen() {
  return (
    <div className="flex bg-card">
      <Sidebar active="pomodoro" />
      <div className="min-w-0 flex-1 p-4 sm:p-6">
        <h3 className="text-center text-[15px] font-bold text-navy sm:text-[17px]">
          <span aria-hidden>🍅</span> Pomodoro Tekniği
        </h3>

        <div className="mt-5 flex flex-col items-center">
          <div className="relative flex h-36 w-36 items-center justify-center rounded-full border-[6px] border-muted sm:h-44 sm:w-44">
            <div
              className="absolute inset-[-6px] rounded-full"
              style={{
                background:
                  "conic-gradient(var(--brand) 0deg 252deg, transparent 252deg 360deg)",
                mask: "radial-gradient(farthest-side, transparent calc(100% - 6px), #000 calc(100% - 6px))",
                WebkitMask:
                  "radial-gradient(farthest-side, transparent calc(100% - 6px), #000 calc(100% - 6px))",
              }}
            />
            <div className="text-center">
              <p className="font-display text-3xl font-bold tabular-nums text-navy sm:text-4xl">
                17:24
              </p>
              <p className="text-[11px] font-medium text-brand">Çalışma</p>
            </div>
          </div>

          <div className="mt-4 flex items-center gap-2">
            <span className="rounded-[3px] bg-sky px-4 py-1.5 text-[12px] font-medium text-white">
              Duraklat
            </span>
            <span className="rounded-[3px] border border-border px-4 py-1.5 text-[12px] text-navy-500">
              Sıfırla
            </span>
            <span className="rounded-[3px] border border-border px-4 py-1.5 text-[12px] text-navy-500">
              Geç
            </span>
          </div>

          <p className="mt-4 text-[11px] text-muted-foreground">
            Tamamlanan Pomodorolar: <span className="font-bold text-navy">3</span>
          </p>
        </div>

        <div className="mt-5 rounded-[5px] border border-border bg-surface-2 p-3 text-[11px] leading-relaxed text-navy-500">
          <p className="mb-1 font-bold text-navy">📌 Pomodoro Tekniği Nasıl Çalışır?</p>
          <p>1. 25 dakika boyunca odaklanarak çalışın</p>
          <p>2. Çalışma süresi bitince 5 dakika kısa mola verin</p>
          <p>3. Her 4 pomodoroda bir 15 dakika uzun mola verin</p>
          <p>4. Bu döngüyü tekrarlayarak verimliliğinizi artırın</p>
        </div>
      </div>
    </div>
  );
}
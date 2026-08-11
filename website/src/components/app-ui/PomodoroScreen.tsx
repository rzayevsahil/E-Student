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
          {/* Dual Glow Circular Ring + Dark Interior Container */}
          <div className="relative flex h-44 w-44 items-center justify-center sm:h-52 sm:w-52">
            {/* SVG Ring with Glow */}
            <svg
              className="absolute inset-0 h-full w-full"
              viewBox="0 0 200 200"
              fill="none"
              xmlns="http://www.w3.org/2000/svg"
            >
              <filter id="glow-cyan" x="-20%" y="-20%" width="140%" height="140%">
                <feGaussianBlur stdDeviation="5" result="blur" />
                <feMerge>
                  <feMergeNode in="blur" />
                  <feMergeNode in="SourceGraphic" />
                </feMerge>
              </filter>
              <filter id="glow-orange" x="-20%" y="-20%" width="140%" height="140%">
                <feGaussianBlur stdDeviation="5" result="blur" />
                <feMerge>
                  <feMergeNode in="blur" />
                  <feMergeNode in="SourceGraphic" />
                </feMerge>
              </filter>

              {/* Left Arc (Cyan) */}
              <path
                d="M 100,10 A 90,90 0 0,0 100,190"
                stroke="#00D2FF"
                strokeWidth="6"
                strokeLinecap="round"
                filter="url(#glow-cyan)"
              />

              {/* Right Arc (Orange / Coral) */}
              <path
                d="M 100,10 A 90,90 0 0,1 100,190"
                stroke="#FF6B35"
                strokeWidth="6"
                strokeLinecap="round"
                filter="url(#glow-orange)"
              />
            </svg>

            {/* Inner Dark Circle */}
            <div className="z-10 flex h-[142px] w-[142px] flex-col items-center justify-center rounded-full border border-[#2a2f38] bg-[#181c22] p-3 text-center shadow-inner sm:h-[168px] sm:w-[168px]">
              <p className="text-[11px] font-semibold text-[#00D2FF]">Çalışma</p>
              <p className="my-1 font-['DSEG7_Classic',monospace] text-2xl font-bold tracking-widest text-white drop-shadow-[0_0_8px_rgba(255,255,255,0.4)] sm:text-3xl">
                24:59
              </p>
              <p className="text-[10px] text-slate-400">25 dk odaklanma</p>
            </div>
          </div>

          <div className="mt-5 flex items-center gap-2">
            <span className="rounded-[4px] bg-[#00D2FF] px-4 py-1.5 text-[12px] font-bold text-[#0F172A] shadow-sm">
              Başlat
            </span>
            <span className="rounded-[4px] bg-[#FF6B35] px-4 py-1.5 text-[12px] font-medium text-white shadow-sm">
              Sıfırla
            </span>
            <span className="rounded-[4px] border border-border bg-surface-2 px-4 py-1.5 text-[12px] text-navy-500">
              Geç
            </span>
          </div>

          <p className="mt-4 text-[11px] text-muted-foreground">
            Tamamlanan Pomodorolar: <span className="font-bold text-[#00D2FF]">3</span>
          </p>
        </div>

        <div className="mt-5 rounded-[6px] border border-border bg-surface-2 p-3.5 text-[11px] leading-relaxed text-navy-500">
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
import type { ReactNode } from "react";

export function WindowFrame({
  children,
  className = "",
  title = "E-Student",
}: {
  children: ReactNode;
  className?: string;
  title?: string;
}) {
  return (
    <div
      className={`overflow-hidden rounded-lg border border-border bg-card shadow-app ${className}`}
    >
      <div className="flex items-center gap-2 border-b border-border bg-surface-2 px-3 py-2">
        <span className="text-[11px] font-semibold text-navy-500">{title}</span>
        <div className="ml-auto flex items-center gap-3 text-navy-500/70">
          <span className="block h-px w-3 bg-current" />
          <span className="block h-2.5 w-2.5 border border-current" />
          <span className="relative block h-2.5 w-2.5">
            <span className="absolute inset-0 rotate-45 border-t border-current top-1/2" />
            <span className="absolute inset-0 -rotate-45 border-t border-current top-1/2" />
          </span>
        </div>
      </div>
      {children}
    </div>
  );
}
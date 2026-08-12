import { useState, useEffect } from "react";

const GITHUB_REPO = "rzayevsahil/E-Student";
export const APP_VERSION = "v2.3.8";

let cachedVersion: string | null = null;
let fetchPromise: Promise<string> | null = null;

async function fetchLatestRelease(): Promise<string> {
  if (cachedVersion) return cachedVersion;
  if (fetchPromise) return fetchPromise;

  fetchPromise = (async () => {
    try {
      if (typeof window !== "undefined" && window.sessionStorage) {
        const saved = sessionStorage.getItem("estudent_latest_version");
        if (saved) {
          cachedVersion = saved;
          return saved;
        }
      }

      const res = await fetch(`https://api.github.com/repos/${GITHUB_REPO}/releases/latest`, {
        headers: { Accept: "application/vnd.github.v3+json" },
      });

      if (res.ok) {
        const data = await res.json();
        if (data && data.tag_name && typeof data.tag_name === "string") {
          cachedVersion = data.tag_name;
          if (typeof window !== "undefined" && window.sessionStorage) {
            sessionStorage.setItem("estudent_latest_version", data.tag_name);
          }
          return data.tag_name;
        }
      }
    } catch {
      // Quietly fall back to APP_VERSION on network issues or rate limits
    }
    return APP_VERSION;
  })();

  return fetchPromise;
}

export function useLatestRelease() {
  const [version, setVersion] = useState<string>(cachedVersion || APP_VERSION);

  useEffect(() => {
    let isMounted = true;
    fetchLatestRelease().then((ver) => {
      if (isMounted && ver) {
        setVersion(ver);
      }
    });

    return () => {
      isMounted = false;
    };
  }, []);

  return version;
}

import { useState, useEffect } from "react";

const GITHUB_REPO = "rzayevsahil/E-Student";
const DEFAULT_VERSION = "v2.3.1";

export function useLatestRelease() {
  const [version, setVersion] = useState<string>(DEFAULT_VERSION);

  useEffect(() => {
    let isMounted = true;
    fetch(`https://api.github.com/repos/${GITHUB_REPO}/releases/latest`)
      .then((res) => {
        if (res.ok) return res.json();
        throw new Error("Failed to fetch release");
      })
      .then((data) => {
        if (isMounted && data && data.tag_name) {
          setVersion(data.tag_name);
        }
      })
      .catch(() => {
        // Fallback to DEFAULT_VERSION if fetch fails or rate limited
      });

    return () => {
      isMounted = false;
    };
  }, []);

  return version;
}

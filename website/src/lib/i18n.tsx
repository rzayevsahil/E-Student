import React, { createContext, useContext, useState, useEffect } from "react";

export type Language = "tr" | "az" | "en";

export interface Translations {
  nav: {
    features: string;
    screenshots: string;
    download: string;
  };
  hero: {
    badge: string;
    titlePart1: string;
    titlePart2: string;
    description: string;
    downloadBtn: string;
    githubBtn: string;
    footerText: string;
  };
  intro: {
    title: string;
    subtitle: string;
    items: Array<{ title: string; body: string }>;
  };
  features: {
    docSearchLabel: string;
    docSearchTitle: string;
    docSearchBody: string;
    docSearchPoints: string[];

    fastLabel: string;
    fastTitle: string;
    fastBody: string;
    fastPoints: string[];

    multiLangLabel: string;
    multiLangTitle: string;
    multiLangBody: string;
    multiLangPoints: string[];

    pomodoroLabel: string;
    pomodoroTitle: string;
    pomodoroBody: string;
    pomodoroPoints: string[];
  };
  screenshots: {
    label: string;
    title: string;
    subtitle: string;
  };
  download: {
    title: string;
    subtitle: string;
    downloadBtn: string;
    githubBtn: string;
    footerText: string;
  };
  footer: {
    tagline: string;
    downloadBtn: string;
    explore: string;
    developer: string;
    developerTitle: string;
    feedback: string;
    copyright: string;
    madeWithLove: string;
  };
}

export const translations: Record<Language, Translations> = {
  tr: {
    nav: {
      features: "Özellikler",
      screenshots: "Ekran Görüntüleri",
      download: "İndir",
    },
    hero: {
      badge: "WINDOWS MASAÜSTÜ UYGULAMASI",
      titlePart1: "Ders materyallerinizi ",
      titlePart2: "saniyeler içinde bulun.",
      description:
        "E-Student, PDF, Word, Excel ve PowerPoint belgeleriniz içinde anında arama yapmanızı sağlar ve Pomodoro zamanlayıcı ile çalışma odaklılığınızı artırır.",
      downloadBtn: "Windows için İndir",
      githubBtn: "GitHub'da İncele",
      footerText: "Ücretsiz · Son sürüm {version} · Windows 10 & 11 Kurulumu",
    },
    intro: {
      title: "Daha akıllı çalışmak için ihtiyacınız olan her şey.",
      subtitle:
        "E-Student tüm ders materyallerinizi tek bir pencerede tutar — taranabilir, düzenli ve her an hazır.",
      items: [
        {
          title: "Bilgiye anında ulaşın",
          body: "Aradığınız sözcüğü yazın ve tam olarak hangi belgenin kaçıncı sayfasında veya slaydında geçtiğini görün.",
        },
        {
          title: "Tüm dokümanlarda eşzamanlı arama",
          body: "PDF, Word, Excel ve PowerPoint dosyalarınızın tamamı tek bir noktadan aynı anda taranır.",
        },
        {
          title: "Çok dilli yerelleştirme (TR | EN | AZ)",
          body: "Türkçe, İngilizce ve Azerbaycanca arayüz dilleri arasında tek tıkla anında geçiş yapın.",
        },
        {
          title: "Hızlı önizleme ve sürükle-bırak",
          body: "Sayfaları yan panelde canlı inceleyin ve dosyalarınızı sürükleyip bırakarak saniyeler içinde yükleyin.",
        },
      ],
    },
    features: {
      docSearchLabel: "DOKÜMAN ARAMA",
      docSearchTitle: "Belgelerinizin içerisinde özgürce arayın.",
      docSearchBody:
        "PDF, Word, Excel ve PowerPoint dosyalarınızı ekleyin, ardından aradığınız terimi yazın. E-Student her bir dosyanın içeriğini tarar ve eşleşen sayfa/slayt numarasıyla birlikte önünüze getirir.",
      docSearchPoints: [
        "PDF, Word (.docx), Excel (.xlsx) ve PowerPoint (.pptx, .ppt) formatlarını destekler",
        "Siz yazdıkça sonuçlar anında listelenir — ayrı bir buton gerekmez",
        "Sonuca çift tıklayarak ilgili belgeyi tam sayfasında veya slaydında açın",
      ],
      fastLabel: "HIZLI VE PRATİK",
      fastTitle: "Dosyaları tek tek açıp okumaya son verin.",
      fastBody:
        "Tek bir tanımı bulmak için onlarca sayfa ders notunu kaydırmak zorunda kalmayın. Yüklediğiniz tüm materyal tek seferde taranır ve binlerce sayfa içinden milisaniyeler içinde sonuç döner.",
      fastPoints: [
        "Tek bir arama tüm ders materyalini kapsar",
        "Yüklenen belgeler önbelleğe alınır, sonraki aramalar saliseler sürer",
        "Göz gezdirmek yerine doğrudan ilgili sayfaya zıplayın",
      ],
      multiLangLabel: "ÇOK DİLLİ VE AKILLI ARAYÜZ",
      multiLangTitle: "Tam dil desteği ve canlı bölünmüş önizleme.",
      multiLangBody:
        "Türkçe, İngilizce ve Azerbaycanca dillerinde kesintisiz arayüz. Arama sonuçlarını harici bir programa ihtiyaç duymadan sağ taraftaki bölünmüş okuyucu panelinde doğrudan görüntüleyin.",
      multiLangPoints: [
        "Türkçe, İngilizce ve Azerbaycanca (TR | EN | AZ) tam dinamik yerelleştirme",
        "Yan panelde sayfa ve slayt bazlı canlı metin önizleme ve gezinme",
        "Sürükle-bırak dosya yükleme ve format çip filtreleri",
      ],
      pomodoroLabel: "POMODORO ZAMANLAYICI",
      pomodoroTitle: "Dikkat dağıtıcı unsurları engelleyin.",
      pomodoroBody:
        "Çalışma zamanlayıcınız notlarınızın hemen yanında yer alır. 25 dakikalık odaklanma turları yapın, kısa molalar verin ve gün boyu kaç Pomodoro tamamladığınızı takip edin.",
      pomodoroPoints: [
        "Odaklanma seansları, kısa ve uzun mola döngüleri",
        "Planınız değiştiğinde turu atlayın veya sıfırlayın",
        "Günlük tamamladığınız Pomodoro istatistiklerini takip edin",
      ],
    },
    screenshots: {
      label: "EKRAN GÖRÜNTÜLERİ",
      title: "E-Student'ı iş başında görün.",
      subtitle:
        "Kullanıcı dostu Windows arayüzü — gezinme için yan menü, sol tarafta dosyalarınız ve sağ tarafta anlık arama sonuçları.",
    },
    download: {
      title: "Daha akıllı çalışmaya hazır mısınız?",
      subtitle:
        "E-Student'ı ücretsiz indirerek ders materyallerinizi taranabilir ve düzenli tutun.",
      downloadBtn: "Windows için İndir",
      githubBtn: "GitHub'da İncele",
      footerText: "Kolay Kurulum Sihirbazı · Otomatik Güncelleme · Tamamen Ücretsiz",
    },
    footer: {
      tagline:
        "Ara. Düzenle. Odaklan. Ders materyallerinizi anında bulmanızı ve çalışma oturumlarınızı takip etmenizi sağlayan Windows asistanı.",
      downloadBtn: "Windows için İndir",
      explore: "Keşfet",
      developer: "Geliştirici",
      developerTitle: "E-Student Geliştiricisi",
      feedback: "Geri Bildirim Gönder",
      copyright: "© 2026 E-Student. Tüm hakları saklıdır.",
      madeWithLove: "Öğrenciler için özenle tasarlandı · Ücretsiz",
    },
  },
  az: {
    nav: {
      features: "Xüsusiyyətlər",
      screenshots: "Ekran Görüntüləri",
      download: "Yüklə",
    },
    hero: {
      badge: "WINDOWS MASAÜSTÜ TƏTBİQİ",
      titlePart1: "Dərs materiallarınızı ",
      titlePart2: "saniyələr içində tapın.",
      description:
        "E-Student, PDF, Word, Excel və PowerPoint sənədlərinizdə anında axtarış etməyinizi təmin edir və Pomodoro taymeri ilə diqqətinizi artırır.",
      downloadBtn: "Windows üçün Yüklə",
      githubBtn: "GitHub-da Bax",
      footerText: "Ödənişsiz · Son versiya {version} · Windows 10 & 11 Yükləyicisi",
    },
    intro: {
      title: "Daha ağıllı təhsil almaq üçün ehtiyacınız olan hər şey.",
      subtitle:
        "E-Student bütün dərs materiallarınızı tək bir pəncərədə saxlayır — axtarıla bilən, mütəşəkkil və hər an hazır.",
      items: [
        {
          title: "Məlumata anında çatın",
          body: "Axtardığınız sözü yazın və dəqiq hansı sənədin neçənci səhifəsində və ya slaydında olduğunu görün.",
        },
        {
          title: "Bütün sənədlərdə eyni vaxtda axtarış",
          body: "PDF, Word, Excel və PowerPoint fayllarınızın hamısı tək bir yerdən eyni anda taranır.",
        },
        {
          title: "Çoxdilli lokallaşdırma (AZ | EN | TR)",
          body: "Azərbaycan, İngilis və Türk interfeys dilləri arasında tək kliklə anında keçid edin.",
        },
        {
          title: "Tez önizləmə və sürüklə-burax",
          body: "Səhifələri yan paneldə canlı nəzərdən keçirin və faylları sürükləyib buraxaraq saniyələr içində yükləyin.",
        },
      ],
    },
    features: {
      docSearchLabel: "SƏNƏD AXTARIŞI",
      docSearchTitle: "Sənədlərinizin içərisində sərbəst axtarın.",
      docSearchBody:
        "PDF, Word, Excel və PowerPoint fayllarınızı əlavə edin, sonra axtardığınız sözü yazın. E-Student hər bir sənədin məzmununu tarayır və uyğun gələn səhifə/slayd nömrəsi ilə təqdim edir.",
      docSearchPoints: [
        "PDF, Word (.docx), Excel (.xlsx) və PowerPoint (.pptx, .ppt) formatlarını dəstəkləyir",
        "Yazdıqca nəticələr anında siyahılanır — ayrı axtarış düyməsinə ehtiyac yoxdur",
        "Nəticəyə iki dəfə klikləyərək müvafiq sənədi dəqiq səhifəsində və ya slaydında açın",
      ],
      fastLabel: "SÜRƏTLİ VƏ RAHAT",
      fastTitle: "Faylları tək-tək açıb oxumağa son qoyun.",
      fastBody:
        "Tək bir tərifi tapmaq üçün onlarla səhifə dərs qeydini vərəqləməyə ehtiyac yoxdur. Yüklədiyiniz bütün materiallar bir dəfəyə taranır və minlərlə səhifə içindən milisaniyələr içində nəticə qayıdır.",
      fastPoints: [
        "Tək bir axtarış bütün dərs materiallarını əhatə edir",
        "Sənədlər keşlənir, növbəti axtarışlar salisələr çəkir",
        "Səhifələri vərəqləmək əvəzinə birbaşa müvafiq səhifəyə keçin",
      ],
      multiLangLabel: "ÇOXDİLLİ VƏ AĞILLI İNTERFEYS",
      multiLangTitle: "Tam dil dəstəyi və canlı bölünmüş önizləmə.",
      multiLangBody:
        "Azərbaycan, İngilis və Türk dillərində kəsintisiz interfeys. Axtarış nəticələrini kənar proqramlara ehtiyac olmadan sağ tərəfdəki bölünmüş oxuyucu panelində birbaşa baxın.",
      multiLangPoints: [
        "Azərbaycan, İngilis və Türk (AZ | EN | TR) tam dinamik lokallaşdırma",
        "Yan paneldə səhifə və slayd bazlı canlı mətn önizləmə və keçid",
        "Sürüklə-burax fayl yükləmə və format çip filtrləri",
      ],
      pomodoroLabel: "POMODORO TAYMERİ",
      pomodoroTitle: "Diqqəti cəmləyin və vaxtı idarə edin.",
      pomodoroBody:
        "Çalışma taymeriniz qeydlərinizin dərhal yanında yerləşir. 25 dəqiqəlik diqqət seansları edin, qısa fasilələr verin və gün ərzində neçə Pomodoro tamamladığınızı izləyin.",
      pomodoroPoints: [
        "Diqqət seansları, qısa və uzun fasilə dövrələri",
        "Planınız dəyişdikdə dövrəni keçin və ya sıfırlayın",
        "Gündəlik tamamladığınız Pomodoro statistikalarını izləyin",
      ],
    },
    screenshots: {
      label: "EKRAN GÖRÜNTÜLƏRİ",
      title: "E-Student-i fəaliyyətdə görün.",
      subtitle:
        "İstifadəçi dostu Windows interfeysi — naviqasiya üçün yan menyu, sol tərəfdə fayllarınız və sağ tərəfdə axtarış nəticələri.",
    },
    download: {
      title: "Daha ağıllı təhsil almağa hazırsınız?",
      subtitle:
        "E-Student-i ödənişsiz yükləyərək dərs materiallarınızı axtarıla bilən və mütəşəkkil saxlayın.",
      downloadBtn: "Windows üçün Yüklə",
      githubBtn: "GitHub-da Bax",
      footerText: "Asan Yükləmə Sehrbazı · Avtomatik Yeniləmə · Tamamilə Ödənişsiz",
    },
    footer: {
      tagline:
        "Axtar. Təşkil et. Diqqəti cəmlə. Dərs materiallarınızı anında tapmağı və çalışma seanslarınızı izləməyi təmin edən Windows köməkçisi.",
      downloadBtn: "Windows üçün Yüklə",
      explore: "Kəşf et",
      developer: "Tərtibatçı",
      developerTitle: "E-Student Tərtibatçısı",
      feedback: "Rəy Göndər",
      copyright: "© 2026 E-Student. Bütün hüquqlar qorunur.",
      madeWithLove: "Tələbələr üçün diqqətlə hazırlandı · Ödənişsiz",
    },
  },
  en: {
    nav: {
      features: "Features",
      screenshots: "Screenshots",
      download: "Download",
    },
    hero: {
      badge: "WINDOWS DESKTOP APPLICATION",
      titlePart1: "Find your study materials ",
      titlePart2: "in seconds.",
      description:
        "E-Student makes it easy to search through your PDF, Word, Excel and PowerPoint documents and keep your study sessions focused with a Pomodoro timer.",
      downloadBtn: "Download for Windows",
      githubBtn: "View on GitHub",
      footerText: "Free · Latest version {version} · Installer for Windows 10 & 11",
    },
    intro: {
      title: "Everything you need to study smarter.",
      subtitle:
        "E-Student keeps all of your course material in one window — searchable, organized and ready when you need it.",
      items: [
        {
          title: "Find information quickly",
          body: "Type a word and see exactly which document, page or slide it appears on.",
        },
        {
          title: "Search across your documents",
          body: "PDF, Word, Excel and PowerPoint files are all searched together, in one place.",
        },
        {
          title: "Multi-language support (EN | TR | AZ)",
          body: "Switch instantly between English, Turkish, and Azerbaijani interface languages with a single click.",
        },
        {
          title: "Split-view reader & drag-and-drop",
          body: "Preview match pages directly in the side panel and drag-and-drop files to load in seconds.",
        },
      ],
    },
    features: {
      docSearchLabel: "DOCUMENT SEARCH",
      docSearchTitle: "Search through your documents.",
      docSearchBody:
        "Add your PDF, Word, Excel and PowerPoint files once, then type what you're looking for. E-Student searches inside every document and shows you the file, page or slide where your words appear.",
      docSearchPoints: [
        "Works with PDF, Word (.docx), Excel (.xlsx) and PowerPoint (.pptx, .ppt) files",
        "Results appear as you type — no separate search button",
        "Double-click a result to open the file on the right page or slide",
      ],
      fastLabel: "FAST & CONVENIENT",
      fastTitle: "Stop opening documents one by one.",
      fastBody:
        "No more scrolling through a dozen lecture notes to find a single definition. Everything you loaded is searched at once, and results come back instantly — even across thousands of pages.",
      fastPoints: [
        "One search covers all of your loaded study material",
        "Documents are remembered, so the next search starts instantly",
        "Jump straight to the exact page instead of skimming",
      ],
      multiLangLabel: "MULTI-LANGUAGE & SMART UI",
      multiLangTitle: "Full language support & instant split-view reader.",
      multiLangBody:
        "Fully localized in English, Turkish, and Azerbaijani. Read search matches right inside the app using the split-view reader panel without launching external software.",
      multiLangPoints: [
        "Full dynamic localization in English, Turkish, and Azerbaijani (EN | TR | AZ)",
        "Split-view side panel for live page-by-page document previewing",
        "Drag-and-drop file imports and dynamic format filter chips",
      ],
      pomodoroLabel: "POMODORO",
      pomodoroTitle: "Stay focused on what matters.",
      pomodoroBody:
        "A focus timer lives right next to your notes. Study in 25-minute sessions, take short breaks, and see how many rounds you've finished — all without leaving the app.",
      pomodoroPoints: [
        "Focus sessions with short and long breaks",
        "Skip or reset a session whenever your plan changes",
        "Keep track of the pomodoros you completed today",
      ],
    },
    screenshots: {
      label: "SCREENSHOTS",
      title: "See E-Student in action.",
      subtitle:
        "A clean, familiar Windows interface — a sidebar for navigation, your files on the left and your results on the right.",
    },
    download: {
      title: "Ready to study smarter?",
      subtitle:
        "Download E-Student and keep your study materials searchable and organized.",
      downloadBtn: "Download for Windows",
      githubBtn: "View on GitHub",
      footerText: "Guided installer · Automatic updates · Free to use",
    },
    footer: {
      tagline:
        "Search. Organize. Focus. A lightweight Windows study companion for finding your documents fast and keeping your focus sessions on track.",
      downloadBtn: "Download for Windows",
      explore: "Explore",
      developer: "Developer",
      developerTitle: "Creator & maintainer of E-Student",
      feedback: "Send feedback",
      copyright: "© 2026 E-Student. All rights reserved.",
      madeWithLove: "Made with care for students · Free to use",
    },
  },
};

interface LanguageContextType {
  lang: Language;
  setLang: (lang: Language) => void;
  t: Translations;
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [lang, setLangState] = useState<Language>(() => {
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem("estudent_lang") as Language;
      if (saved && ["tr", "az", "en"].includes(saved)) return saved;
      const browserLang = navigator.language.toLowerCase();
      if (browserLang.startsWith("az")) return "az";
      if (browserLang.startsWith("tr")) return "tr";
    }
    return "tr"; // Default to Turkish for regional preference or fallback
  });

  const setLang = (newLang: Language) => {
    setLangState(newLang);
    if (typeof window !== "undefined") {
      localStorage.setItem("estudent_lang", newLang);
    }
  };

  useEffect(() => {
    document.documentElement.lang = lang;
  }, [lang]);

  return (
    <LanguageContext.Provider value={{ lang, setLang, t: translations[lang] }}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error("useLanguage must be used within a LanguageProvider");
  }
  return context;
};

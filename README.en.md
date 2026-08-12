# E-Student - Smart Student Assistant & Productivity Platform

[🇹🇷 Türkçe](README.md) | [🇬🇧 English](README.en.md) | [🇦🇿 Azərbaycanca](README.az.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**E-Student** is a modern Windows desktop assistant designed to help students manage their study materials, instantly search inside their documents, and boost productivity using the Pomodoro technique.

🌐 **Official Website**: [https://rzayevsahil.github.io/E-Student/](https://rzayevsahil.github.io/E-Student/)

---

## 🌟 Key Features

- 🔍 **Smart Document & Study Note Search**:
  - Upload PDF, Word (`.docx`, `.doc`), Excel (`.xlsx`, `.xls`), and PowerPoint (`.pptx`, `.ppt`) files.
  - Search inside lecture notes, presentation slides, terms, numbers, and tables in milliseconds.
  - Instant search across thousands of pages powered by a multi-threaded background engine and local text caching.

- 🌐 **Multi-Language UI Support (EN | TR | AZ)**:
  - English, Turkish, and Azerbaijani interface languages.
  - Instant dynamic language switching within the app with persistent language preference.

- 👁️ **Split-View Reader Panel**:
  - Preview search results page-by-page or slide-by-slide in the right-hand panel without leaving the application.
  - Quick page navigation (Previous/Next page) and option to open in external application.

- 📂 **Drag & Drop & Smart Filtering**:
  - Drag and drop files directly into the application from your desktop or folders.
  - Dynamic chip filters by file format (PDF, Word, Excel, PPT) and file name search.

- ⏱️ **Pomodoro Timer & Focus Management**:
  - Focus sessions (25 min), short (5 min), and long (15 min) break timers.
  - Completed Pomodoro statistics and customizable action buttons.

- ⚡ **High Performance & Text Caching**:
  - Document contents are cached after the first scan, making subsequent searches instant.

- 🔄 **Automatic Updates & Easy Installation**:
  - Fast guided installation powered by Inno Setup.
  - Auto-check for updates via GitHub Releases with silent background updates.

- 🎨 **Modern & Clean User Interface**:
  - User-friendly modern WPF interface, dark mode support, and intuitive navigation menu.

---

## 🛠️ Technology Stack

- **.NET 8.0 (Windows)** - Framework
- **WPF** - UI Framework
- **PdfPig** - PDF Processing
- **ClosedXML** - Excel Processing
- **DocumentFormat.OpenXml** - Word & PowerPoint Processing
- **Inno Setup** - Windows Setup Installer
- **CommunityToolkit.Mvvm** - MVVM Pattern
- **Microsoft.Extensions.DependencyInjection** - Dependency Injection

---

## 📦 Download and Installation

### 🚀 For Users (Quick Setup)

1. Download the latest **`E-Student-Setup-vX.X.X.exe`** installer from the [GitHub Releases](https://github.com/rzayevsahil/E-Student/releases) page.
2. Double-click the downloaded `.exe` file and follow the installation steps.
3. Launch the app using the **E-Student** shortcut created on your desktop or Start Menu.

### 💻 For Developers (Building from Source)

1. Clone the repository:
   ```bash
   git clone https://github.com/rzayevsahil/E-Student.git
   cd E-Student
   ```
2. Build the project:
   ```bash
   dotnet build DocumentSearch/DocumentSearch.csproj
   ```
3. Run the application:
   ```bash
   dotnet run --project DocumentSearch/DocumentSearch.csproj
   ```

---

## 💡 User Guide

1. **Document Search & Lecture Notes**: 
   - Use "Upload Files" to add your PDF, Word, Excel, and PowerPoint study materials.
   - Type any keyword into the search bar; matching results with page and slide numbers appear instantly.

2. **Pomodoro Focus Mode**:
   - Switch to the Pomodoro tab from the left sidebar, start your study timer, and adjust your break intervals.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE). See the [LICENSE](LICENSE) file for details.

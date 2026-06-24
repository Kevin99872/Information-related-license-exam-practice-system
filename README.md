<div align="center">

<img src="desktop/B3/Assets/app-icon-128.png" width="96" alt="Smart Exam System logo" />

# Smart Exam System

**An offline-first, cross-platform practice platform for IT certification exams — with a built-in code runner and an AI tutor.**

`TQC+` · `CPE` · `APCS` · `Technician Class C`

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-12.0-883AE3)](https://avaloniaui.net/)
[![SQLite](https://img.shields.io/badge/SQLite-local-003B57)](https://www.sqlite.org/)
[![Platforms](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-informational)]()

**English** · [繁體中文](README-CN.md)

</div>

---

## Why this project

As the software industry evolves, engineers are increasingly measured against certifications — LeetCode, CPE, TQC, and the like. But preparing for them is expensive and time-consuming: registration fees, prep-course costs, and hours that working professionals simply don't have.

**Smart Exam System** removes those barriers. Every question bank lives locally, the whole app runs without an internet connection, and an integrated AI tutor reviews your code — so you can practice anytime, on any OS, for free.

---

## Screenshots

### Home — Quick Simulation
Pick a certification and start practicing immediately. Popular banks show live progress.

![Home page](Assart/mainpage.png)

### Exam Setup
Confirm the bank, question count, time limit, and language before you begin.

![Exam setup](Assart/startpage.png)

### Exam in Progress — Built-in IDE
A real, runnable code workspace with a timer, randomized questions, a file pane, and a submit-and-verify button.

![Exam page](Assart/problemtestpage.png)

### Results & AI Tutor
Get your grade, accuracy, and time. Ask the AI questions or have it analyze your code for bugs and style issues — all in one panel.

![Result page](Assart/resultpage.png)

<details>
<summary><b>More screens</b> (question browser, bank management, import, settings)</summary>

<br/>

| Question Browser | Loaded Banks |
| :---: | :---: |
| ![Question list](Assart/problemtrainlist.png) | ![Loaded banks](Assart/problemdatalist.png) |
| Filter by exam type / difficulty / status, preview and edit solution code. | Track loaded banks, total questions, attempts, and overall accuracy. |

| Import Question Bank | Style Settings |
| :---: | :---: |
| ![Import](Assart/problemimport.png) | ![Style](Assart/programstyle.png) |
| Add single questions or bulk-import via CSV / XLS / XLSX / TXT. | Switch theme, font, and size with a live preview. |

| Exam Behavior | AI Model | Data Management |
| :---: | :---: | :---: |
| ![Exam settings](Assart/comfig.png) | ![AI settings](Assart/aiportconfig.png) | ![Data settings](Assart/problemconfig.png) |
| Toggle instant answers, countdown, shuffle, and difficulty. | Configure the Ollama endpoint or a local Transformers model. | Export records, back up banks, and set compiler paths. |

</details>

---

## Features

- ** Works fully offline** — Every question bank is stored locally and managed with **SQLite**. No network required.
- ** Truly cross-platform** — Built on **Avalonia**, a single release runs on **Windows, Linux, and macOS** (Intel & Apple Silicon).
- ** Multi-language code runner** — Compile and test against **Python, C/C++, and C#** runtimes; the judge runs your code and compares actual output against expected output.
- **Built-in AI tutor** — Connect a local **Ollama** model (default `qwen2.5-coder:7b`) or a local Transformers model. Ask coding questions and get one-click analysis of bugs and style problems — no API key required.
- ** Progress tracking** — Per-bank progress, attempt counts, and overall accuracy at a glance.
- ** Flexible import** — Add questions one at a time or bulk-import from `CSV` / `XLS` / `XLSX` / `TXT`, including multiple test cases per question.

---

## Quick Start

### Install (recommended)

1. Open the **[Releases](../../releases)** page.
2. Download the build for your OS (`windows` / `linux` / `macOS-x64` / `macOS-arm64`).
3. Unzip into a folder and run the app. **Done!**

### Configure compiler paths

Go to **Settings → Data Management → Runtime Environment** and set the paths for your platform:

| Runtime | Default | Notes |
| --- | --- | --- |
| Python | `python` / full path to `python.exe` | Required for Python questions |
| C++ | `g++` | Required for C/C++ questions |
| .NET | `dotnet` | Required for C# questions |

### (Optional) Enable the AI tutor

1. Install [Ollama](https://ollama.com/) and pull a model, e.g. `ollama pull qwen2.5-coder:7b`.
2. In **Settings → AI Model**, confirm the endpoint (`http://localhost:11434`) and model name.

---

## Build from Source

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/<your-account>/Information-related-license-exam-practice-system.git
cd Information-related-license-exam-practice-system/desktop/B3

# Run
dotnet run

# Publish for all platforms (from the repo root, via PowerShell)
pwsh ../../package.ps1
```

`package.ps1` produces self-contained zips for `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64` (macOS builds are wrapped into a `.app` bundle).

---

## Tech Stack

| Layer | Technology |
| --- | --- |
| UI framework | Avalonia 12.0 (Fluent theme), MVVM via CommunityToolkit.Mvvm |
| Runtime | .NET 10.0 |
| Database | SQLite via Entity Framework Core 8 |
| Spreadsheet import | NPOI |
| AI backend | Ollama (local) / local Transformers model |
| Code judging | External Python / g++ / dotnet processes |

---

## Importing Question Banks

The **Import** page supports two methods:

- **Single question** — Enter the problem code, exam type, title, description, and solution code, plus one or more test/verification data rows.
- **Bulk import** — Upload `.csv`, `.xls`, or `.xlsx`. Each row is one test case; rows sharing the same `ProblemCode` merge into a single question.

### Column specification

Build your import sheet in this column order:

```
ProblemCode, ExamType, Title, Description, Difficulty, Status,
SolutionLanguage, SolutionCode, OrderIndex, IsExample, TestInput, ExpectedOutput
```

### Example values

| Field | Example |
| --- | --- |
| `ProblemCode` | `PYD101` |
| `ExamType` | `TQC` |
| `Difficulty` | `1 / 2 / 3` or `Easy / Medium / Hard` |
| `Status` | `Draft` or `Active` |
| `IsExample` | `True / False` |

>  You can download a ready-made CSV template directly from the **Import** page, edit it in Excel / LibreOffice, and re-import it.

---

## Acknowledgements

- **EZTest** — English certification practice
- **TronClass** — university learning platform
- The certification-prep computer schools that inspired this project

---

<div align="center">
<sub>Built with Avalonia · Made for learners who'd rather practice than pay.</sub>
</div>

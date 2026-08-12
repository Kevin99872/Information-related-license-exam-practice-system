from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
APCS_FOLDER = ROOT / "APCS-implementation-problem-list"
OUTPUT_FILE = APCS_FOLDER / "APCS-implementation-problems.csv"

HEADER = [
    "ProblemCode",
    "ExamType",
    "Title",
    "Description",
    "Difficulty",
    "Status",
    "SolutionLanguage",
    "SolutionCode",
    "OrderIndex",
    "IsExample",
    "TestInput",
    "ExpectedOutput",
]

# First line of every problem file: "APCS 實作題 APCP001 <Title>"
TITLE_RE = re.compile(r"^APCS\s+實作題\s+(APCP\d+)\s+(.+)$", re.MULTILINE)
DIFFICULTY_RE = re.compile(r"難度：\s*(簡單|中等|困難)")
TOPIC_RE = re.compile(r"主題：\s*(.+)")

# Generic "== <marker name> ==" section header, e.g. "== 題目 ==",
# "== 參考解法 (Python) ==", "== 範例 1 輸入 ==", "== 範例 1 輸出 ==".
MARKER_RE = re.compile(r"^==\s*(.+?)\s*==[ \t]*\n", re.MULTILINE)

EXAMPLE_INPUT_RE = re.compile(r"^範例\s*(\d+)\s*輸入$")
EXAMPLE_OUTPUT_RE = re.compile(r"^範例\s*(\d+)\s*輸出$")

DIFFICULTY_MAP = {"簡單": 1, "中等": 2, "困難": 3}


def normalize_block(text: str) -> str:
    """Normalize a prose block (Description): unify newlines, strip trailing
    whitespace per line, collapse 3+ blank lines to 2, strip outer blanks."""
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    text = re.sub(r"[ \t]+$", "", text, flags=re.MULTILINE)
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip("\n")


def normalize_code(text: str) -> str:
    """Normalize a code block (SolutionCode): unify newlines, trim only the
    leading/trailing blank separator lines. Internal indentation and blank
    lines within the code are preserved exactly (Python is whitespace
    sensitive)."""
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    return text.strip("\n")


def normalize_example(text: str) -> str:
    """Normalize an example input/output block: unify newlines, trim only the
    leading/trailing blank separator lines. Internal blank lines are kept
    since they can be meaningful test data (e.g. an empty array line)."""
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    return text.strip("\n")


def parse_sections(content: str) -> dict[str, str]:
    """Split the file body into named sections keyed by the '== name ==' markers.
    Returns a dict mapping marker name -> raw section text (everything up to
    the next marker or end of file)."""
    matches = list(MARKER_RE.finditer(content))
    sections: dict[str, str] = {}
    for i, m in enumerate(matches):
        name = m.group(1)
        start = m.end()
        end = matches[i + 1].start() if i + 1 < len(matches) else len(content)
        sections[name] = content[start:end]
    return sections


def parse_problem_file(txt_path: Path) -> dict:
    content = txt_path.read_text(encoding="utf-8")
    problem_code = txt_path.stem

    title_match = TITLE_RE.search(content)
    if not title_match:
        raise ValueError(f"{txt_path.name}: unable to parse title line")
    file_code, title = title_match.group(1), title_match.group(2).strip()
    if file_code != problem_code:
        raise ValueError(
            f"{txt_path.name}: title line code '{file_code}' does not match filename"
        )

    difficulty_match = DIFFICULTY_RE.search(content)
    if not difficulty_match:
        raise ValueError(f"{txt_path.name}: unable to parse 難度")
    difficulty = DIFFICULTY_MAP[difficulty_match.group(1)]

    topic_match = TOPIC_RE.search(content)
    topic = topic_match.group(1).strip() if topic_match else ""

    sections = parse_sections(content)

    if "題目" not in sections:
        raise ValueError(f"{txt_path.name}: missing == 題目 == section")
    description = normalize_block(sections["題目"])

    solution_key = next((k for k in sections if k.startswith("參考解法")), None)
    if solution_key is None:
        raise ValueError(f"{txt_path.name}: missing == 參考解法 (Python) == section")
    solution_code = normalize_code(sections[solution_key])

    examples: dict[int, dict[str, str]] = {}
    for name, text in sections.items():
        m_in = EXAMPLE_INPUT_RE.match(name)
        m_out = EXAMPLE_OUTPUT_RE.match(name)
        if m_in:
            idx = int(m_in.group(1))
            examples.setdefault(idx, {})["input"] = normalize_example(text)
        elif m_out:
            idx = int(m_out.group(1))
            examples.setdefault(idx, {})["output"] = normalize_example(text)

    if not examples:
        raise ValueError(f"{txt_path.name}: no examples found")

    ordered_examples = []
    for idx in sorted(examples):
        pair = examples[idx]
        if "input" not in pair or "output" not in pair:
            raise ValueError(f"{txt_path.name}: 範例 {idx} missing input or output")
        ordered_examples.append((pair["input"], pair["output"]))

    return {
        "problem_code": problem_code,
        "title": title,
        "difficulty": difficulty,
        "topic": topic,
        "description": description,
        "solution_code": solution_code,
        "examples": ordered_examples,
    }


def main() -> None:
    rows: list[list[object]] = []
    txt_paths = sorted(APCS_FOLDER.glob("APCP*.txt"))
    for txt_path in txt_paths:
        problem = parse_problem_file(txt_path)
        for order_index, (test_input, expected_output) in enumerate(problem["examples"]):
            rows.append([
                problem["problem_code"],
                "APCS",
                problem["title"],
                problem["description"],
                problem["difficulty"],
                "Active",
                "Python",
                problem["solution_code"],
                order_index,
                True,
                test_input,
                expected_output,
            ])

    with OUTPUT_FILE.open("w", encoding="utf-8", newline="") as csv_file:
        writer = csv.writer(csv_file, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(HEADER)
        writer.writerows(rows)

    print(
        f"Generated {OUTPUT_FILE} with {len(rows)} data rows "
        f"from {len(txt_paths)} problem files."
    )


if __name__ == "__main__":
    main()

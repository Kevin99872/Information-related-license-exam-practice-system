from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CPE_FOLDER = ROOT / "CPE-problem-list"
OUTPUT_FILE = CPE_FOLDER / "CPE-problems.csv"

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

# First line looks like "CPE 10922 2 the 9s" or "CPE Matrix Rotation" (UVa number omitted).
TITLE_RE = re.compile(r"^CPE\s+(?:\d+\s+)?(.+)$")

DIFFICULTY_RE = re.compile(r"^難度：(簡單|中等|困難)\s*$", re.MULTILINE)

# Description = everything between "== 題目 ==" and whichever of
# "== 參考解法 (Python) ==" / "== 範例 N 輸入 ==" comes first.
DESCRIPTION_RE = re.compile(
    r"== 題目 ==\n(.*?)\n(?:== 參考解法 \(Python\) ==|== 範例 \d+ 輸入 ==)",
    re.DOTALL,
)

# Optional reference-solution block, present only on some (mostly new) problems.
SOLUTION_RE = re.compile(
    r"== 參考解法 \(Python\) ==\n(.*?)\n(?:== 範例 \d+ 輸入 ==|\Z)",
    re.DOTALL,
)

# One block per example: "== 範例 N 輸入 ==" ... "== 範例 N 輸出 ==" ... up to the
# next "== 範例 M 輸入 ==" marker or end of file.
EXAMPLE_RE = re.compile(
    r"== 範例 (\d+) 輸入 ==\n(.*?)\n== 範例 \1 輸出 ==\n(.*?)(?=\n== 範例 \d+ 輸入 ==|\Z)",
    re.DOTALL,
)

DIFFICULTY_MAP = {"簡單": 1, "中等": 2, "困難": 3}


def normalize_block(text: str) -> str:
    """Normalize a description block: unify newlines, strip trailing
    whitespace on each line, collapse 3+ blank lines to 2, and trim the
    block's own leading/trailing blank lines."""
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = re.sub(r"[ \t]+$", "", text, flags=re.MULTILINE)
    return text.strip()


def normalize_example_block(text: str) -> str:
    """Normalize a TestInput/ExpectedOutput block: unify newlines and trim
    only trailing newlines. Leading/internal blank lines are preserved
    verbatim since they can be meaningful test data (e.g. an empty-string
    test case)."""
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    return text.rstrip("\n")


def extract_title(content: str) -> str:
    first_line = content.split("\n", 1)[0].strip()
    match = TITLE_RE.match(first_line)
    if not match:
        raise ValueError(f"Unable to parse title from line: {first_line!r}")
    return match.group(1).strip()


def extract_difficulty(content: str) -> int:
    match = DIFFICULTY_RE.search(content)
    if not match:
        raise ValueError("Unable to parse difficulty")
    return DIFFICULTY_MAP[match.group(1)]


def extract_description(content: str) -> str:
    match = DESCRIPTION_RE.search(content)
    if not match:
        raise ValueError("Unable to parse description")
    return normalize_block(match.group(1))


def extract_solution_code(content: str) -> str:
    match = SOLUTION_RE.search(content)
    if not match:
        return ""
    return match.group(1).replace("\r\n", "\n").replace("\r", "\n").rstrip("\n")


def extract_examples(content: str) -> list[tuple[str, str]]:
    examples: list[tuple[str, str]] = []
    for match in EXAMPLE_RE.finditer(content):
        input_block = normalize_example_block(match.group(2))
        output_block = normalize_example_block(match.group(3))
        examples.append((input_block, output_block))
    return examples


if __name__ == "__main__":
    rows: list[list[object]] = []
    txt_paths = sorted(CPE_FOLDER.glob("CPE*.txt"))
    for txt_path in txt_paths:
        problem_code = txt_path.stem
        content = txt_path.read_text(encoding="utf-8")

        title = extract_title(content)
        difficulty = extract_difficulty(content)
        description = extract_description(content)
        solution_code = extract_solution_code(content)

        examples = extract_examples(content)
        if not examples:
            examples = [("", "")]

        for order_index, (test_input, expected_output) in enumerate(examples):
            rows.append([
                problem_code,
                "CPE",
                title,
                description,
                difficulty,
                "Active",
                "Python",
                solution_code,
                order_index,
                order_index == 0,
                test_input,
                expected_output,
            ])

    with OUTPUT_FILE.open("w", encoding="utf-8", newline="") as csv_file:
        writer = csv.writer(csv_file, quoting=csv.QUOTE_MINIMAL, lineterminator="\n")
        writer.writerow(HEADER)
        writer.writerows(rows)

    print(f"Generated {OUTPUT_FILE} with {len(rows)} data rows from {len(txt_paths)} problem files.")

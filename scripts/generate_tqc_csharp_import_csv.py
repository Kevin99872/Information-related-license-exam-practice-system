from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CSHARP_FOLDER = ROOT / "TQC-CSharp-problem-list"
OUTPUT_FILE = CSHARP_FOLDER / "TQC-CSharp-problems.csv"

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

TITLE_RE = re.compile(r"TQC\+\s+程式語言C#\s+CSD\d+\s+(.+?)\s*$", re.MULTILINE)
DIFFICULTY_RE = re.compile(r"^難度：(簡單|中等|困難)\s*$", re.MULTILINE)
DESCRIPTION_RE = re.compile(
    r"== 題目 ==\n(.*?)\n== 參考解法 \(C#\) ==",
    re.DOTALL,
)
SOLUTION_RE = re.compile(
    r"== 參考解法 \(C#\) ==\n(.*?)\n== 範例 1 輸入 ==",
    re.DOTALL,
)
EXAMPLE_RE = re.compile(
    r"== 範例 (\d+) 輸入 ==\n(.*?)\n== 範例 \1 輸出 ==\n(.*?)(?=\n== 範例 \d+ 輸入 ==|\Z)",
    re.DOTALL,
)

DIFFICULTY_MAP = {"簡單": 1, "中等": 2, "困難": 3}


def normalize_block(text: str) -> str:
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    return text.strip("\n")


def extract_title(content: str) -> str:
    match = TITLE_RE.search(content)
    if not match:
        raise ValueError("Unable to parse title")
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


def extract_solution(content: str) -> str:
    match = SOLUTION_RE.search(content)
    if not match:
        raise ValueError("Unable to parse solution code")
    return normalize_block(match.group(1))


def extract_examples(content: str) -> list[tuple[str, str]]:
    examples: list[tuple[str, str]] = []
    for match in EXAMPLE_RE.finditer(content):
        test_input = normalize_block(match.group(2))
        expected_output = normalize_block(match.group(3))
        examples.append((test_input, expected_output))
    return examples


if __name__ == "__main__":
    rows: list[list[object]] = []
    txt_files = sorted(CSHARP_FOLDER.glob("CSD*.txt"))
    for txt_path in txt_files:
        problem_code = txt_path.stem
        content = txt_path.read_text(encoding="utf-8")

        title = extract_title(content)
        difficulty = extract_difficulty(content)
        description = extract_description(content)
        solution = extract_solution(content)
        examples = extract_examples(content)

        if not examples:
            raise ValueError(f"{problem_code}: no examples found")

        for order_index, (test_input, expected_output) in enumerate(examples):
            rows.append([
                problem_code,
                "TQC",
                title,
                description,
                difficulty,
                "Active",
                "C#",
                solution,
                order_index,
                True,
                test_input,
                expected_output,
            ])

    with OUTPUT_FILE.open("w", encoding="utf-8", newline="") as csv_file:
        writer = csv.writer(csv_file, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(HEADER)
        writer.writerows(rows)

    print(f"Generated {OUTPUT_FILE} with {len(rows)} data rows from {len(txt_files)} problem files.")

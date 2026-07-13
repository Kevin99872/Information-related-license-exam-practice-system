from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
TQC_FOLDER = ROOT / "TQC-problem-list"
OUTPUT_FILE = TQC_FOLDER / "TQC-problems.csv"

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

TITLE_RE = re.compile(r"TQC\+\s+程式語言Python\s+\d+\s+(.+)$", re.MULTILINE)
DESCRIPTION_RE = re.compile(
    r"1\.\s*題目說明:\s*(.*?)(?=2\.\s*設計說明:)",
    re.DOTALL | re.IGNORECASE,
)
EXAMPLE_RE = re.compile(
    r"範例輸入(?:\d+)?\s*(.*?)\s*範例輸出(?:\d+)?\s*(.*?)(?=範例輸入(?:\d+)?\s*|\Z)",
    re.DOTALL | re.IGNORECASE,
)


def normalize_block(text: str) -> str:
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = re.sub(r"^[ \t]+|[ \t]+$", "", text, flags=re.MULTILINE)
    return text.strip()


def extract_title(content: str) -> str:
    match = TITLE_RE.search(content)
    if not match:
        raise ValueError("Unable to parse title")
    return match.group(1).strip()


def extract_description(content: str) -> str:
    match = DESCRIPTION_RE.search(content)
    if match:
        desc = match.group(1)
    else:
        desc = content[:500]
    return normalize_block(desc)


def extract_examples(content: str) -> list[tuple[str, str]]:
    examples: list[tuple[str, str]] = []
    for match in EXAMPLE_RE.finditer(content):
        input_block = normalize_block(match.group(1))
        output_block = normalize_block(match.group(2))
        if input_block and output_block:
            examples.append((input_block, output_block))
    return examples


def difficulty_from_problem_code(problem_code: str) -> int:
    match = re.search(r"(\d+)$", problem_code)
    if not match:
        return 2
    number = int(match.group(1))
    if number >= 400:
        return 3
    if number >= 200:
        return 2
    return 1


if __name__ == "__main__":
    rows: list[list[object]] = []
    for txt_path in sorted(TQC_FOLDER.glob("PYD*.txt")):
        problem_code = txt_path.stem
        content = txt_path.read_text(encoding="utf-8")
        title = extract_title(content)
        description = extract_description(content)
        difficulty = difficulty_from_problem_code(problem_code)

        examples = extract_examples(content)
        if not examples:
            examples = [("", "")]

        for order_index, (test_input, expected_output) in enumerate(examples):
            rows.append([
                problem_code,
                "TQC",
                title,
                description,
                difficulty,
                "Active",
                "Python",
                "",
                order_index,
                True,
                test_input,
                expected_output,
            ])

    with OUTPUT_FILE.open("w", encoding="utf-8", newline="") as csv_file:
        writer = csv.writer(csv_file, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(HEADER)
        writer.writerows(rows)

    print(f"Generated {OUTPUT_FILE} with {len(rows)} data rows from {len(list(TQC_FOLDER.glob('PYD*.txt')))} problem files.")

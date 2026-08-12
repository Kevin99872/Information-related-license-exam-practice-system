from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SWC_FOLDER = ROOT / "SWD-C-problem-list"
OUTPUT_FILE = SWC_FOLDER / "SWD-C-problems.csv"

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

EXAM_TYPE = "Software"

TITLE_RE = re.compile(r"^電腦軟體設計丙級\s+練習題\s+(SWC\d+)\s+(.+)$")
DIFFICULTY_RE = re.compile(r"難度：(簡單|中等|困難)")
DIFFICULTY_MAP = {"簡單": 1, "中等": 2, "困難": 3}

STATEMENT_MARKER = "== 題目 =="
SOLUTION_MARKER = "== 參考解法 (Python) =="
EXAMPLE_INPUT_RE = re.compile(r"^== 範例 (\d+) 輸入 ==$")
EXAMPLE_OUTPUT_RE = re.compile(r"^== 範例 (\d+) 輸出 ==$")


def strip_blank_edges(lines: list[str]) -> str:
    """Join lines with \n, stripping leading/trailing fully-blank lines only
    (internal blank lines are preserved, e.g. blank line between 題目 and
    輸入說明/輸出說明)."""
    start = 0
    end = len(lines)
    while start < end and lines[start].strip() == "":
        start += 1
    while end > start and lines[end - 1].strip() == "":
        end -= 1
    return "\n".join(lines[start:end])


def parse_problem(txt_path: Path):
    raw = txt_path.read_text(encoding="utf-8")
    lines = raw.replace("\r\n", "\n").replace("\r", "\n").split("\n")

    title_match = TITLE_RE.match(lines[0].strip())
    if not title_match:
        raise ValueError(f"{txt_path}: cannot parse title line: {lines[0]!r}")
    problem_code, title = title_match.group(1), title_match.group(2).strip()

    diff_match = None
    for line in lines[:6]:
        diff_match = DIFFICULTY_RE.search(line)
        if diff_match:
            break
    if not diff_match:
        raise ValueError(f"{txt_path}: cannot find 難度 line")
    difficulty = DIFFICULTY_MAP[diff_match.group(1)]

    # locate marker line indices
    stmt_idx = next(i for i, l in enumerate(lines) if l.strip() == STATEMENT_MARKER)
    sol_idx = next(i for i, l in enumerate(lines) if l.strip() == SOLUTION_MARKER)

    example_markers: list[tuple[int, str, int]] = []  # (line_idx, kind, number)
    for i, l in enumerate(lines):
        m = EXAMPLE_INPUT_RE.match(l.strip())
        if m:
            example_markers.append((i, "input", int(m.group(1))))
            continue
        m = EXAMPLE_OUTPUT_RE.match(l.strip())
        if m:
            example_markers.append((i, "output", int(m.group(1))))

    if not example_markers:
        raise ValueError(f"{txt_path}: no examples found")

    description = strip_blank_edges(lines[stmt_idx + 1 : sol_idx])

    first_example_idx = example_markers[0][0]
    solution_lines = lines[sol_idx + 1 : first_example_idx]
    # strip only fully-blank leading/trailing lines; keep code indentation intact
    s, e = 0, len(solution_lines)
    while s < e and solution_lines[s].strip() == "":
        s += 1
    while e > s and solution_lines[e - 1].strip() == "":
        e -= 1
    solution_code = "\n".join(solution_lines[s:e])

    # pair up input/output markers by example number, in document order
    examples: dict[int, dict[str, str]] = {}
    for idx, (line_idx, kind, number) in enumerate(example_markers):
        next_idx = example_markers[idx + 1][0] if idx + 1 < len(example_markers) else len(lines)
        block = strip_blank_edges(lines[line_idx + 1 : next_idx])
        examples.setdefault(number, {})[kind] = block

    ordered_examples = []
    for number in sorted(examples):
        pair = examples[number]
        if "input" not in pair or "output" not in pair:
            raise ValueError(f"{txt_path}: example {number} missing input or output block")
        ordered_examples.append((pair["input"], pair["output"]))

    return {
        "code": problem_code,
        "title": title,
        "difficulty": difficulty,
        "description": description,
        "solution": solution_code,
        "examples": ordered_examples,
    }


if __name__ == "__main__":
    rows: list[list[object]] = []
    txt_files = sorted(SWC_FOLDER.glob("SWC*.txt"))
    for txt_path in txt_files:
        problem = parse_problem(txt_path)
        for order_index, (test_input, expected_output) in enumerate(problem["examples"]):
            rows.append([
                problem["code"],
                EXAM_TYPE,
                problem["title"],
                problem["description"],
                problem["difficulty"],
                "Active",
                "Python",
                problem["solution"],
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

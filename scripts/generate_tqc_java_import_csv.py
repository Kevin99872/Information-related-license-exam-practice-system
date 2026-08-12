from __future__ import annotations

import csv
import re
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
JAVA_FOLDER = ROOT / "TQC-Java-problem-list"
OUTPUT_FILE = JAVA_FOLDER / "TQC-Java-problems.csv"

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

TITLE_RE = re.compile(r"TQC\+\s+程式語言Java\s+(JAD\d+)\s+(.+)$", re.MULTILINE)
DIFFICULTY_RE = re.compile(r"難度：\s*(簡單|中等|困難)")
TOPIC_RE = re.compile(r"主題：\s*(.+)$", re.MULTILINE)
DESCRIPTION_RE = re.compile(
    r"==\s*題目\s*==\s*(.*?)(?===\s*參考解法)",
    re.DOTALL,
)
SOLUTION_RE = re.compile(
    r"==\s*參考解法\s*\(Java\)\s*==\s*(.*?)(?===\s*範例)",
    re.DOTALL,
)
EXAMPLE_RE = re.compile(
    r"==[ \t]*範例[ \t]*(\d+)[ \t]*輸入[ \t]*==[ \t]*\r?\n(.*?)\r?\n==[ \t]*範例[ \t]*\d+[ \t]*輸出[ \t]*==[ \t]*\r?\n(.*?)(?=\r?\n==[ \t]*範例[ \t]*\d+[ \t]*輸入[ \t]*==|\Z)",
    re.DOTALL,
)

DIFFICULTY_MAP = {"簡單": 1, "中等": 2, "困難": 3}


def normalize_block(text: str) -> str:
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    return text.strip("\n").rstrip()


def extract_title(content: str, code: str) -> str:
    match = TITLE_RE.search(content)
    if not match:
        raise ValueError(f"{code}: unable to parse title")
    return match.group(2).strip()


def extract_difficulty(content: str, code: str) -> int:
    match = DIFFICULTY_RE.search(content)
    if not match:
        raise ValueError(f"{code}: unable to parse difficulty")
    return DIFFICULTY_MAP[match.group(1)]


def extract_description(content: str, code: str) -> str:
    match = DESCRIPTION_RE.search(content)
    if not match:
        raise ValueError(f"{code}: unable to parse description")
    return normalize_block(match.group(1))


def extract_solution(content: str, code: str) -> str:
    match = SOLUTION_RE.search(content)
    if not match:
        raise ValueError(f"{code}: unable to parse solution code")
    return normalize_block(match.group(1))


def extract_examples(content: str, code: str) -> list[tuple[str, str]]:
    examples: list[tuple[str, str]] = []
    for match in EXAMPLE_RE.finditer(content):
        input_block = normalize_block(match.group(2))
        output_block = normalize_block(match.group(3))
        examples.append((input_block, output_block))
    if not examples:
        raise ValueError(f"{code}: no examples parsed")
    return examples


def parse_problem_file(txt_path: Path) -> dict:
    code = txt_path.stem
    content = txt_path.read_text(encoding="utf-8")
    return {
        "code": code,
        "title": extract_title(content, code),
        "difficulty": extract_difficulty(content, code),
        "description": extract_description(content, code),
        "solution": extract_solution(content, code),
        "examples": extract_examples(content, code),
    }


def compile_and_run(solution_code: str, test_input: str, workdir: Path) -> tuple[bool, str]:
    """Compile solution_code as Main.java in workdir and run it with test_input on stdin.
    Returns (success, stdout_or_error)."""
    main_java = workdir / "Main.java"
    main_java.write_text(solution_code, encoding="utf-8")

    compile_proc = subprocess.run(
        ["javac", "Main.java"],
        cwd=workdir,
        capture_output=True,
        text=True,
        timeout=60,
    )
    if compile_proc.returncode != 0:
        return False, f"COMPILE ERROR:\n{compile_proc.stderr}"

    run_proc = subprocess.run(
        ["java", "Main"],
        cwd=workdir,
        input=test_input,
        capture_output=True,
        text=True,
        timeout=20,
    )
    if run_proc.returncode != 0 and not run_proc.stdout:
        return False, f"RUNTIME ERROR:\n{run_proc.stderr}"

    return True, run_proc.stdout


def verify_problem(problem: dict, verbose: bool = True) -> tuple[int, int]:
    """Compile once, run each example, compare against expected output.
    Returns (passed_count, total_count)."""
    passed = 0
    total = len(problem["examples"])
    with tempfile.TemporaryDirectory(prefix=f"jad_{problem['code']}_") as tmp:
        workdir = Path(tmp)
        main_java = workdir / "Main.java"
        main_java.write_text(problem["solution"], encoding="utf-8")
        compile_proc = subprocess.run(
            ["javac", "Main.java"], cwd=workdir, capture_output=True, text=True, timeout=60
        )
        if compile_proc.returncode != 0:
            if verbose:
                print(f"[FAIL] {problem['code']}: COMPILE ERROR\n{compile_proc.stderr}")
            return 0, total

        for idx, (test_input, expected_output) in enumerate(problem["examples"], start=1):
            run_proc = subprocess.run(
                ["java", "Main"],
                cwd=workdir,
                input=test_input,
                capture_output=True,
                text=True,
                timeout=20,
            )
            actual = run_proc.stdout.rstrip("\n").rstrip("\r")
            expected = expected_output.rstrip("\n").rstrip("\r")
            # also strip trailing whitespace on each line, consistently
            actual_norm = "\n".join(line.rstrip() for line in actual.split("\n"))
            expected_norm = "\n".join(line.rstrip() for line in expected.split("\n"))
            if actual_norm == expected_norm:
                passed += 1
            else:
                if verbose:
                    print(
                        f"[FAIL] {problem['code']} example {idx}\n"
                        f"  input: {test_input!r}\n"
                        f"  expected: {expected_norm!r}\n"
                        f"  actual:   {actual_norm!r}\n"
                        f"  stderr: {run_proc.stderr}"
                    )
    return passed, total


def build_rows(problems: list[dict]) -> list[list[object]]:
    rows: list[list[object]] = []
    for problem in problems:
        for order_index, (test_input, expected_output) in enumerate(problem["examples"]):
            rows.append(
                [
                    problem["code"],
                    "TQC",
                    problem["title"],
                    problem["description"],
                    problem["difficulty"],
                    "Active",
                    "Java",
                    problem["solution"],
                    order_index,
                    "True",
                    test_input,
                    expected_output,
                ]
            )
    return rows


def write_csv(rows: list[list[object]]) -> None:
    with OUTPUT_FILE.open("w", encoding="utf-8", newline="") as csv_file:
        writer = csv.writer(csv_file, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(HEADER)
        writer.writerows(rows)


def reverify_csv() -> tuple[int, int]:
    """Final automated re-verification pass: read the generated CSV and
    re-compile/re-run each row's SolutionCode+TestInput, asserting output == ExpectedOutput."""
    passed = 0
    total = 0
    failures: list[str] = []
    with OUTPUT_FILE.open("r", encoding="utf-8", newline="") as csv_file:
        reader = csv.DictReader(csv_file)
        rows = list(reader)

    # Group by ProblemCode so we only compile once per problem.
    by_code: dict[str, list[dict]] = {}
    for row in rows:
        by_code.setdefault(row["ProblemCode"], []).append(row)

    for code, code_rows in by_code.items():
        solution = code_rows[0]["SolutionCode"]
        with tempfile.TemporaryDirectory(prefix=f"jadcsv_{code}_") as tmp:
            workdir = Path(tmp)
            main_java = workdir / "Main.java"
            main_java.write_text(solution, encoding="utf-8")
            compile_proc = subprocess.run(
                ["javac", "Main.java"], cwd=workdir, capture_output=True, text=True, timeout=60
            )
            if compile_proc.returncode != 0:
                total += len(code_rows)
                failures.append(f"{code}: COMPILE ERROR\n{compile_proc.stderr}")
                continue

            for row in code_rows:
                total += 1
                run_proc = subprocess.run(
                    ["java", "Main"],
                    cwd=workdir,
                    input=row["TestInput"],
                    capture_output=True,
                    text=True,
                    timeout=20,
                )
                actual = run_proc.stdout.rstrip("\n").rstrip("\r")
                expected = row["ExpectedOutput"].rstrip("\n").rstrip("\r")
                actual_norm = "\n".join(line.rstrip() for line in actual.split("\n"))
                expected_norm = "\n".join(line.rstrip() for line in expected.split("\n"))
                if actual_norm == expected_norm:
                    passed += 1
                else:
                    failures.append(
                        f"{code} order={row['OrderIndex']}: expected={expected_norm!r} actual={actual_norm!r} stderr={run_proc.stderr}"
                    )

    for failure in failures:
        print(f"[CSV RE-VERIFY FAIL] {failure}")

    return passed, total


if __name__ == "__main__":
    txt_files = sorted(JAVA_FOLDER.glob("JAD*.txt"))
    problems = [parse_problem_file(p) for p in txt_files]

    do_verify = "--no-verify" not in sys.argv

    if do_verify:
        print(f"Verifying {len(problems)} problem files by compiling & running each example...")
        total_passed = 0
        total_examples = 0
        failing_codes = []
        for problem in problems:
            passed, total = verify_problem(problem)
            total_passed += passed
            total_examples += total
            if passed != total:
                failing_codes.append(problem["code"])
        print(f"Pre-generation verification: {total_passed}/{total_examples} examples passed.")
        if failing_codes:
            print(f"Problems with failures: {failing_codes}")
            sys.exit(1)

    rows = build_rows(problems)
    write_csv(rows)
    print(f"Generated {OUTPUT_FILE} with {len(rows)} data rows from {len(problems)} problem files.")

    if do_verify:
        print("Running final CSV re-verification pass (reading CSV, recompiling, rerunning)...")
        passed, total = reverify_csv()
        print(f"CSV re-verification: {passed}/{total} rows passed.")
        if passed != total:
            sys.exit(1)

from __future__ import annotations

import csv
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
APCC_FOLDER = ROOT / "APCS-concept-problem-list"
OUTPUT_FILE = APCC_FOLDER / "APCS-concept-problems.csv"

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

# 每一份 .txt 的固定格式（見 APCS-concept-problem-list/README.md）：
#
#   APCS 觀念題 APCC001 <標題>
#
#   難度：簡單|中等|困難
#   主題：<主題>
#
#   == 題目 ==
#   <題目敘述 + (A)~(D) 選項>
#
#   == 正解 ==
#   <A|B|C|D>
#
#   == 解析 ==
#   <解析文字，不會被匯入 Description，避免洩題>

TITLE_RE = re.compile(r"^APCS 觀念題 APCC\d+ (.+)$", re.MULTILINE)
DIFFICULTY_RE = re.compile(r"^難度：(簡單|中等|困難)\s*$", re.MULTILINE)
# Description = 從「== 題目 ==」之後，到「== 正解 ==」之前（也就是題目敘述 + 選項），
# 明確排除 == 正解 == 與 == 解析 == 兩個區塊，避免洩題。
DESCRIPTION_RE = re.compile(
    r"== 題目 ==\s*\n(.*?)\n== 正解 ==",
    re.DOTALL,
)
ANSWER_RE = re.compile(r"== 正解 ==\s*\n\s*([ABCD])\s*$", re.MULTILINE)

DIFFICULTY_MAP = {"簡單": 1, "中等": 2, "困難": 3}

# TestInput 不可為空字串（匯入驗證會直接拒絕整列，見
# desktop/B3/Services/ProblemImportService.cs 的 ValidateSpreadsheetRow）。
# 這些題目本身不需要任何標準輸入（SolutionCode 是 print("<letter>")，
# 完全不會讀取 stdin），因此固定填入一個無意義的佔位字元，僅用於通過匯入驗證。
TEST_INPUT_PLACEHOLDER = "-"


def normalize_block(text: str) -> str:
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    text = re.sub(r"\n{3,}", "\n\n", text)
    text = re.sub(r"[ \t]+$", "", text, flags=re.MULTILINE)
    return text.strip()


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
        raise ValueError("Unable to parse description (== 題目 == .. == 正解 == block)")
    return normalize_block(match.group(1))


def extract_answer(content: str) -> str:
    match = ANSWER_RE.search(content)
    if not match:
        raise ValueError("Unable to parse answer letter")
    return match.group(1)


if __name__ == "__main__":
    rows: list[list[object]] = []
    txt_files = sorted(APCC_FOLDER.glob("APCC*.txt"))
    for txt_path in txt_files:
        problem_code = txt_path.stem
        content = txt_path.read_text(encoding="utf-8")

        title = extract_title(content)
        difficulty = extract_difficulty(content)
        description = extract_description(content)
        answer_letter = extract_answer(content)

        # 保險檢查：Description 不可含有正解/解析區塊的痕跡（避免洩題）。
        if "== 正解 ==" in description or "== 解析 ==" in description:
            raise ValueError(f"{problem_code}: Description leaked 正解/解析 section")

        rows.append([
            problem_code,
            "APCS",
            title,
            description,
            difficulty,
            "Active",
            "Python",
            f'print("{answer_letter}")',
            0,
            False,
            TEST_INPUT_PLACEHOLDER,
            answer_letter,
        ])

    with OUTPUT_FILE.open("w", encoding="utf-8", newline="") as csv_file:
        writer = csv.writer(csv_file, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(HEADER)
        writer.writerows(rows)

    print(f"Generated {OUTPUT_FILE} with {len(rows)} data rows from {len(txt_files)} problem files.")

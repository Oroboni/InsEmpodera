from __future__ import annotations

import re
import sys
from pathlib import Path


CREATE_TABLE_RE = re.compile(
    r"(?is)(CREATE\s+TABLE\s+`(?P<table>[^`]+)`\s*\()(?P<body>.*?)(\)\s*ENGINE=)"
)
ALTER_TABLE_RE = re.compile(
    r"(?is)ALTER\s+TABLE\s+`(?P<table>[^`]+)`(?P<body>.*?);"
)
PRIMARY_KEY_RE = re.compile(r"(?i)ADD\s+PRIMARY\s+KEY\s*\(\s*`(?P<column>[^`]+)`\s*\)")
FOREIGN_KEY_RE = re.compile(r"(?i)FOREIGN\s+KEY\s*\(\s*`(?P<column>[^`]+)`\s*\)")
INSERT_START_RE = re.compile(r"(?i)INSERT\s+INTO\s+`(?P<table>[^`]+)`")


def collect_identifier_columns(sql: str) -> tuple[dict[str, set[str]], dict[str, set[str]]]:
    primary_keys: dict[str, set[str]] = {}
    foreign_keys: dict[str, set[str]] = {}

    for match in ALTER_TABLE_RE.finditer(sql):
        table = match.group("table")
        body = match.group("body")
        primary_keys.setdefault(table, set()).update(
            key.group("column") for key in PRIMARY_KEY_RE.finditer(body)
        )
        foreign_keys.setdefault(table, set()).update(
            key.group("column") for key in FOREIGN_KEY_RE.finditer(body)
        )

    return primary_keys, foreign_keys


def rewrite_table_definitions(
    sql: str,
    primary_keys: dict[str, set[str]],
    foreign_keys: dict[str, set[str]],
) -> str:
    def rewrite_table(match: re.Match[str]) -> str:
        table = match.group("table")
        body = match.group("body")
        primary = primary_keys.get(table, set())
        identifiers = primary | foreign_keys.get(table, set())

        for column in sorted(identifiers, key=len, reverse=True):
            definition = re.compile(
                rf"(?i)(?P<prefix>`{re.escape(column)}`\s+)"
                r"(?P<type>int\s*\(\s*11\s*\)|binary\s*\(\s*16\s*\))"
                r"(?P<tail>[^\r\n]*)"
            )

            def rewrite_column(column_match: re.Match[str]) -> str:
                tail = column_match.group("tail")
                comma = "," if tail.rstrip().endswith(",") else ""
                tail_without_comma = tail.rstrip()
                if comma:
                    tail_without_comma = tail_without_comma[:-1].rstrip()

                tail_without_comma = re.sub(
                    r"(?i)\s+DEFAULT\s*\(\s*floor\s*\(\s*100000\s*\+\s*rand\s*\(\s*\)\s*\*\s*900000\s*\)\s*\)",
                    "",
                    tail_without_comma,
                )

                if column in primary and not re.search(r"(?i)\bDEFAULT\b", tail_without_comma):
                    tail_without_comma += (
                        " DEFAULT (UNHEX(REPLACE(UUID_v4(), '-', '')))"
                    )

                return (
                    f"{column_match.group('prefix')}binary(16)"
                    f"{tail_without_comma}{comma}"
                )

            body, count = definition.subn(rewrite_column, body)
            if count != 1:
                raise ValueError(
                    f"Coluna identificadora {table}.{column} apareceu {count} vez(es) no CREATE TABLE."
                )

        return match.group(1) + body + match.group(4)

    return CREATE_TABLE_RE.sub(rewrite_table, sql)


def find_statement_end(sql: str, start: int) -> int:
    in_string = False
    escaped = False
    index = start

    while index < len(sql):
        char = sql[index]
        if in_string:
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == "'":
                if index + 1 < len(sql) and sql[index + 1] == "'":
                    index += 1
                else:
                    in_string = False
        elif char == "'":
            in_string = True
        elif char == ";":
            return index
        index += 1

    raise ValueError("INSERT sem ponto e virgula final.")


def tuple_ranges(values: str) -> list[tuple[int, int]]:
    ranges: list[tuple[int, int]] = []
    in_string = False
    escaped = False
    depth = 0
    tuple_start: int | None = None
    index = 0

    while index < len(values):
        char = values[index]
        if in_string:
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == "'":
                if index + 1 < len(values) and values[index + 1] == "'":
                    index += 1
                else:
                    in_string = False
        elif char == "'":
            in_string = True
        elif char == "(":
            if depth == 0:
                tuple_start = index + 1
            depth += 1
        elif char == ")":
            depth -= 1
            if depth < 0:
                raise ValueError("Parenteses desbalanceados em INSERT.")
            if depth == 0:
                if tuple_start is None:
                    raise ValueError("Tupla de INSERT sem inicio.")
                ranges.append((tuple_start, index))
                tuple_start = None
        index += 1

    if in_string or depth != 0:
        raise ValueError("String ou parenteses desbalanceados em INSERT.")
    return ranges


def value_ranges(tuple_body: str) -> list[tuple[int, int]]:
    ranges: list[tuple[int, int]] = []
    in_string = False
    escaped = False
    depth = 0
    start = 0
    index = 0

    while index < len(tuple_body):
        char = tuple_body[index]
        if in_string:
            if escaped:
                escaped = False
            elif char == "\\":
                escaped = True
            elif char == "'":
                if index + 1 < len(tuple_body) and tuple_body[index + 1] == "'":
                    index += 1
                else:
                    in_string = False
        elif char == "'":
            in_string = True
        elif char == "(":
            depth += 1
        elif char == ")":
            depth -= 1
        elif char == "," and depth == 0:
            ranges.append((start, index))
            start = index + 1
        index += 1

    ranges.append((start, len(tuple_body)))
    return ranges


def deterministic_binary_guid(number: int) -> str:
    if number < 0:
        raise ValueError(f"ID negativo nao suportado: {number}")
    return f"UNHEX('{number:032X}')"


def rewrite_insert(
    statement: str,
    table: str,
    identifier_columns: set[str],
) -> tuple[str, int]:
    header = re.match(
        r"(?is)(INSERT\s+INTO\s+`[^`]+`\s*\((?P<columns>.*?)\)\s*VALUES\s*)(?P<values>.*);$",
        statement,
    )
    if not header:
        raise ValueError(f"Formato de INSERT nao reconhecido para {table}.")

    columns = re.findall(r"`([^`]+)`", header.group("columns"))
    target_indexes = {
        index for index, column in enumerate(columns) if column in identifier_columns
    }
    if not target_indexes:
        return statement, 0

    values = header.group("values")
    replacements: list[tuple[int, int, str]] = []

    for tuple_start, tuple_end in tuple_ranges(values):
        body = values[tuple_start:tuple_end]
        tokens = value_ranges(body)
        if len(tokens) != len(columns):
            raise ValueError(
                f"INSERT em {table}: {len(tokens)} valores para {len(columns)} colunas."
            )

        for target_index in target_indexes:
            token_start, token_end = tokens[target_index]
            raw = body[token_start:token_end]
            leading = len(raw) - len(raw.lstrip())
            trailing = len(raw) - len(raw.rstrip())
            stripped = raw.strip()

            if re.fullmatch(r"(?i)NULL", stripped) or stripped.upper().startswith("UNHEX("):
                continue
            if not re.fullmatch(r"\d+", stripped):
                raise ValueError(
                    f"Valor de ID inesperado em {table}.{columns[target_index]}: {stripped[:80]}"
                )

            replacement_start = tuple_start + token_start + leading
            replacement_end = tuple_start + token_end - trailing
            replacements.append(
                (replacement_start, replacement_end, deterministic_binary_guid(int(stripped)))
            )

    for replacement_start, replacement_end, replacement in sorted(
        replacements, key=lambda item: item[0], reverse=True
    ):
        values = values[:replacement_start] + replacement + values[replacement_end:]

    return statement[: header.start("values")] + values + ";", len(replacements)


def rewrite_inserts(
    sql: str,
    primary_keys: dict[str, set[str]],
    foreign_keys: dict[str, set[str]],
) -> tuple[str, int]:
    replacements: list[tuple[int, int, str]] = []
    converted = 0

    for match in INSERT_START_RE.finditer(sql):
        start = match.start()
        end = find_statement_end(sql, match.end()) + 1
        table = match.group("table")
        identifiers = primary_keys.get(table, set()) | foreign_keys.get(table, set())
        rewritten, count = rewrite_insert(sql[start:end], table, identifiers)
        replacements.append((start, end, rewritten))
        converted += count

    for start, end, replacement in reversed(replacements):
        sql = sql[:start] + replacement + sql[end:]

    return sql, converted


def main() -> int:
    path = Path(sys.argv[1] if len(sys.argv) > 1 else "Banco.txt")
    raw = path.read_bytes()
    has_bom = raw.startswith(b"\xef\xbb\xbf")
    sql = raw.decode("utf-8-sig")

    primary_keys, foreign_keys = collect_identifier_columns(sql)
    if not primary_keys or not foreign_keys:
        raise ValueError("Nao foi possivel identificar as chaves primarias e estrangeiras.")

    rewritten = rewrite_table_definitions(sql, primary_keys, foreign_keys)
    rewritten, converted_values = rewrite_inserts(rewritten, primary_keys, foreign_keys)

    encoded = rewritten.encode("utf-8")
    if has_bom:
        encoded = b"\xef\xbb\xbf" + encoded
    path.write_bytes(encoded)

    identifier_count = sum(
        len(primary_keys.get(table, set()) | foreign_keys.get(table, set()))
        for table in set(primary_keys) | set(foreign_keys)
    )
    print(f"Colunas de ID/FK validadas: {identifier_count}")
    print(f"Valores numericos convertidos em INSERTs: {converted_values}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

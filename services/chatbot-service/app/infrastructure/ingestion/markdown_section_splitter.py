from __future__ import annotations
import re

_HEADING_RE = re.compile(r"^(#{2,3})\s+(.*)$")
_HARD_SEPARATOR_RE = re.compile(r"^-{3,}\s*$")


def split_markdown_sections(text: str) -> list[dict[str, str]]:
    sections: list[dict[str, str]] = []
    current_h2: str | None = None
    current_h3: str | None = None
    buffer: list[str] = []

    def flush() -> None:
        content = "\n".join(buffer).strip("\n")
        if content.strip():
            heading_parts: list[str] = [
                h for h in (current_h2, current_h3)
                if h is not None
            ]
            sections.append({"heading": " > ".join(heading_parts), "content": content})
        buffer.clear()

    for line in text.splitlines():
        heading_match = _HEADING_RE.match(line)
        if heading_match:
            flush()
            level, title = heading_match.groups()
            title = title.strip()
            if level == "##":
                current_h2 = title
                current_h3 = None
            else:
                current_h3 = title
            continue

        if _HARD_SEPARATOR_RE.match(line):
            flush()
            continue

        buffer.append(line)

    flush()
    return sections
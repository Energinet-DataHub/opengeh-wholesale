from datetime import datetime

import configargparse


def valid_date(s: str) -> datetime:
    """Validate a date.

    See https://stackoverflow.com/questions/25470844/specify-date-format-for-python-argparse-input-arguments
    """
    try:
        return datetime.strptime(s, "%Y-%m-%dT%H:%M:%SZ")
    except ValueError:
        msg = f"not a valid date: {s!r}"
        raise configargparse.ArgumentTypeError(msg)


def valid_list(s: str) -> list[str]:
    if not s.startswith("[") or not s.endswith("]"):
        msg = "Grid area codes must be a list enclosed by an opening '[' and a closing ']'"
        raise configargparse.ArgumentTypeError(msg)

    # 1. Remove enclosing list characters 2. Split each grid area code 3. Remove possibly enclosing spaces.
    tokens = [token.strip() for token in s.strip("[]").split(",")]

    # Grid area codes must always consist of 3 digits
    if any(len(token) != 3 or any(c < "0" or c > "9" for c in token) for token in tokens):
        msg = "Grid area codes must consist of 3 digits"
        raise configargparse.ArgumentTypeError(msg)

    return tokens


def valid_log_level(s: str) -> str:
    if s in ["information", "debug"]:
        return str(s)
    else:
        msg = "loglevel is not valid"
        raise configargparse.ArgumentTypeError(msg)

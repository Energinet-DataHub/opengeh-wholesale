from datetime import datetime
import ast
import configargparse


def valid_date(s):
    """See https://stackoverflow.com/questions/25470844/specify-date-format-for-python-argparse-input-arguments"""
    try:
        return datetime.strptime(s, "%Y-%m-%dT%H:%M:%SZ")
    except ValueError:
        msg = "not a valid date: {0!r}".format(s)
        raise configargparse.ArgumentTypeError(msg)


def valid_list(s):
    """See https://stackoverflow.com/questions/25470844/specify-date-format-for-python-argparse-input-arguments"""
    try:
        return ast.literal_eval(s)
    except ValueError:
        msg = "not a valid grid area list"
        raise configargparse.ArgumentTypeError(msg)


def valid_log_level(s):
    if s in ["information", "debug"]:
        return str(s)
    else:
        msg = "loglevel is not valid"
        raise configargparse.ArgumentTypeError(msg)

from pyspark import SparkConf
from pyspark.sql import functions as F, types as T
from pyspark.sql.session import SparkSession
from typing import Any


def get_dbutils(spark: SparkSession):
    """Get the DBUtils object from the SparkSession.

    Args:
        spark (SparkSession): The SparkSession object.

    Returns:
        DBUtils: The DBUtils object.
    """
    try:
        from pyspark.dbutils import DBUtils  # type: ignore

        dbutils = DBUtils(spark)
    except ImportError:
        raise ImportError(
            "DBUtils is not available in local mode. This is expected when running tests."  # noqa
        )
    return dbutils


def get_param_or_fail(
    d: dict,
    key: str,
    message: str,
    _type: T.DataType = T.StringType(),
    default: Any = None,
):
    """Get a parameter from a dictionary or fail if it is missing.

    Args:
        d (dict): The dictionary to get the parameter from.
        key (str): The key of the parameter.
        message (str): The message to display if the parameter is missing.
        _type (T.DataType, optional): The type of the parameter.
            Defaults to T.StringType().
        default (Any, optional): The default value of the parameter.
            Defaults to None.

    Returns:
        Any: The value of the parameter.
    """
    val = d.get(key) or default
    if val is None:
        raise Exception(f"Parameter '{key}' is missing without a default: '{message}'")
    return F.lit(val).cast(_type)


def log_params(d, title: str = "Received parameters:"):
    """Log the parameters in a dictionary.

    Args:
        d (dict): The dictionary to log.
        title (str, optional): The title of the log. Defaults to "Received parameters:".
    """
    max_key_len = max(len(key) for key in d.keys())
    print(title)
    for key, value in d.items():
        print(f"   {key:{max_key_len + 1}s}: {value}")

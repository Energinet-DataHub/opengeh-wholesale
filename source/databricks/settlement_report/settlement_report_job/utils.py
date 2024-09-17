import itertools
import zipfile

from typing import Any
from pyspark.sql import Column, SparkSession
from pyspark.sql import functions as F


def map_from_dict(d: dict) -> Column:
    """Converts a dictionary to a Spark map column

    Args:
        d (dict): Dictionary to convert to a Spark map column

    Returns:
        Column: Spark map column
    """
    return F.create_map([F.lit(x) for x in itertools.chain(*d.items())])


def create_zip_file(
    dbutils: Any, report_id: str, save_path: str, files_to_zip: list[str]
) -> None:
    """Creates a zip file from a list of files and saves it to the specified path.

    Notice that we have to create the zip file in /tmp and then move it to the desired
    location. This is done as `direct-append` or `non-sequential` writes are not
    supported in Databricks.

    Args:
        dbutils (Any): The DBUtils object.
        report_id (str): The report ID.
        save_path (str): The path to save the zip file.
        files_to_zip (list[str]): The list of files to zip.

    Raises:
        Exception: If there are no files to zip.
        Exception: If the save path does not end with .zip.
    """
    if len(files_to_zip) == 0:
        raise Exception("No files to zip")
    if not save_path.endswith(".zip"):
        raise Exception("Save path must end with .zip")

    tmp_path = f"/tmp/{report_id}.zip"
    with zipfile.ZipFile(tmp_path, "a", zipfile.ZIP_DEFLATED) as ref:
        for fp in files_to_zip:
            file_name = fp.split("/")[-1]
            ref.write(fp, arcname=file_name)
    dbutils.fs.mv(f"file:{tmp_path}", save_path)


def get_dbutils(spark: SparkSession) -> Any:
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

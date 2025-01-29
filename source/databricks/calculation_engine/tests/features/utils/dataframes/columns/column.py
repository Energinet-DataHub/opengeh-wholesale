from dataclasses import dataclass

from pyspark.sql.types import DataType


@dataclass
class Column:
    name: str
    data_type: DataType

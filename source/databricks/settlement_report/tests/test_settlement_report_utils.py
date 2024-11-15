from pathlib import Path
import pytest
from datetime import datetime
from tempfile import TemporaryDirectory
from pyspark.sql import SparkSession, functions as F
from pyspark.sql.types import StructType, StructField, StringType

import settlement_report_job.domain.utils.map_to_csv_naming as market_naming
from settlement_report_job.infrastructure.wholesale.data_values import (
    ChargeTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.metering_point_type import (
    MeteringPointTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)


def test_get_dbutils__when_run_locally__raise_exception(spark: SparkSession):
    # Act
    with pytest.raises(Exception):
        get_dbutils(spark)

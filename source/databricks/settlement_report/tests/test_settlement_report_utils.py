from pathlib import Path
import pytest
from datetime import datetime
from tempfile import TemporaryDirectory
from pyspark.sql import SparkSession, functions as F
from pyspark.sql.types import StructType, StructField, StringType

import settlement_report_job.domain.utils.report_naming_convention as market_naming
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


def test_map_from_dict__when_applied_to_new_col__returns_df_with_new_col(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1), ("b", 2), ("c", 3)], ["key", "value"])

    # Act
    mapper = map_from_dict({"a": "another_a"})
    actual = df.select("*", mapper[F.col("key")].alias("new_key"))

    # Assert
    expected = spark.createDataFrame(
        [("a", 1, "another_a"), ("b", 2, None), ("c", 3, None)],
        ["key", "value", "new_key"],
    )
    assert actual.collect() == expected.collect()


def test_map_from_dict__when_applied_as_overwrite__returns_df_with_overwritten_column(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1), ("b", 2), ("c", 3)], ["key", "value"])

    # Act
    mapper = map_from_dict({"a": "another_a"})
    actual = df.select(mapper[F.col("key")].alias("key"), "value")

    # Assert
    expected = spark.createDataFrame(
        [
            ("another_a", 1),
            (None, 2),
            (None, 3),
        ],
        ["key", "value"],
    )
    assert actual.collect() == expected.collect()


def test_get_dbutils__when_run_locally__raise_exception(spark: SparkSession):
    # Act
    with pytest.raises(Exception):
        get_dbutils(spark)


def test_create_zip_file__when_dbutils_is_none__raise_exception():
    # Arrange
    dbutils = None
    report_id = "report_id"
    save_path = "save_path.zip"
    files_to_zip = ["file1", "file2"]

    # Act
    with pytest.raises(Exception):
        create_zip_file(dbutils, report_id, save_path, files_to_zip)


def test_create_zip_file__when_save_path_is_not_zip__raise_exception():
    # Arrange
    dbutils = None
    report_id = "report_id"
    save_path = "save_path"
    files_to_zip = ["file1", "file2"]

    # Act
    with pytest.raises(Exception):
        create_zip_file(dbutils, report_id, save_path, files_to_zip)


def test_create_zip_file__when_no_files_to_zip__raise_exception():
    # Arrange
    dbutils = None
    report_id = "report_id"
    save_path = "save_path.zip"
    files_to_zip = ["file1", "file2"]

    # Act
    with pytest.raises(Exception):
        create_zip_file(dbutils, report_id, save_path, files_to_zip)


def test_create_zip_file__when_files_to_zip__create_zip_file(dbutils):
    # Arrange
    tmp_dir = TemporaryDirectory()
    with open(f"{tmp_dir.name}/file1", "w") as f:
        f.write("content1")
    with open(f"{tmp_dir.name}/file2", "w") as f:
        f.write("content2")

    report_id = "report_id"
    save_path = f"{tmp_dir.name}/save_path.zip"
    files_to_zip = [f"{tmp_dir.name}/file1", f"{tmp_dir.name}/file2"]

    # Act
    create_zip_file(dbutils, report_id, save_path, files_to_zip)

    # Assert
    assert Path(save_path).exists()
    tmp_dir.cleanup()


def test_write_files__csv_separator_is_comma_and_decimals_use_points(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1.1), ("b", 2.2), ("c", 3.3)], ["key", "value"])
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = write_files(
        df,
        csv_path,
        partition_columns=[],
        order_by=[],
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                assert all_lines_written[0] == "a,1.1\n"
                assert all_lines_written[1] == "b,2.2\n"
                assert all_lines_written[2] == "c,3.3\n"

    assert columns == ["key", "value"]

    tmp_dir.cleanup()


def test_write_files__when_order_by_specified_on_multiple_partitions(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame(
        [("b", 2.2), ("b", 1.1), ("c", 3.3)],
        ["key", "value"],
    )
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = write_files(
        df,
        csv_path,
        partition_columns=["key"],
        order_by=["value"],
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                if len(all_lines_written == 1):
                    assert all_lines_written[0] == "c;3,3\n"
                elif len(all_lines_written == 2):
                    assert all_lines_written[0] == "b;1,1\n"
                    assert all_lines_written[1] == "b;2,2\n"
                else:
                    raise AssertionError("Found unexpected csv file.")

    assert columns == ["value"]

    tmp_dir.cleanup()


def test_write_files__when_df_includes_timestamps__creates_csv_without_milliseconds(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame(
        [
            ("a", datetime(2024, 10, 21, 12, 10, 30, 0)),
            ("b", datetime(2024, 10, 21, 12, 10, 30, 30)),
            ("c", datetime(2024, 10, 21, 12, 10, 30, 123)),
        ],
        ["key", "value"],
    )
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = write_files(
        df,
        csv_path,
        partition_columns=[],
        order_by=[],
        rows_per_file=1000,
    )

    # Assert
    assert Path(csv_path).exists()

    for x in Path(csv_path).iterdir():
        if x.is_file() and x.name[-4:] == ".csv":
            with x.open(mode="r") as f:
                all_lines_written = f.readlines()

                assert all_lines_written[0] == "a,2024-10-21T12:10:30Z\n"
                assert all_lines_written[1] == "b,2024-10-21T12:10:30Z\n"
                assert all_lines_written[2] == "c,2024-10-21T12:10:30Z\n"

    assert columns == ["key", "value"]

    tmp_dir.cleanup()


@pytest.mark.parametrize(
    "charge_type, expected_charge_type",
    [
        pytest.param(
            ChargeTypeDataProductValue.SUBSCRIPTION,
            "D01",
            id="when charge type is subscription, then charge type is D01",
        ),
        pytest.param(
            ChargeTypeDataProductValue.FEE,
            "D02",
            id="when charge type is fee, then charge type is D02",
        ),
        pytest.param(
            ChargeTypeDataProductValue.TARIFF,
            "D03",
            id="when charge type is tariff, then charge type is D03",
        ),
    ],
)
def test_mapping_of_charge_type(
    spark: SparkSession,
    charge_type: ChargeTypeDataProductValue,
    expected_charge_type: str,
) -> None:
    # Arrange
    df = spark.createDataFrame(
        data=[[charge_type.value]],
        schema=StructType([StructField("charge_type", StringType(), True)]),
    )

    # Act
    actual = df.select(map_from_dict(market_naming.CHARGE_TYPES)[F.col("charge_type")])

    # Assert
    assert actual.collect()[0][0] == expected_charge_type


@pytest.mark.parametrize(
    "calculation_type, expected_process_variant",
    [
        pytest.param(
            CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT,
            "1ST",
            id="when calculation type is first_correction_settlement, then process variant is 1ST",
        ),
        pytest.param(
            CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT,
            "2ND",
            id="when calculation type is second_correction_settlement, then process variant is 2ND",
        ),
        pytest.param(
            CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT,
            "3RD",
            id="when calculation type is third_correction_settlement, then process variant is 3RD",
        ),
        pytest.param(
            CalculationTypeDataProductValue.WHOLESALE_FIXING,
            None,
            id="when calculation type is wholesale_fixing, then process variant is None",
        ),
        pytest.param(
            CalculationTypeDataProductValue.BALANCE_FIXING,
            None,
            id="when calculation type is balance_fixing, then process variant is None",
        ),
    ],
)
def test_mapping_of_process_variant(
    spark: SparkSession,
    calculation_type: CalculationTypeDataProductValue,
    expected_process_variant: str,
) -> None:
    # Arrange
    df = spark.createDataFrame([[calculation_type.value]], ["calculation_type"])

    # Act
    actual = df.select(
        map_from_dict(market_naming.CALCULATION_TYPES_TO_PROCESS_VARIANT)[
            F.col("calculation_type")
        ]
    )

    # Assert
    assert actual.collect()[0][0] == expected_process_variant


@pytest.mark.parametrize(
    "calculation_type, expected_energy_business_process",
    [
        pytest.param(
            CalculationTypeDataProductValue.BALANCE_FIXING,
            "D04",
            id="when calculation type is balance_fixing, then energy business process is D04",
        ),
        pytest.param(
            CalculationTypeDataProductValue.WHOLESALE_FIXING,
            "D05",
            id="when calculation type is wholesale_fixing, then energy business process is D05",
        ),
        pytest.param(
            CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT,
            "D32",
            id="when calculation type is first_correction_settlement, then energy business process is D32",
        ),
        pytest.param(
            CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT,
            "D32",
            id="when calculation type is second_correction_settlement, then energy business process is D32",
        ),
        pytest.param(
            CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT,
            "D32",
            id="when calculation type is third_correction_settlement, then energy business process is D32",
        ),
    ],
)
def test_mapping_of_energy_business_process(
    spark: SparkSession,
    calculation_type: CalculationTypeDataProductValue,
    expected_energy_business_process: str,
) -> None:
    # Arrange
    df = spark.createDataFrame([[calculation_type.value]], ["calculation_type"])

    # Act
    actual = df.select(
        map_from_dict(market_naming.CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS)[
            F.col("calculation_type")
        ]
    )

    # Assert
    assert actual.collect()[0][0] == expected_energy_business_process


@pytest.mark.parametrize(
    "metering_point_type, expected_metering_point_type",
    [
        pytest.param(
            MeteringPointTypeDataProductValue.CONSUMPTION,
            "E17",
            id="when metering point type is consumption, then type of mp is E17",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.PRODUCTION,
            "E18",
            id="when metering point type is production, then type of mp is E18",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.EXCHANGE,
            "E20",
            id="when metering point type is exchange, then type of mp is E20",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.VE_PRODUCTION,
            "D01",
            id="when metering point type is ve_production, then type of mp is D01",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.NET_PRODUCTION,
            "D05",
            id="when metering point type is net_production, then type of mp is D05",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.SUPPLY_TO_GRID,
            "D06",
            id="when metering point type is supply_to_grid, then type of mp is D06",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.CONSUMPTION_FROM_GRID,
            "D07",
            id="when metering point type is consumption_from_grid, then type of mp is D07",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.WHOLESALE_SERVICES_INFORMATION,
            "D08",
            id="when metering point type is wholesale_services_information, then type of mp is D08",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.OWN_PRODUCTION,
            "D09",
            id="when metering point type is own_production, then type of mp is D09",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.NET_FROM_GRID,
            "D10",
            id="when metering point type is net_from_grid, then type of mp is D10",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.NET_TO_GRID,
            "D11",
            id="when metering point type is net_to_grid, then type of mp is D11",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.TOTAL_CONSUMPTION,
            "D12",
            id="when metering point type is total_consumption, then type of mp is D12",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.ELECTRICAL_HEATING,
            "D14",
            id="when metering point type is electrical_heating, then type of mp is D14",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.NET_CONSUMPTION,
            "D15",
            id="when metering point type is net_consumption, then type of mp is D15",
        ),
        pytest.param(
            MeteringPointTypeDataProductValue.EFFECT_SETTLEMENT,
            "D19",
            id="when metering point type is effect_settlement, then type of mp is D19",
        ),
    ],
)
def test_mapping_of_metering_point_type(
    spark: SparkSession,
    metering_point_type: MeteringPointTypeDataProductValue,
    expected_metering_point_type: str,
) -> None:
    # Arrange
    df = spark.createDataFrame([[metering_point_type.value]], ["metering_point_type"])

    # Act
    actual = df.select(
        map_from_dict(market_naming.METERING_POINT_TYPES)[F.col("metering_point_type")]
    )

    # Assert
    assert actual.collect()[0][0] == expected_metering_point_type


@pytest.mark.parametrize(
    "settlement_method, expected_settlement_method",
    [
        pytest.param(
            SettlementMethodDataProductValue.NON_PROFILED,
            "E02",
            id="when settlement method is non_profiled, then settlement method is E02",
        ),
        pytest.param(
            SettlementMethodDataProductValue.FLEX,
            "D01",
            id="when settlement method is flex, then settlement method is D01",
        ),
    ],
)
def test_mapping_of_settlement_method(
    spark: SparkSession,
    settlement_method: SettlementMethodDataProductValue,
    expected_settlement_method: str,
) -> None:
    # Arrange
    df = spark.createDataFrame([[settlement_method.value]], ["settlement_method"])

    # Act
    actual = df.select(
        map_from_dict(market_naming.SETTLEMENT_METHODS)[F.col("settlement_method")]
    )

    # Assert
    assert actual.collect()[0][0] == expected_settlement_method

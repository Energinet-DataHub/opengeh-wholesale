# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from datetime import datetime
from pathlib import Path
from tempfile import TemporaryDirectory

from assertion import assert_file_names_and_columns
from settlement_report_job.infrastructure import csv_writer

from pyspark.sql import SparkSession, DataFrame
import pyspark.sql.functions as F
from settlement_report_job.domain.utils.market_role import (
    MarketRole,
)
from settlement_report_job.domain.energy_results.prepare_for_csv import (
    prepare_for_csv,
)
from data_seeding import (
    standard_wholesale_fixing_scenario_data_generator,
)
from settlement_report_job.infrastructure.csv_writer import _write_files
from test_factories.default_test_data_spec import (
    create_energy_results_data_spec,
)
from dbutils_fixture import DBUtilsFixture
from functools import reduce
import pytest

from settlement_report_job.domain.utils.report_data_type import ReportDataType

from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
import test_factories.time_series_points_csv_factory as time_series_points_factory
import test_factories.energy_factory as energy_factory
from settlement_report_job.domain.utils.csv_column_names import CsvColumnNames
from settlement_report_job.infrastructure.paths import get_report_output_path
from settlement_report_job.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
)
import settlement_report_job.domain.time_series_points.order_by_columns as time_series_points_order_by_columns
import settlement_report_job.domain.energy_results.order_by_columns as energy_order_by_columns


def _read_csv_file(
    directory: str,
    file_name: str,
    spark: SparkSession,
) -> DataFrame:
    file_name = f"{directory}/{file_name}"
    return spark.read.csv(file_name, header=True)


@pytest.mark.parametrize(
    "resolution,grid_area_codes,expected_file_count",
    [
        (MeteringPointResolutionDataProductValue.HOUR, ["804", "805"], 2),
        (MeteringPointResolutionDataProductValue.QUARTER, ["804", "805"], 2),
        (MeteringPointResolutionDataProductValue.HOUR, ["804"], 1),
        (MeteringPointResolutionDataProductValue.QUARTER, ["804", "805", "806"], 3),
    ],
)
def test_write__returns_files_corresponding_to_grid_area_codes(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    resolution: MeteringPointResolutionDataProductValue,
    grid_area_codes: list[str],
    expected_file_count: int,
):
    # Arrange
    report_data_type = (
        ReportDataType.TimeSeriesHourly
        if resolution == MeteringPointResolutionDataProductValue.HOUR
        else ReportDataType.TimeSeriesQuarterly
    )
    test_spec = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=grid_area_codes,
        resolution=resolution,
    )
    df_prepared_time_series_points = time_series_points_factory.create(spark, test_spec)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=time_series_points_order_by_columns.order_by_columns(
            requesting_actor_market_role=standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
    )

    # Assert
    assert len(result_files) == expected_file_count


def test_write__when_higher_default_parallelism__number_of_files_is_unchanged(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    spark.conf.set("spark.sql.shuffle.partitions", "10")
    spark.conf.set("spark.default.parallelism", "10")
    report_data_type = ReportDataType.TimeSeriesHourly
    expected_file_count = 2
    test_spec = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=["804", "805"],
    )
    df_prepared_time_series_points = time_series_points_factory.create(spark, test_spec)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=time_series_points_order_by_columns.order_by_columns(
            requesting_actor_market_role=standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
    )

    # Assert
    assert len(result_files) == expected_file_count


@pytest.mark.parametrize(
    "number_of_rows,rows_per_file,expected_file_count",
    [
        (201, 100, 3),
        (101, 100, 2),
        (100, 100, 1),
        (99, 100, 1),
    ],
)
def test_write__when_prevent_large_files_is_enabled__writes_expected_number_of_files(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    number_of_rows: int,
    rows_per_file: int,
    expected_file_count: int,
):
    # Arrange
    report_data_type = ReportDataType.TimeSeriesHourly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series_points = time_series_points_factory.create(spark, test_spec)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=time_series_points_order_by_columns.order_by_columns(
            requesting_actor_market_role=standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
        rows_per_file=rows_per_file,
    )

    # Assert
    assert df_prepared_time_series_points.count() == number_of_rows
    assert len(result_files) == expected_file_count


@pytest.mark.parametrize(
    "number_of_metering_points,number_of_days_for_each_mp,rows_per_file,expected_file_count",
    [
        (21, 10, 100, 3),
        (11, 10, 100, 2),
        (9, 10, 100, 1),
    ],
)
def test_write__files_have_correct_ordering_for_each_file(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    number_of_metering_points: int,
    number_of_days_for_each_mp: int,
    rows_per_file: int,
    expected_file_count: int,
):
    # Arrange
    expected_order_by = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.time,
    ]
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_metering_points,
        num_days_per_metering_point=number_of_days_for_each_mp,
    )
    df_prepared_time_series_points = time_series_points_factory.create(spark, test_spec)
    df_prepared_time_series_points = df_prepared_time_series_points.orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=ReportDataType.TimeSeriesHourly,
        order_by_columns=expected_order_by,
        rows_per_file=rows_per_file,
    )

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    for file_name in result_files:
        directory = get_report_output_path(standard_wholesale_fixing_scenario_args)
        df_actual = _read_csv_file(directory, file_name, spark)
        df_expected = df_actual.orderBy(expected_order_by)
        assert df_actual.collect() == df_expected.collect()


@pytest.mark.parametrize(
    "number_of_rows,grid_area_codes,expected_file_count",
    [
        (20, ["804"], 1),
        (20, ["804", "805"], 2),
    ],
)
def test_write__files_have_correct_ordering_for_each_grid_area_code_file(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    number_of_rows: int,
    grid_area_codes: list[str],
    expected_file_count: int,
):
    # Arrange
    expected_order_by = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.time,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    test_spec = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=grid_area_codes,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series_points = time_series_points_factory.create(spark, test_spec)
    df_prepared_time_series_points = df_prepared_time_series_points.orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=expected_order_by,
    )

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    for file_name in result_files:
        directory = get_report_output_path(standard_wholesale_fixing_scenario_args)
        df_actual = _read_csv_file(directory, file_name, spark)
        df_expected = df_actual.orderBy(expected_order_by)
        assert df_actual.collect() == df_expected.collect()


def test_write__files_have_correct_ordering_for_multiple_metering_point_types(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 3
    individual_dataframes = []
    expected_order_by = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.time,
    ]
    report_data_type = ReportDataType.TimeSeriesQuarterly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec_consumption = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=10,
    )
    test_spec_production = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.PRODUCTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=20,
    )
    df_prepared_time_series_points_consumption = time_series_points_factory.create(
        spark, test_spec_consumption
    )
    df_prepared_time_series_points_production = time_series_points_factory.create(
        spark, test_spec_production
    )
    df_prepared_time_series_points = df_prepared_time_series_points_consumption.union(
        df_prepared_time_series_points_production
    ).orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=expected_order_by,
        rows_per_file=10,
    )
    result_files.sort()

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    directory = get_report_output_path(standard_wholesale_fixing_scenario_args)
    for file in result_files:
        individual_dataframes.append(_read_csv_file(directory, file, spark))
    df_actual = reduce(DataFrame.unionByName, individual_dataframes)
    df_expected = df_actual.orderBy(expected_order_by)
    assert df_actual.collect() == df_expected.collect()


@pytest.mark.parametrize(
    "number_of_rows,rows_per_file,expected_file_count",
    [
        (201, 100, 3),
        (101, 100, 2),
        (99, 100, 1),
    ],
)
def test_write__files_have_correct_sorting_across_multiple_files(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    number_of_rows: int,
    rows_per_file: int,
    expected_file_count: int,
):
    # Arrange
    individual_dataframes = []
    expected_order_by = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.time,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series_points = time_series_points_factory.create(spark, test_spec)
    df_prepared_time_series_points = df_prepared_time_series_points.orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=expected_order_by,
        rows_per_file=rows_per_file,
    )
    result_files.sort()

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    directory = get_report_output_path(standard_wholesale_fixing_scenario_args)
    for file in result_files:
        individual_dataframes.append(_read_csv_file(directory, file, spark))
    df_actual = reduce(DataFrame.unionByName, individual_dataframes)
    df_expected = df_actual.orderBy(expected_order_by)
    assert df_actual.collect() == df_expected.collect()


def test_write__when_prevent_large_files__chunk_index_start_at_1(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 3
    report_data_type = ReportDataType.TimeSeriesQuarterly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec_consumption = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=30,
    )
    df_prepared_time_series_points = time_series_points_factory.create(
        spark, test_spec_consumption
    )

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=time_series_points_order_by_columns.order_by_columns(
            requesting_actor_market_role=standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
        rows_per_file=10,
    )

    # Assert
    assert len(result_files) == expected_file_count
    for result_file in result_files:
        file_name = result_file[:-4]
        file_name_components = file_name.split("_")

        chunk_id_if_present = file_name_components[-1]
        assert int(chunk_id_if_present) >= 1 and int(chunk_id_if_present) < 4


def test_write__when_prevent_large_files_but_too_few_rows__chunk_index_should_be_excluded(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 1
    report_data_type = ReportDataType.TimeSeriesQuarterly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec_consumption = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=30,
    )
    df_prepared_time_series_points = time_series_points_factory.create(
        spark, test_spec_consumption
    )

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series_points,
        report_data_type=report_data_type,
        order_by_columns=time_series_points_order_by_columns.order_by_columns(
            requesting_actor_market_role=standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
        rows_per_file=31,
    )

    # Assert
    assert len(result_files) == expected_file_count
    file_name_components = result_files[0][:-4].split("_")

    assert not file_name_components[
        -1
    ].isdigit(), (
        "A valid integer indicating a present chunk index was found when not expected!"
    )


def test_write__when_prevent_large_files_and_multiple_grid_areas_but_too_few_rows__chunk_index_should_be_excluded(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 2
    report_data_type = ReportDataType.TimeSeriesQuarterly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec_consumption = time_series_points_factory.TimeSeriesPointsCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=10,
        grid_area_codes=["804", "805"],
    )
    prepared_time_series_point = time_series_points_factory.create(
        spark, test_spec_consumption, add_grid_area_code_partitioning_column=True
    )

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=prepared_time_series_point,
        report_data_type=report_data_type,
        order_by_columns=time_series_points_order_by_columns.order_by_columns(
            requesting_actor_market_role=standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
        rows_per_file=31,
    )

    # Assert
    assert len(result_files) == expected_file_count
    for result_file in result_files:
        file_name_components = result_file[:-4].split("_")
        chunk_id_if_present = file_name_components[-1]

        assert (
            not chunk_id_if_present.isdigit()
        ), "A valid integer indicating a present chunk index was found when not expected!"


def test_write__when_energy_and_split_report_by_grid_area_is_false__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.energy_quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_02-01-2024_02-01-2024.csv",
    ]

    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area = {
        standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[
            0
        ]: standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area[
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]
        ]
    }
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    standard_wholesale_fixing_scenario_args.split_report_by_grid_area = True

    df = prepare_for_csv(
        energy_factory.create_energy_per_es_v1(
            spark, create_energy_results_data_spec(grid_area_code="804")
        ),
        standard_wholesale_fixing_scenario_args.split_report_by_grid_area,
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
    )

    # Act
    actual_file_names = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df,
        report_data_type=ReportDataType.EnergyResults,
        order_by_columns=energy_order_by_columns.order_by_columns(
            requesting_actor_market_role=standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
        rows_per_file=10000,
    )

    # Assert
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_file_names,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_write__when_energy_supplier_and_split_per_grid_area_is_false__returns_correct_columns_and_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.energy_quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_flere-net_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
    ]

    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )
    energy_supplier_id = "1000000000000"
    standard_wholesale_fixing_scenario_args.requesting_actor_id = energy_supplier_id
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = [energy_supplier_id]
    standard_wholesale_fixing_scenario_args.split_report_by_grid_area = False

    df = prepare_for_csv(
        energy_factory.create_energy_per_es_v1(
            spark,
            create_energy_results_data_spec(
                grid_area_code="804", energy_supplier_id=energy_supplier_id
            ),
        ).union(
            energy_factory.create_energy_per_es_v1(
                spark,
                create_energy_results_data_spec(
                    grid_area_code="805", energy_supplier_id=energy_supplier_id
                ),
            )
        ),
        False,
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
    )

    # Act
    actual_file_names = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df,
        report_data_type=ReportDataType.EnergyResults,
        order_by_columns=energy_order_by_columns.order_by_columns(
            standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
        rows_per_file=10000,
    )

    # Assert
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_file_names,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_write__when_energy_and_prevent_large_files__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 4  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.energy_quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_02-01-2024_02-01-2024_1.csv",
        "RESULTENERGY_804_02-01-2024_02-01-2024_2.csv",
        "RESULTENERGY_804_02-01-2024_02-01-2024_3.csv",
        "RESULTENERGY_804_02-01-2024_02-01-2024_4.csv",
    ]

    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area = {
        standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[
            0
        ]: standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area[
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]
        ]
    }
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    standard_wholesale_fixing_scenario_args.split_report_by_grid_area = False
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True

    df = energy_factory.create_energy_per_es_v1(
        spark, create_energy_results_data_spec(grid_area_code="804")
    )

    df = prepare_for_csv(
        df,
        True,
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
    )

    # Act
    actual_file_names = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df,
        report_data_type=ReportDataType.EnergyResults,
        order_by_columns=energy_order_by_columns.order_by_columns(
            standard_wholesale_fixing_scenario_args.requesting_actor_market_role,
        ),
        rows_per_file=df.count() // expected_file_count + 1,
    )

    # Assert
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_file_names,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_write_files__csv_separator_is_comma_and_decimals_use_points(
    spark: SparkSession,
):
    # Arrange
    df = spark.createDataFrame([("a", 1.1), ("b", 2.2), ("c", 3.3)], ["key", "value"])
    tmp_dir = TemporaryDirectory()
    csv_path = f"{tmp_dir.name}/csv_file"

    # Act
    columns = _write_files(
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
    columns = _write_files(
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
    columns = _write_files(
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

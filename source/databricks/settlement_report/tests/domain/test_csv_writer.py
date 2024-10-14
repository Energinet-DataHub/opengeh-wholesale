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
from settlement_report_job.domain import csv_writer

from pyspark.sql import SparkSession, DataFrame
import pyspark.sql.functions as F
from tests.fixtures import DBUtilsFixture
from functools import reduce
import pytest
from settlement_report_job.domain.report_data_type import ReportDataType

from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
import tests.test_factories.time_series_csv_factory as factory
from settlement_report_job.domain.csv_column_names import (
    TimeSeriesPointCsvColumnNames,
)
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
)


def _read_csv_file(file_name: str, spark: SparkSession) -> DataFrame:
    return spark.read.csv(file_name, header=True, sep=",")


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
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=grid_area_codes,
        resolution=resolution,
    )
    df_prepared_time_series = factory.create(spark, test_spec)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
    )

    # Assert
    assert len(result_files) > 0
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
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=["804", "805"],
    )
    df_prepared_time_series = factory.create(spark, test_spec)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
    )

    # Assert
    assert len(result_files) > 0
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
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series = factory.create(spark, test_spec)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
        rows_per_file=rows_per_file,
    )

    # Assert
    assert df_prepared_time_series.count() == number_of_rows
    assert len(result_files) > 0
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
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_metering_points,
        num_days_per_metering_point=number_of_days_for_each_mp,
    )
    df_prepared_time_series = factory.create(spark, test_spec)
    df_prepared_time_series = df_prepared_time_series.orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
        rows_per_file=rows_per_file,
    )

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    for file in result_files:
        df_actual = _read_csv_file(file, spark)
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
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=grid_area_codes,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series = factory.create(spark, test_spec)
    df_prepared_time_series = df_prepared_time_series.orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
    )

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    for file in result_files:
        df_actual = _read_csv_file(file, spark)
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
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ]
    report_data_type = ReportDataType.TimeSeriesQuarterly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    standard_wholesale_fixing_scenario_args.locale = "en-gb"
    test_spec_consumption = factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=10,
    )
    test_spec_production = factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.PRODUCTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=20,
    )
    df_prepared_time_series_consumption = factory.create(spark, test_spec_consumption)
    df_prepared_time_series_production = factory.create(spark, test_spec_production)
    df_prepared_time_series = df_prepared_time_series_consumption.union(
        df_prepared_time_series_production
    ).orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
        rows_per_file=10,
    )
    result_files.sort()

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    for file in result_files:
        individual_dataframes.append(_read_csv_file(file, spark))
    df_actual = reduce(DataFrame.unionByName, individual_dataframes)
    df_expected = df_actual.orderBy(expected_order_by)
    print(df_actual.collect()[0], df_expected.collect()[0])
    print(df_actual.collect()[-1], df_expected.collect()[-1])
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
        TimeSeriesPointCsvColumnNames.metering_point_type,
        TimeSeriesPointCsvColumnNames.metering_point_id,
        TimeSeriesPointCsvColumnNames.start_of_day,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series = factory.create(spark, test_spec)
    df_prepared_time_series = df_prepared_time_series.orderBy(F.rand())

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
        rows_per_file=rows_per_file,
    )
    result_files.sort()

    # Assert
    assert len(result_files) == expected_file_count

    # Assert that the files are ordered by metering_point_type, metering_point_id, start_of_day
    # Asserting that the dataframe is unchanged
    for file in result_files:
        individual_dataframes.append(_read_csv_file(file, spark))
    df_actual = reduce(DataFrame.unionByName, individual_dataframes)
    df_expected = df_actual.orderBy(expected_order_by)
    assert df_actual.collect() == df_expected.collect()

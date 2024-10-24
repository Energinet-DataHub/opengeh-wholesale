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
from settlement_report_job.domain.market_role import (
    MarketRole,
)
from settlement_report_job.domain.energy_results.prepare_for_csv import (
    prepare_for_csv,
)
from tests.data_seeding import (
    standard_wholesale_fixing_scenario_data_generator,
)
from tests.test_factories.default_test_data_spec import (
    create_energy_results_data_spec,
)
from tests.dbutils_fixture import DBUtilsFixture
from functools import reduce
import pytest
from settlement_report_job.domain.report_data_type import ReportDataType

from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
import test_factories.time_series_csv_factory as time_series_factory
import test_factories.energy_factory as energy_factory
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
)
from settlement_report_job.utils import _get_csv_writer_options_based_on_locale


def _read_csv_file(
    file_name: str, spark: SparkSession, expected_delimiter: str
) -> DataFrame:
    return spark.read.option("delimiter", expected_delimiter).csv(
        file_name, header=True, sep=expected_delimiter
    )


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
    test_spec = time_series_factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=grid_area_codes,
        resolution=resolution,
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec)

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
    test_spec = time_series_factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=["804", "805"],
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec)

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
    test_spec = time_series_factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec)

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
        CsvColumnNames.type_of_mp,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.start_date_time,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = time_series_factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_metering_points,
        num_days_per_metering_point=number_of_days_for_each_mp,
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec)
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

    expected_delimiter = _get_csv_writer_options_based_on_locale(
        standard_wholesale_fixing_scenario_args.locale
    )["delimiter"]
    for file in result_files:
        df_actual = _read_csv_file(file, spark, expected_delimiter)
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
        CsvColumnNames.type_of_mp,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.start_date_time,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    test_spec = time_series_factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=grid_area_codes,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec)
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

    expected_delimiter = _get_csv_writer_options_based_on_locale(
        standard_wholesale_fixing_scenario_args.locale
    )["delimiter"]
    for file in result_files:
        df_actual = _read_csv_file(file, spark, expected_delimiter)
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
        CsvColumnNames.type_of_mp,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.start_date_time,
    ]
    report_data_type = ReportDataType.TimeSeriesQuarterly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    standard_wholesale_fixing_scenario_args.locale = "en-gb"
    test_spec_consumption = time_series_factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=10,
    )
    test_spec_production = time_series_factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.PRODUCTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=20,
    )
    df_prepared_time_series_consumption = time_series_factory.create(
        spark, test_spec_consumption
    )
    df_prepared_time_series_production = time_series_factory.create(
        spark, test_spec_production
    )
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

    expected_delimiter = _get_csv_writer_options_based_on_locale(
        standard_wholesale_fixing_scenario_args.locale
    )["delimiter"]
    for file in result_files:
        individual_dataframes.append(_read_csv_file(file, spark, expected_delimiter))
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
        CsvColumnNames.type_of_mp,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.start_date_time,
    ]
    report_data_type = ReportDataType.TimeSeriesHourly
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = time_series_factory.TimeSeriesCsvTestDataSpec(
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=number_of_rows,
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec)
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
    expected_delimiter = _get_csv_writer_options_based_on_locale(
        standard_wholesale_fixing_scenario_args.locale
    )["delimiter"]
    for file in result_files:
        individual_dataframes.append(_read_csv_file(file, spark, expected_delimiter))
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
    standard_wholesale_fixing_scenario_args.locale = "en-gb"
    test_spec_consumption = time_series_factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=30,
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec_consumption)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
        rows_per_file=10,
    )

    # Assert
    assert len(result_files) == expected_file_count
    for result_file in result_files:
        file_name = result_file[:-4].split("/")[-1]
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
    standard_wholesale_fixing_scenario_args.locale = "en-gb"
    test_spec_consumption = time_series_factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        num_metering_points=30,
    )
    df_prepared_time_series = time_series_factory.create(spark, test_spec_consumption)

    # Act
    result_files = csv_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        df=df_prepared_time_series,
        report_data_type=report_data_type,
        rows_per_file=31,
    )

    # Assert
    assert len(result_files) == expected_file_count
    file_name = result_files[0]
    file_name = file_name[:-4].split("/")[-1]
    file_name_components = file_name.split("_")

    chunk_id_if_present = file_name_components[-1]
    try:
        int(chunk_id_if_present)
        assert (
            False
        ), "A valid integer indicating a present chunk index was found when not expected!"
    except ValueError:
        pass


def test_write__when_energy_and_split_report_by_grid_area_is_false__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 1  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.energy_business_process,
        CsvColumnNames.start_date_time,
        CsvColumnNames.resolution_duration,
        CsvColumnNames.type_of_mp,
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
    )

    # Act
    actual_files = csv_writer.write(
        dbutils,
        standard_wholesale_fixing_scenario_args,
        df,
        ReportDataType.EnergyResults,
        10000,
    )

    # Assert
    actual_file_names = [file.split("/")[-1] for file in actual_files]
    for actual_file_name in actual_file_names:
        assert actual_file_name in expected_file_names

    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns


def test_write__when_energy_supplier_and_split_per_grid_area_is_false__returns_correct_columns_and_files(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    expected_file_count = 1
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_business_process,
        CsvColumnNames.start_date_time,
        CsvColumnNames.resolution_duration,
        CsvColumnNames.type_of_mp,
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
        standard_wholesale_fixing_scenario_args.split_report_by_grid_area,
    )

    # Act
    actual_files = csv_writer.write(
        dbutils,
        standard_wholesale_fixing_scenario_args,
        df,
        ReportDataType.EnergyResults,
        10000,
    )

    # Assert
    actual_file_names = [file.split("/")[-1] for file in actual_files]
    for actual_file_name in actual_file_names:
        assert actual_file_name in expected_file_names

    assert len(actual_files) == expected_file_count
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns

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
from decimal import Decimal
from datetime import datetime
from geh_stream.codelists import Colname, ResultKeyName, ResolutionDuration, MarketEvaluationPointType
from geh_stream.aggregation_utils.aggregators import adjust_production
from geh_stream.codelists import Quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema
from pyspark.sql.functions import col
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, BooleanType
import pytest
import pandas as pd

# Default values
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_sum_quantity = Decimal(1)
default_added_system_correction = Decimal(3)
default_aggregated_quality = Quality.estimated.value
default_resolution = ResolutionDuration.hour
default_metering_point_type = MarketEvaluationPointType.production.value

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_time_window = {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
default_valid_from = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)
default_valid_to = datetime.strptime("2020-01-01T01:00:00+0000", date_time_formatting_string)

metadata = Metadata("1", "1", "1", "1", "1")


@pytest.fixture(scope="module")
def hourly_production_result_schema():
    """
    Input hourly production result data frame schema
    """
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.sum_quantity, DecimalType()) \
        .add(Colname.time_window, StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.quality, StringType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_point_type, StringType())


@pytest.fixture(scope="module")
def added_system_correction_result_schema():
    """
    Input system correction result schema
    """
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.added_system_correction, DecimalType()) \
        .add(Colname.time_window, StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType()) \
        .add(Colname.quality, StringType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_point_type, StringType())


@pytest.fixture(scope="module")
def sys_cor_schema():
    """
    Input system correction data frame schema
    """
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.from_date, TimestampType()) \
        .add(Colname.to_date, TimestampType()) \
        .add(Colname.is_system_correction, BooleanType())


@pytest.fixture(scope="module")
def expected_schema():
    """
    Expected aggregation schema
    NOTE: Spark seems to add 10 to the precision of the decimal type on summations.
    Thus, the expected schema should be precision of 20, 10 more than the default of 10.
    If this is an issue we can always cast back to the original decimal precision in the aggregate
    function.
    https://stackoverflow.com/questions/57203383/spark-sum-and-decimaltype-precision
    """
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType()) \
        .add(Colname.quality, StringType())


@pytest.fixture(scope="module")
def hourly_production_result_row_factory(spark, hourly_production_result_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                responsible=default_responsible,
                supplier=default_supplier,
                sum_quantity=default_sum_quantity,
                time_window=default_time_window,
                aggregated_quality=default_aggregated_quality,
                resolution=default_resolution,
                metering_point_type=default_metering_point_type):
        pandas_df = pd.DataFrame({
            Colname.grid_area: [domain],
            Colname.balance_responsible_id: [responsible],
            Colname.energy_supplier_id: [supplier],
            Colname.sum_quantity: [sum_quantity],
            Colname.time_window: [time_window],
            Colname.quality: [aggregated_quality],
            Colname.resolution: [resolution],
            Colname.metering_point_type: [metering_point_type]})
        return spark.createDataFrame(pandas_df, schema=hourly_production_result_schema)
    return factory


@pytest.fixture(scope="module")
def added_system_correction_result_row_factory(spark, added_system_correction_result_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                added_system_correction=default_added_system_correction,
                time_window=default_time_window,
                sum_quantity=default_sum_quantity,
                aggregated_quality=default_aggregated_quality,
                resolution=default_resolution,
                metering_point_type=default_metering_point_type):
        pandas_df = pd.DataFrame({
            Colname.grid_area: [domain],
            Colname.added_system_correction: [added_system_correction],
            Colname.time_window: [time_window],
            Colname.sum_quantity: [sum_quantity],
            Colname.quality: [aggregated_quality],
            Colname.resolution: [resolution],
            Colname.metering_point_type: [metering_point_type]})
        return spark.createDataFrame(pandas_df, schema=added_system_correction_result_schema)
    return factory


@pytest.fixture(scope="module")
def sys_cor_row_factory(spark, sys_cor_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                responsible=default_responsible,
                supplier=default_supplier,
                valid_from=default_valid_from,
                valid_to=default_valid_to,
                is_system_correction=True):
        pandas_df = pd.DataFrame({
            Colname.grid_area: [domain],
            Colname.balance_responsible_id: [responsible],
            Colname.energy_supplier_id: [supplier],
            Colname.from_date: [valid_from],
            Colname.to_date: [valid_to],
            Colname.is_system_correction: [is_system_correction]})
        return spark.createDataFrame(pandas_df, schema=sys_cor_schema)
    return factory


def test_grid_area_system_correction_is_added_to_system_correction_energy_responsible(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):
    results = {}
    results[ResultKeyName.hourly_production] = create_dataframe_from_aggregation_result_schema(metadata, hourly_production_result_row_factory(supplier="A"))

    results[ResultKeyName.added_system_correction] = create_dataframe_from_aggregation_result_schema(metadata, added_system_correction_result_row_factory())

    results[ResultKeyName.grid_loss_sys_cor_master_data] = sys_cor_row_factory(supplier="A")

    result_df = adjust_production(results, metadata)

    assert result_df.filter(col(Colname.energy_supplier_id) == "A").collect()[0][Colname.sum_quantity] == default_added_system_correction + default_sum_quantity


def test_grid_area_grid_loss_is_not_added_to_non_grid_loss_energy_responsible(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):
    results = {}
    results[ResultKeyName.hourly_production] = create_dataframe_from_aggregation_result_schema(metadata, hourly_production_result_row_factory(supplier="A"))

    results[ResultKeyName.added_system_correction] = create_dataframe_from_aggregation_result_schema(metadata, added_system_correction_result_row_factory())

    results[ResultKeyName.grid_loss_sys_cor_master_data] = sys_cor_row_factory(supplier="B")

    result_df = adjust_production(results, metadata)

    assert result_df.filter(col(Colname.energy_supplier_id) == "A").collect()[0][Colname.sum_quantity] == default_sum_quantity


def test_result_dataframe_contains_same_number_of_results_with_same_energy_suppliers_as_flex_consumption_result_dataframe(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):
    results = {}
    hp_row_1 = hourly_production_result_row_factory(supplier="A")
    hp_row_2 = hourly_production_result_row_factory(supplier="B")
    hp_row_3 = hourly_production_result_row_factory(supplier="C")

    results[ResultKeyName.hourly_production] = create_dataframe_from_aggregation_result_schema(metadata, hp_row_1.union(hp_row_2).union(hp_row_3))

    results[ResultKeyName.added_system_correction] = create_dataframe_from_aggregation_result_schema(metadata, added_system_correction_result_row_factory())

    results[ResultKeyName.grid_loss_sys_cor_master_data] = sys_cor_row_factory(supplier="C")

    result_df = adjust_production(results, metadata)

    result_df_collect = result_df.collect()
    assert result_df.count() == 3
    assert result_df_collect[0][Colname.energy_supplier_id] == "A"
    assert result_df_collect[1][Colname.energy_supplier_id] == "B"
    assert result_df_collect[2][Colname.energy_supplier_id] == "C"


def test_correct_system_correction_entry_is_used_to_determine_energy_responsible_for_the_given_time_window_from_hourly_production_result_dataframe(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):
    results = {}
    time_window_1 = {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
    time_window_2 = {Colname.start: datetime(2020, 1, 1, 1, 0), Colname.end: datetime(2020, 1, 1, 2, 0)}
    time_window_3 = {Colname.start: datetime(2020, 1, 1, 2, 0), Colname.end: datetime(2020, 1, 1, 3, 0)}

    hp_row_1 = hourly_production_result_row_factory(supplier="A", time_window=time_window_1)
    hp_row_2 = hourly_production_result_row_factory(supplier="B", time_window=time_window_2)
    hp_row_3 = hourly_production_result_row_factory(supplier="B", time_window=time_window_3)

    results[ResultKeyName.hourly_production] = create_dataframe_from_aggregation_result_schema(metadata, hp_row_1.union(hp_row_2).union(hp_row_3))

    gasc_result_1 = Decimal(1)
    gasc_result_2 = Decimal(2)
    gasc_result_3 = Decimal(3)

    gasc_row_1 = added_system_correction_result_row_factory(time_window=time_window_1, added_system_correction=gasc_result_1)
    gasc_row_2 = added_system_correction_result_row_factory(time_window=time_window_2, added_system_correction=gasc_result_2)
    gasc_row_3 = added_system_correction_result_row_factory(time_window=time_window_3, added_system_correction=gasc_result_3)

    results[ResultKeyName.added_system_correction] = create_dataframe_from_aggregation_result_schema(metadata, gasc_row_1.union(gasc_row_2).union(gasc_row_3))

    sc_row_1 = sys_cor_row_factory(supplier="A", valid_from=time_window_1["start"], valid_to=time_window_1["end"])
    sc_row_2 = sys_cor_row_factory(supplier="C", valid_from=time_window_2["start"], valid_to=time_window_2["end"])
    sc_row_3 = sys_cor_row_factory(supplier="B", valid_from=time_window_3["start"], valid_to=None)

    results[ResultKeyName.grid_loss_sys_cor_master_data] = sc_row_1.union(sc_row_2).union(sc_row_3)

    result_df = adjust_production(results, metadata)

    assert result_df.count() == 3
    assert result_df.filter(col(Colname.energy_supplier_id) == "A"). \
        filter(col(f"{Colname.time_window_start}") == time_window_1["start"]). \
        collect()[0][Colname.sum_quantity] == default_sum_quantity + gasc_result_1
    assert result_df.filter(col(Colname.energy_supplier_id) == "B"). \
        filter(col(f"{Colname.time_window_start}") == time_window_2["start"]). \
        collect()[0][Colname.sum_quantity] == default_sum_quantity
    assert result_df.filter(col(Colname.energy_supplier_id) == "B"). \
        filter(col(f"{Colname.time_window_start}") == time_window_3["start"]). \
        collect()[0][Colname.sum_quantity] == default_sum_quantity + gasc_result_3

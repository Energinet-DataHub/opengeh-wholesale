import datetime

from pyspark.sql import SparkSession

import tests.calculation.energy.metering_point_time_series_factories as factories
from geh_wholesale.calculation.energy.aggregators.metering_point_time_series_aggregators import (
    aggregate_per_es,
)
from geh_wholesale.codelists import MeteringPointType, SettlementMethod
from geh_wholesale.constants import Colname

ONE_TIME = datetime.datetime.now()
ANOTHER_TIME = ONE_TIME + datetime.timedelta(minutes=15)
ANY_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION


class TestWhenValidInput:
    def test_returns_values_aggregated_for_ga(self, spark: SparkSession):
        # Arrange
        row = factories.create_row()
        metering_point_time_series = factories.create(spark, [row, row])

        # Act
        actual = aggregate_per_es(
            metering_point_time_series,
            factories.DEFAULT_METERING_POINT_TYPE,
            None,
        )

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        actual_row = actual_rows[0]
        assert actual_row[Colname.grid_area_code] == factories.DEFAULT_GRID_AREA
        assert actual_row[Colname.to_grid_area_code] is None  # None because it's not an exchange result
        assert actual_row[Colname.from_grid_area_code] is None  # None because it's not an exchange result
        assert actual_row[Colname.balance_responsible_party_id] == factories.DEFAULT_BALANCE_RESPONSIBLE_ID
        assert actual_row[Colname.energy_supplier_id] == factories.DEFAULT_ENERGY_SUPPLIER_ID
        assert actual_row[Colname.observation_time] == factories.DEFAULT_OBSERVATION_TIME
        assert actual_row[Colname.quantity] == round(2 * factories.DEFAULT_QUANTITY, 3)
        assert actual_row[Colname.qualities] == [factories.DEFAULT_QUALITY.value]

    def test_returns_rows_for_each_ga(self, spark: SparkSession):
        # Arrange
        rows = [factories.create_row(), factories.create_row(grid_area="another-ga")]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_es(df, factories.DEFAULT_METERING_POINT_TYPE, None)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2

    def test_returns_rows_for_each_observation_time(self, spark: SparkSession):
        # Arrange
        another_time = factories.DEFAULT_OBSERVATION_TIME + datetime.timedelta(minutes=15)
        rows = [
            factories.create_row(),
            factories.create_row(observation_time=another_time),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_es(df, factories.DEFAULT_METERING_POINT_TYPE, None)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2

    def test_returns_rows_for_each_es(self, spark: SparkSession):
        # Arrange
        rows = [
            factories.create_row(),
            factories.create_row(energy_supplier_id="another_es"),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_es(df, factories.DEFAULT_METERING_POINT_TYPE, None)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2

    def test_returns_rows_for_each_brp(self, spark: SparkSession):
        # Arrange
        rows = [
            factories.create_row(),
            factories.create_row(balance_responsible_id="another_brp"),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_es(df, factories.DEFAULT_METERING_POINT_TYPE, None)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2


class TestWhenValidInputAndFilteringApplied:
    def test_returns_rows_for_selected_metering_point_type(self, spark: SparkSession):
        # Arrange
        rows = [
            factories.create_row(grid_area="a", metering_point_type=MeteringPointType.CONSUMPTION),
            factories.create_row(grid_area="b", metering_point_type=MeteringPointType.PRODUCTION),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_es(df, MeteringPointType.CONSUMPTION, None)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.grid_area_code] == "a"

    def test_returns_rows_filtered_by_settlement_method(self, spark: SparkSession):
        # Arrange
        rows = [
            factories.create_row(grid_area="a", settlement_method=SettlementMethod.FLEX),
            factories.create_row(grid_area="b", settlement_method=SettlementMethod.NON_PROFILED),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_es(df, factories.DEFAULT_METERING_POINT_TYPE, SettlementMethod.FLEX)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 1
        assert actual_rows[0][Colname.grid_area_code] == "a"

    def test_returns_rows_not_filtered_by_settlement_method(self, spark: SparkSession):
        # Arrange
        rows = [
            factories.create_row(grid_area="a", settlement_method=SettlementMethod.NON_PROFILED),
            factories.create_row(grid_area="b", settlement_method=SettlementMethod.NON_PROFILED),
        ]
        df = factories.create(spark, rows)

        # Act
        actual = aggregate_per_es(df, factories.DEFAULT_METERING_POINT_TYPE, None)

        # assert
        actual_rows = actual.df.collect()
        assert len(actual_rows) == 2

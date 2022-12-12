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
from tests.helpers.test_schemas import tariff_schema, tariff_sum_and_count_schema, tariff_distinct_schema
from geh_stream.codelists import Colname, ChargeType
from geh_stream.wholesale_utils.calculators.tariff_calculators import \
    sum_quantity_and_count_charges, \
    select_distinct_tariffs, \
    join_with_agg_df
import pytest
import pandas as pd


tariffs_dataset = [
    ("001-D01-001", "001", ChargeType.tariff, "001", "P1D", "No", datetime(2020, 1, 1, 0, 0),
     Decimal("200.50"), "D01", "1", "E17", "E22", "D01", "1", Decimal("1.0005")),
    ("001-D01-001", "001", ChargeType.tariff, "001", "P1D", "No", datetime(2020, 1, 1, 0, 0),
     Decimal("200.50"), "D01", "1", "E17", "E22", "D01", "1", Decimal("1.0005")),
    ("001-D01-002", "001", ChargeType.tariff, "001", "P1D", "No", datetime(2020, 1, 15, 0, 0),
     Decimal("200.50"), "D01", "1", "E17", "E22", "D01", "1", Decimal("1.000"))
]


@pytest.mark.parametrize("tariffs,expected_charge_count,expected_quantity", [
    (tariffs_dataset, 2, Decimal("2.002"))
])
def test__sum_quantity_and_count_charges__counts_quantity_and_sums_up_amount_of_charges(
    spark,
    tariffs,
    expected_charge_count,
    expected_quantity
):
    # Arrange
    tariffs = spark.createDataFrame(tariffs, schema=tariff_schema)

    # Act
    result = sum_quantity_and_count_charges(tariffs)

    # Assert
    result_collect = result.collect()
    assert result_collect[0][Colname.charge_count] == expected_charge_count
    assert result_collect[0][Colname.total_quantity] == expected_quantity


@pytest.mark.parametrize("tariffs,expected_count", [
    (tariffs_dataset, 2)
])
def test__select_distinct_tariffs__selects_distinct_tariffs(
    spark,
    tariffs,
    expected_count
):
    # Arrange
    tariffs = spark.createDataFrame(tariffs, schema=tariff_schema)

    # Act
    result = select_distinct_tariffs(tariffs)

    # Assert
    assert result.count() == expected_count


tariffs_distinct_dataset = [
    ("001-D01-001", "001", ChargeType.tariff, "001", "P1D", "No",
     datetime(2020, 1, 1, 0, 0), Decimal("200.50"), "1", "E17", "D01", "1"),
    ("001-D01-002", "001", ChargeType.tariff, "001", "P1D", "No",
     datetime(2020, 1, 15, 0, 0), Decimal("200.50"), "1", "E17", "D01", "1")
]
agg_dataset = [
    ("1", "1", datetime(2020, 1, 1, 0, 0), "E17", "D01", "001-D01-001", Decimal("2.002"), 2),
    ("1", "1", datetime(2020, 1, 15, 0, 0), "E17", "D01", "001-D01-002", Decimal("1.000"), 1),
]


@pytest.mark.parametrize("tariffs,agg_df,expected_total_amount", [
    (tariffs_distinct_dataset, agg_dataset, Decimal("401.401"))
])
def test__join_with_agg_df__gets_the_expected_total_amount(
    spark,
    tariffs,
    agg_df,
    expected_total_amount
):
    # Arrange
    tariffs = spark.createDataFrame(tariffs, schema=tariff_distinct_schema)
    agg_df = spark.createDataFrame(agg_df, schema=tariff_sum_and_count_schema)

    # Act
    result = join_with_agg_df(tariffs, agg_df)

    # Assert
    assert result.collect()[0][Colname.total_amount] == expected_total_amount

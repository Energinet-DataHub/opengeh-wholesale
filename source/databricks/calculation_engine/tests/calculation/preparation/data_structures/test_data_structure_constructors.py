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
from typing import Type

import pytest
from pyspark.sql import SparkSession, Row

import package.calculation.preparation.data_structures as d


@pytest.mark.parametrize(
    "sut",
    [
        d.PreparedSubscriptions,
        d.PreparedTariffs,
        d.PreparedMeteringPointTimeSeries,
        d.ChargePrices,
        d.ChargeMasterData,
        d.ChargeLinkMeteringPointPeriods,
        d.GridLossResponsible,
    ],
)
def test__constructor__when_invalid_input_schema__raise_assertion_error(
    sut: Type,
    spark: SparkSession,
) -> None:
    # Arrange
    df = spark.createDataFrame(data=[Row(**({"Hello": "World"}))])

    # Act & Assert
    with pytest.raises(Exception):
        sut(df)

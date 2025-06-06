from typing import Type

import pytest
from pyspark.sql import Row, SparkSession

import geh_wholesale.calculation.preparation.data_structures as d


@pytest.mark.parametrize(
    "sut",
    [
        d.PreparedSubscriptions,
        d.PreparedTariffs,
        d.PreparedMeteringPointTimeSeries,
        d.ChargePrices,
        d.ChargePriceInformation,
        d.ChargeLinkMeteringPointPeriods,
        d.GridLossMeteringPointPeriods,
    ],
)
def test__constructor__when_invalid_input_schema__raise_assertion_error(
    sut: Type,
    spark: SparkSession,
) -> None:
    # Arrange
    df = spark.createDataFrame(data=[Row(**({"Hello": "World"}))])

    # Act & Assert
    with pytest.raises(Exception):  # noqa: PT011
        sut(df)

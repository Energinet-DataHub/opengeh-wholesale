import pytest
from pyspark.sql import SparkSession, functions as F
from pyspark.sql.types import StructType, StructField, StringType

from settlement_report_job.domain.utils.map_from_dict import map_from_dict
from settlement_report_job.infrastructure.wholesale.data_values import (
    SettlementMethodDataProductValue,
    MeteringPointTypeDataProductValue,
    ChargeTypeDataProductValue,
    CalculationTypeDataProductValue,
)

import settlement_report_job.domain.utils.map_to_csv_naming as mapping_dicts


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
    actual = df.select(map_from_dict(mapping_dicts.CHARGE_TYPES)[F.col("charge_type")])

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
        map_from_dict(mapping_dicts.CALCULATION_TYPES_TO_PROCESS_VARIANT)[
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
        map_from_dict(mapping_dicts.CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS)[
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
        map_from_dict(mapping_dicts.METERING_POINT_TYPES)[F.col("metering_point_type")]
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
        map_from_dict(mapping_dicts.SETTLEMENT_METHODS)[F.col("settlement_method")]
    )

    # Assert
    assert actual.collect()[0][0] == expected_settlement_method

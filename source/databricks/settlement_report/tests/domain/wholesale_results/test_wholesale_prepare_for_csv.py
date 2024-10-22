import pytest
from pyspark.sql import SparkSession

from settlement_report_job.domain.wholesale_results.prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.wholesale.data_values import (
    CalculationTypeDataProductValue,
    MeteringPointTypeDataProductValue,
)
from settlement_report_job.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)
from test_factories.default_test_data_spec import create_amounts_per_charge_data_spec
from test_factories.amounts_per_charge_factory import create_wholesale


@pytest.mark.parametrize(
    "calculation_type, expected_process_variant",
    [
        (CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT, "1ST"),
        (CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT, "2ND"),
        (CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT, "3RD"),
    ],
)
def test_when_calculation_type_correction__then_process_variant_has_value(
    spark: SparkSession, calculation_type, expected_process_variant
) -> None:
    spec = create_amounts_per_charge_data_spec(calculation_type=calculation_type)
    wholesale = create_wholesale(spark, spec)
    actual = prepare_for_csv(wholesale)
    assert actual.collect()[0]["PROCESSVARIANT"] == expected_process_variant


@pytest.mark.parametrize(
    "calculation_type",
    [
        CalculationTypeDataProductValue.WHOLESALE_FIXING,
        CalculationTypeDataProductValue.BALANCE_FIXING,
    ],
)
def test_when_calculation_type_not_correction__then_process_variant_is_empty(
    spark: SparkSession, calculation_type
) -> None:
    spec = create_amounts_per_charge_data_spec(calculation_type=calculation_type)
    wholesale = create_wholesale(spark, spec)
    actual = prepare_for_csv(wholesale)
    assert actual.collect()[0]["PROCESSVARIANT"] is None


@pytest.mark.parametrize(
    "calculation_type, expected_energy_business_process",
    [
        (CalculationTypeDataProductValue.BALANCE_FIXING, "D04"),
        (CalculationTypeDataProductValue.WHOLESALE_FIXING, "D05"),
        (CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT, "D32"),
        (CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT, "D32"),
        (CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT, "D32"),
    ],
)
def test_when_calculation_type__then_energy_business_process(
    spark: SparkSession, calculation_type, expected_energy_business_process
) -> None:
    spec = create_amounts_per_charge_data_spec(calculation_type=calculation_type)
    wholesale = create_wholesale(spark, spec)
    actual = prepare_for_csv(wholesale)
    assert (
        actual.collect()[0]["ENERGYBUSINESSPROCESS"] == expected_energy_business_process
    )


@pytest.mark.parametrize(
    "metering_point_type, expected_type_of_mp",
    [
        (MeteringPointTypeDataProductValue.CONSUMPTION, "E17"),
        (MeteringPointTypeDataProductValue.PRODUCTION, "E18"),
        (MeteringPointTypeDataProductValue.EXCHANGE, "E20"),
        (MeteringPointTypeDataProductValue.VE_PRODUCTION, "D01"),
        (MeteringPointTypeDataProductValue.NET_PRODUCTION, "D05"),
        (MeteringPointTypeDataProductValue.SUPPLY_TO_GRID, "D06"),
        (MeteringPointTypeDataProductValue.CONSUMPTION_FROM_GRID, "D07"),
        (MeteringPointTypeDataProductValue.WHOLESALE_SERVICES_INFORMATION, "D08"),
        (MeteringPointTypeDataProductValue.OWN_PRODUCTION, "D09"),
        (MeteringPointTypeDataProductValue.NET_FROM_GRID, "D10"),
        (MeteringPointTypeDataProductValue.NET_TO_GRID, "D11"),
        (MeteringPointTypeDataProductValue.TOTAL_CONSUMPTION, "D12"),
        (MeteringPointTypeDataProductValue.ELECTRICAL_HEATING, "D14"),
        (MeteringPointTypeDataProductValue.NET_CONSUMPTION, "D15"),
        (MeteringPointTypeDataProductValue.EFFECT_SETTLEMENT, "D19"),
    ],
)
def test_when_metering_point_type__then_type_of_mp(
    spark: SparkSession, metering_point_type, expected_type_of_mp
) -> None:
    spec = create_amounts_per_charge_data_spec(metering_point_type=metering_point_type)
    wholesale = create_wholesale(spark, spec)
    actual = prepare_for_csv(wholesale)
    assert actual.collect()[0]["TYPEOFMP"] == expected_type_of_mp


@pytest.mark.parametrize(
    "settlement_method, expected_settlement_method",
    [
        (SettlementMethodDataProductValue.NON_PROFILED.value, "E02"),
        (SettlementMethodDataProductValue.FLEX.value, "D01"),
    ],
)
def test_when_settlement_method__then_settlement_method(
    spark: SparkSession, settlement_method, expected_settlement_method
) -> None:
    spec = create_amounts_per_charge_data_spec(settlement_method=settlement_method)
    wholesale = create_wholesale(spark, spec)
    actual = prepare_for_csv(wholesale)
    assert actual.collect()[0]["SETTLEMENTMETHOD"] == expected_settlement_method

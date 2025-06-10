import pyspark.sql.functions as f
from pyspark.sql import DataFrame
from pyspark.sql.types import ArrayType, DecimalType, StringType

from geh_wholesale.codelists import ChargeType, ChargeUnit
from geh_wholesale.constants import Colname


def calculate_total_quantity_and_amount(prepared_charge: DataFrame, charge_type: ChargeType) -> DataFrame:
    qualities_function = _get_qualities_function(charge_type)

    df = prepared_charge.groupBy(
        Colname.charge_key,
        Colname.charge_type,
        Colname.charge_code,
        Colname.charge_owner,
        Colname.grid_area_code,
        Colname.energy_supplier_id,
        Colname.charge_time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.resolution,
        Colname.charge_tax,
        Colname.charge_price,
    ).agg(
        f.sum(Colname.quantity).alias(Colname.total_quantity),
        qualities_function,
    )

    df = df.withColumn(
        Colname.total_amount,
        (f.col(Colname.total_quantity) * f.col(Colname.charge_price)).cast(DecimalType(18, 6)),
    )

    df = df.withColumn(Colname.total_quantity, f.col(Colname.total_quantity).cast(DecimalType(18, 3)))
    df = df.withColumn(Colname.charge_price, f.col(Colname.charge_price).cast(DecimalType(18, 6)))

    df = _add_charge_unit(df, charge_type)

    return df


def _get_qualities_function(charge_type: ChargeType) -> f.Column:
    if charge_type == ChargeType.TARIFF:
        return f.flatten(f.collect_set(Colname.qualities)).alias(Colname.qualities)
    elif charge_type == ChargeType.FEE or charge_type == ChargeType.SUBSCRIPTION:
        return f.lit(None).cast(ArrayType(StringType())).alias(Colname.qualities)
    else:
        raise ValueError(f"Unknown charge type: {charge_type.value}")


def _add_charge_unit(df: DataFrame, charge_type: ChargeType) -> DataFrame:
    if charge_type == ChargeType.TARIFF:
        charge_unit = ChargeUnit.KWH
    elif charge_type == ChargeType.FEE or charge_type == ChargeType.SUBSCRIPTION:
        charge_unit = ChargeUnit.PIECES
    else:
        raise ValueError(f"Unknown charge type: {charge_type.value}")

    return df.withColumn(Colname.unit, f.lit(charge_unit.value))

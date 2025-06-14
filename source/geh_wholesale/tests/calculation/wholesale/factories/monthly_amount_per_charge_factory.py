import datetime
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.wholesale.data_structures import MonthlyAmountPerCharge
from geh_wholesale.calculation.wholesale.data_structures.monthly_amount_per_charge import (
    monthly_amount_per_charge_schema,
)
from geh_wholesale.codelists import (
    ChargeType,
)
from geh_wholesale.constants import Colname


def create_row(
    grid_area: str = "543",
    energy_supplier_id: str = "1234567890123",
    unit: str = "kWh",
    charge_time: datetime = datetime.datetime.now(),
    total_amount: int | Decimal | None = None,
    charge_tax: bool = True,
    charge_code: str = "4000",
    charge_type: ChargeType = ChargeType.TARIFF,
    charge_owner: str = "001",
) -> Row:
    row = {
        Colname.grid_area_code: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.unit: unit,
        Colname.charge_time: charge_time,
        Colname.total_amount: total_amount,
        Colname.charge_tax: charge_tax,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
    }

    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> MonthlyAmountPerCharge:
    """If data is None, a single row with default values is created."""
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=monthly_amount_per_charge_schema)
    return MonthlyAmountPerCharge(df)

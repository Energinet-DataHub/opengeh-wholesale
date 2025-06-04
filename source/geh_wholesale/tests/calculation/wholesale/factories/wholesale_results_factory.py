import datetime
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
    wholesale_results_schema,
)
from geh_wholesale.codelists import (
    ChargeQuality,
    ChargeType,
    MeteringPointType,
    SettlementMethod,
)
from geh_wholesale.constants import Colname

default_qualities = [ChargeQuality.CALCULATED]


def create_row(
    grid_area: str = "543",
    energy_supplier_id: str = "1234567890123",
    total_quantity: int | Decimal | None = None,
    unit: str = "kWh",
    qualities: list[ChargeQuality] | None = None,
    charge_time: datetime = datetime.datetime.now(),
    resolution: str = "PT1H",
    metering_point_type: MeteringPointType = MeteringPointType.CONSUMPTION,
    settlement_method: SettlementMethod = SettlementMethod.FLEX,
    charge_price: int | Decimal | None = None,
    total_amount: int | Decimal | None = None,
    charge_tax: bool = True,
    charge_code: str = "4000",
    charge_type: ChargeType = ChargeType.TARIFF,
    charge_owner: str = "001",
) -> Row:
    if isinstance(total_quantity, int):
        total_quantity = Decimal(total_quantity)
    if isinstance(charge_price, int):
        charge_price = Decimal(charge_price)
    if isinstance(total_quantity, int):
        total_amount = Decimal(total_amount)

    row = {
        Colname.grid_area_code: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.total_quantity: total_quantity,
        Colname.unit: unit,
        Colname.qualities: qualities,
        Colname.charge_time: charge_time,
        Colname.resolution: resolution,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.charge_price: charge_price,
        Colname.total_amount: total_amount,
        Colname.charge_tax: charge_tax,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
    }

    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> WholesaleResults:
    """If data is None, a single row with default values is created."""
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=wholesale_results_schema)
    return WholesaleResults(df)

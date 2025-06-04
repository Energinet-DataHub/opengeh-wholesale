import datetime
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from geh_wholesale.codelists import MeteringPointType, QuantityQuality, SettlementMethod
from geh_wholesale.constants import Colname

DEFAULT_GRID_AREA = "100"
DEFAULT_OBSERVATION_TIME = datetime.datetime.now()
DEFAULT_QUANTITY = Decimal("999.123")
DEFAULT_QUALITIES = [QuantityQuality.MEASURED]
DEFAULT_METERING_POINT_ID = "1234567890234"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.NON_PROFILED
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "9999999999999"


def create_row(
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str | None = None,
    to_grid_area: str | None = None,
    observation_time: datetime.datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
    qualities: None | QuantityQuality | list[QuantityQuality] = None,
    energy_supplier_id: str | None = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str | None = DEFAULT_BALANCE_RESPONSIBLE_ID,
    metering_point_id: str | None = None,
) -> Row:
    if isinstance(quantity, int):
        quantity = Decimal(quantity)

    if qualities is None:
        qualities = DEFAULT_QUALITIES
    elif isinstance(qualities, QuantityQuality):
        qualities = [qualities]
    qualities = [q.value for q in qualities]

    row = {
        Colname.grid_area_code: grid_area,
        Colname.from_grid_area_code: from_grid_area,
        Colname.to_grid_area_code: to_grid_area,
        Colname.balance_responsible_party_id: balance_responsible_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.observation_time: observation_time,
        Colname.quantity: quantity,
        Colname.qualities: qualities,
        Colname.metering_point_type: metering_point_id,
    }

    return Row(**row)


def create_grid_loss_row(
    grid_area: str = DEFAULT_GRID_AREA,
    observation_time: datetime.datetime = DEFAULT_OBSERVATION_TIME,
    quantity: int | Decimal = DEFAULT_QUANTITY,
) -> Row:
    """Suggestion: Consider creating a type for grid loss results."""
    return create_row(
        grid_area=grid_area,
        from_grid_area=None,
        to_grid_area=None,
        observation_time=observation_time,
        quantity=quantity,
        qualities=[QuantityQuality.CALCULATED],  # Grid loss has exactly this quality
        energy_supplier_id=None,  # Is not added until positive/negative grid loss
        balance_responsible_id=None,  # Never exists for grid loss metering points
        metering_point_id=None,  # Is not added until positive/negative grid loss
    )


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> EnergyResults:
    """If data is None, a single row with default values is created."""
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=energy_results_schema)
    return EnergyResults(df)

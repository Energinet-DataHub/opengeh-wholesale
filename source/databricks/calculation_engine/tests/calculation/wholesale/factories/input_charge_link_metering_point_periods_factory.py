from datetime import datetime

from pyspark import Row

from package.codelists import ChargeType
from package.constants import Colname


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_TYPE = ChargeType.FEE
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_TYPE = "consumption"
    METERING_POINT_ID = "123456789012345678901234567"
    SETTLEMENT_METHOD = "D01"
    QUANTITY = 1
    FROM_DATE: datetime = datetime(2019, 12, 31, 23)
    TO_DATE: datetime = datetime(2020, 1, 31, 23)


def create_row(
    charge_type: str = DefaultValues.CHARGE_TYPE.value,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    quantity: int = DefaultValues.QUANTITY,
    metering_point_type: str = DefaultValues.METERING_POINT_TYPE,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    settlement_method: str = DefaultValues.SETTLEMENT_METHOD,
    grid_area: str = DefaultValues.GRID_AREA,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime = DefaultValues.TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_type: charge_type,
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.metering_point_type: metering_point_type,
        Colname.settlement_method: settlement_method,
        Colname.grid_area_code: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
    }

    return Row(**row)

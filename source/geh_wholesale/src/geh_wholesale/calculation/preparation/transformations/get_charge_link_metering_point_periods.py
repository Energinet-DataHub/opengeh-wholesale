import pyspark.sql.functions as f
from pyspark.sql import DataFrame

from geh_wholesale.calculation.preparation.data_structures.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from geh_wholesale.constants import Colname


def get_charge_link_metering_point_periods(
    charge_links: DataFrame,
    metering_points: DataFrame,
) -> ChargeLinkMeteringPointPeriods:
    charge_link_metering_point_periods = (
        charge_links.join(
            metering_points,
            Colname.metering_point_id,
            "inner",
        )
        # We only want the overlap between the metering point period and the charge link period.
        .where(
            (charge_links[Colname.from_date] < metering_points[Colname.to_date])
            & (charge_links[Colname.to_date] > metering_points[Colname.from_date])
        )
    ).select(
        charge_links[Colname.charge_key],
        charge_links[Colname.charge_type],
        charge_links[Colname.metering_point_id],
        charge_links[Colname.quantity],
        f.when(
            charge_links[Colname.from_date] > metering_points[Colname.from_date],
            charge_links[Colname.from_date],
        )
        .otherwise(metering_points[Colname.from_date])
        .alias(Colname.from_date),
        f.when(
            charge_links[Colname.to_date] < metering_points[Colname.to_date],
            charge_links[Colname.to_date],
        )
        .otherwise(metering_points[Colname.to_date])
        .alias(Colname.to_date),
        metering_points[Colname.metering_point_type],
        metering_points[Colname.settlement_method],
        Colname.grid_area_code,
        Colname.energy_supplier_id,
    )

    return ChargeLinkMeteringPointPeriods(charge_link_metering_point_periods)

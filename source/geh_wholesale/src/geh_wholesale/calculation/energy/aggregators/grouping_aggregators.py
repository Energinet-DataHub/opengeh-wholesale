from geh_wholesale.calculation.energy.aggregators.transformations.aggregate_sum_and_quality import (
    aggregate_sum_quantity_and_qualities,
)
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.constants import Colname


def aggregate(df: EnergyResults) -> EnergyResults:
    group_by = [Colname.grid_area_code, Colname.observation_time]
    result = aggregate_sum_quantity_and_qualities(df.df, group_by)
    return EnergyResults(result)


def aggregate_per_brp(df: EnergyResults) -> EnergyResults:
    """Aggregate sum per grid area and balance responsible party."""
    group_by = [
        Colname.grid_area_code,
        Colname.balance_responsible_party_id,
        Colname.observation_time,
    ]
    result = aggregate_sum_quantity_and_qualities(df.df, group_by)
    return EnergyResults(result)

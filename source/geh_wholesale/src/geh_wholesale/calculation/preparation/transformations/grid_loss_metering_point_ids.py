from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_ids import (
    GridLossMeteringPointIds,
)
from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_periods import (
    GridLossMeteringPointPeriods,
)
from geh_wholesale.constants import Colname


def get_grid_loss_metering_point_ids(
    grid_loss_metering_point_periods: GridLossMeteringPointPeriods,
) -> GridLossMeteringPointIds:
    return GridLossMeteringPointIds(grid_loss_metering_point_periods.df.select(Colname.metering_point_id).distinct())

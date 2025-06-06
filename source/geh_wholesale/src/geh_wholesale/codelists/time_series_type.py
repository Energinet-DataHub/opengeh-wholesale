from enum import Enum


class TimeSeriesType(Enum):
    """The type of the aggregated time series type of an energy calculation."""

    PRODUCTION = "production"
    NON_PROFILED_CONSUMPTION = "non_profiled_consumption"
    EXCHANGE_PER_NEIGHBOR = "net_exchange_per_neighboring_ga"
    EXCHANGE = "net_exchange_per_ga"
    FLEX_CONSUMPTION = "flex_consumption"
    GRID_LOSS = "grid_loss"
    NEGATIVE_GRID_LOSS = "negative_grid_loss"
    POSITIVE_GRID_LOSS = "positive_grid_loss"
    TOTAL_CONSUMPTION = "total_consumption"
    TEMP_FLEX_CONSUMPTION = "temp_flex_consumption"
    TEMP_PRODUCTION = "temp_production"

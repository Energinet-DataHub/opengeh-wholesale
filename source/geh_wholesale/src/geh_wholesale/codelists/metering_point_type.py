from enum import Enum


class MeteringPointType(Enum):
    PRODUCTION = "production"
    CONSUMPTION = "consumption"
    EXCHANGE = "exchange"
    # The following are for child metering points
    VE_PRODUCTION = "ve_production"
    NET_PRODUCTION = "net_production"
    SUPPLY_TO_GRID = "supply_to_grid"
    CONSUMPTION_FROM_GRID = "consumption_from_grid"
    WHOLESALE_SERVICES_INFORMATION = "wholesale_services_information"
    OWN_PRODUCTION = "own_production"
    NET_FROM_GRID = "net_from_grid"
    NET_TO_GRID = "net_to_grid"
    TOTAL_CONSUMPTION = "total_consumption"
    ELECTRICAL_HEATING = "electrical_heating"
    NET_CONSUMPTION = "net_consumption"
    CAPACITY_SETTLEMENT = "capacity_settlement"

from dependency_injector.wiring import Provide, Container

from package.calculation.calculation_output import CalculationOutput
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_es_link import (
    CalculateNonProfiledConsumptionPerEsLink,
)
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_grid_area_link import (
    CalculateNonProfiledConsumptionPerGridAreaLink,
)
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_per_brp_link import (
    CalculateNonProfiledConsumptionPerBrpLink,
)
from package.calculation.domain.calculation_links.create_calculation_meta_data_link import (
    CreateCalculationMetaDataLink,
)
from package.calculation.domain.calculation_links.end_link import EndCalculationLink
from package.calculation.domain.calculation_links.energy_total_consumption_link import (
    CalculateTotalEnergyConsumptionLink,
)
from package.calculation.domain.calculation_links.get_metering_point_periods_link import (
    GetMeteringPointPeriodsLink,
)
from package.calculation.domain.calculation_links.start_link import StartCalculationLink
from package.calculation.domain.chains.cache_bucket import CacheBucket


class Chain:

    def __init__(self, cache_bucket: CacheBucket = Provide[Container.bucket]):
        start_link: StartCalculationLink = Provide[Container.start_calculation_link]

    def setup_:
        # Set up the calculation chain
        (
            start_link.set_next(CreateCalculationMetaDataLink())
            .set_next(GetMeteringPointPeriodsLink())
            .set_next(CalculateTotalEnergyConsumptionLink())
            .set_next(CalculateNonProfiledConsumptionPerEsLink())
            .set_next(CalculateNonProfiledConsumptionPerBrpLink())
            .set_next(CalculateNonProfiledConsumptionPerGridAreaLink())
            .set_next(EndCalculationLink())
        )

        # Execute calculation chain
        start_link.execute(CalculationOutput())

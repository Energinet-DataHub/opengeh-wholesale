from package.calculation.calculation_output import CalculationOutput

from package.calculation.domain.calculation_links.get_metering_point_periods_link import (
    GetMeteringPointPeriodsLink,
)
from package.calculation.domain.calculation_links.start_link import StartCalculationLink


class Chain:

    def __init__(
        self,
    ) -> None:
        # Set up the calculation chain
        self.start_link = StartCalculationLink()
        self.start_link.set_next(GetMeteringPointPeriodsLink())
        # .set_next(CalculateTotalEnergyConsumptionLink())
        # .set_next(CalculateNonProfiledConsumptionPerEsLink())
        # .set_next(CalculateNonProfiledConsumptionPerBrpLink())
        # .set_next(CalculateNonProfiledConsumptionPerGridAreaLink())
        # .set_next(EndCalculationLink())

    def execute(self) -> CalculationOutput:
        return self.start_link.execute(CalculationOutput())

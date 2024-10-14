from package.calculation.calculation_output import CalculationOutput
from package.calculation.domain.calculation_links import (
    StartCalculationLink,
    EndCalculationLink,
)
from package.calculation.domain.calculation_links.calculate_grid_loss_metering_point_periods_link import (
    CalculateGridLossMeteringPointPeriodsLink,
)
from package.calculation.domain.calculation_links.energy_total_consumption_link import (
    CalculateTotalEnergyConsumptionLink,
)

from package.calculation.domain.calculation_links.get_metering_point_periods_link import (
    GetMeteringPointPeriodsLink,
)


class Chain:

    def __init__(
        self,
    ) -> None:

        # Set up the calculation chain
        self.start_link = StartCalculationLink()
        (
            self.start_link.set_next(GetMeteringPointPeriodsLink())
            .set_next(CalculateGridLossMeteringPointPeriodsLink())
            .set_next(CalculateTotalEnergyConsumptionLink())
            .set_next(EndCalculationLink())
        )

        # .set_next(CalculateTotalEnergyConsumptionLink())
        # .set_next(CalculateNonProfiledConsumptionPerEsLink())
        # .set_next(CalculateNonProfiledConsumptionPerBrpLink())
        # .set_next(CalculateNonProfiledConsumptionPerGridAreaLink())

    def execute(self) -> CalculationOutput:
        return self.start_link.execute(CalculationOutput())

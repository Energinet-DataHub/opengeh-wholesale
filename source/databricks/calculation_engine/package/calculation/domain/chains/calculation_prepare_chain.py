from package.calculation.calculation_output import CalculationOutput
from package.calculation.domain.calculation_links import (
    StartCalculationLink,
    EndCalculationLink,
    CacheMeteringPointPeriodsLink,
    CalculateGridLossMeteringPointPeriodsLink,
    CalculateTotalEnergyConsumptionLink,
)


class CalcuEnergyChain:

    def __init__(
        self,
    ) -> None:

        # Set up the calculation chain
        self.start_link = StartCalculationLink()
        (
            self.start_link.set_next(CacheMeteringPointPeriodsLink())
            .set_next(CalculateGridLossMeteringPointPeriodsLink())
            .set_next(CalculateTotalEnergyConsumptionLink())
            .set_next(EndCalculationLink())
        )

    def execute(self) -> CalculationOutput:
        return self.start_link.execute(CalculationOutput())

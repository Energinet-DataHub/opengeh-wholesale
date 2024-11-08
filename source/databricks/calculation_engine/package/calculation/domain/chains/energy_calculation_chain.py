from package.calculation.calculation_output import CalculationOutput
from package.calculation.domain.calculation_links import (
    StartCalculationLink,
)


class EnergyCalculationChain:

    def __init__(
        self,
    ) -> None:

        # Set up the calculation chain
        self.start_link = StartCalculationLink()

    def execute(self) -> CalculationOutput:
        return self.start_link.execute(CalculationOutput())

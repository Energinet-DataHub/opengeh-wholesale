from package.calculation.calculation_output import CalculationOutput
from package.calculation.domain.calculation_links import (
    StartCalculationLink,
)


class PrepareCalculationChain:

    def __init__(
        self,
    ) -> None:

        self.start_link = StartCalculationLink()

    def execute(self) -> CalculationOutput:
        return self.start_link.execute(CalculationOutput())

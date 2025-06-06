from ..databases.wholesale_internal.calculation_writer import (
    write_calculation,
    write_calculation_grid_areas,
    write_calculation_succeeded_time,
)
from ..databases.wholesale_internal.calculations_grid_areas_storage_model_factory import (
    create_calculation_grid_areas,
)
from .calculator_args import CalculatorArgs


class CalculationMetadataService:
    @staticmethod
    def write(args: CalculatorArgs) -> None:
        write_calculation(args)

        calculation_grid_areas = create_calculation_grid_areas(args)
        write_calculation_grid_areas(calculation_grid_areas)

    @staticmethod
    def write_calculation_succeeded_time(calculation_id: str) -> None:
        write_calculation_succeeded_time(calculation_id)

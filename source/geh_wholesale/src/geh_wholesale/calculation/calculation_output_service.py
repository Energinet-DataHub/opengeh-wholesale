import geh_wholesale.databases.wholesale_basis_data_internal as basis_data_database
import geh_wholesale.databases.wholesale_results_internal as result_database

from .calculation_output import (
    CalculationOutput,
)


class CalculationOutputService:
    @staticmethod
    def write(
        calculation_output: CalculationOutput,
    ) -> None:
        result_database.write_energy_results(calculation_output.energy_results_output)
        if calculation_output.wholesale_results_output is not None:
            result_database.write_wholesale_results(calculation_output.wholesale_results_output)
            result_database.write_monthly_amounts_per_charge(calculation_output.wholesale_results_output)
            result_database.write_total_monthly_amounts(calculation_output.wholesale_results_output)

        if calculation_output.basis_data_output is not None:
            # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
            basis_data_database.write_basis_data(calculation_output.basis_data_output)

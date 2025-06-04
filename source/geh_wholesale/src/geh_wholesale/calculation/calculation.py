from .calculation_core import CalculationCore
from .calculation_metadata_service import CalculationMetadataService
from .calculation_output_service import CalculationOutputService
from .calculator_args import CalculatorArgs
from .preparation import PreparedDataReader


def execute(
    args: CalculatorArgs,
    prepared_data_reader: PreparedDataReader,
    calculation_core: CalculationCore,
    calculation_metadata_service: CalculationMetadataService,
    calculation_output_service: CalculationOutputService,
) -> None:
    calculation_metadata_service.write(args)

    output = calculation_core.execute(args, prepared_data_reader)

    calculation_output_service.write(output)

    # IMPORTANT: Write the succeeded calculation after the results to ensure that the calculation
    # is only marked as succeeded when all results are written
    calculation_metadata_service.write_calculation_succeeded_time(args.calculation_id)

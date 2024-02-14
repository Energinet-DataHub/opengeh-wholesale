# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from package.calculation.CalculationResults import BasisDataContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_output.basis_data_writer import BasisDataWriter
from package.codelists import AggregationLevel
from package.constants import PartitionKeyName
from package.infrastructure import logging_configuration


@logging_configuration.use_span("calculation.basis_data")
def write_basis_data(args: CalculatorArgs, basis_data: BasisDataContainer) -> None:
    basis_data_writer = BasisDataWriter(
        args.wholesale_container_path, args.calculation_id
    )

    write_ga_basis_data_to_csv(basis_data, basis_data_writer)
    write_es_basis_data_to_csv(basis_data, basis_data_writer)


@logging_configuration.use_span("per_grid_area")
def write_ga_basis_data_to_csv(
    basis_data: BasisDataContainer, basis_data_writer: BasisDataWriter
) -> None:
    grouping_folder_name = f"grouping={AggregationLevel.TOTAL_GA.value}"
    partition_keys = [PartitionKeyName.GRID_AREA]

    basis_data_writer.write_basis_data_to_csv(
        basis_data.master_basis_data_for_total_ga,
        basis_data.time_series_quarter_basis_data_for_total_ga,
        basis_data.time_series_hour_basis_data,
        grouping_folder_name,
        partition_keys,
    )


@logging_configuration.use_span("per_energy_supplier")
def write_es_basis_data_to_csv(
    basis_data: BasisDataContainer, basis_data_writer: BasisDataWriter
) -> None:
    grouping_folder_name = f"grouping={AggregationLevel.ES_PER_GA.value}"
    partition_keys = [
        PartitionKeyName.GRID_AREA,
        PartitionKeyName.ENERGY_SUPPLIER_GLN,
    ]

    basis_data_writer.write_basis_data_to_csv(
        basis_data.master_basis_data_for_es_per_ga,
        basis_data.time_series_quarter_basis_data_for_es_per_ga,
        basis_data.time_series_hour_basis_data_for_es_per_ga,
        grouping_folder_name,
        partition_keys,
    )

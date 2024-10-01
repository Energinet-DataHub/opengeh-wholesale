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

from package.calculation import PreparedDataReader
from package.calculation.calculation_output import CalculationOutput
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_es_link import (
    CalculateNonProfiledConsumptionPerEsLink,
)
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_grid_area_link import (
    CalculateNonProfiledConsumptionPerGridAreaLink,
)
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_per_brp_link import (
    CalculateNonProfiledConsumptionPerBrpLink,
)
from package.calculation.domain.calculation_links.calculation_link import (
    CalculationLink,
)
from package.calculation.domain.calculation_links.create_calculation_meta_data_link import (
    CreateCalculationMetaDataLink,
)
from package.calculation.domain.calculation_links.energy_total_consumption_link import (
    CalculateTotalEnergyConsumptionLink,
)
from package.calculation.domain.calculation_links.get_metering_point_periods_link import (
    GetMeteringPointPeriodsLink,
)
from package.calculation.domain.calculation_links.start_link import StartCalculationLink
from package.calculation.energy.data_structures.energy_results import EnergyResults


class Chain:

    def __init__(
        self,
        calculator_args: CalculatorArgs,
    ):

        prepared_data_reader: PreparedDataReader

        # TODO AJW:
        # - Based on the chain of responsibility and decorator pattern
        # - Calculation results are store in CalculationOutput (the set method is only allowed once)
        # - CalculationOutput is passed down the chain
        # - Use DI where ever possible in links, services and repos
        # - Use a bucket for data that can be reused in other links
        # - Add caching to bucket? To repositories? The point is to have caching one place only or at least define it to one layer
        # - 3 Layers
        # - Sub-chains might clarify logic fx. energy chain and wholesale chain and write to UC chain
        # - A calculation link must contribute to calculation output otherwise its not a link?
        #   It means that get all metering points isn't a link's job and the first link that needs metering points must fetch them
        # - Inject of bucket.grid_loss_metering_points? How to do that? Can you inject bucket.grid_loss_metering_points?

        # Minor
        # - Make a common static class for the DI validation in __init__
        # -

        non_profiled_consumption_per_es = EnergyResults()

        start_link = StartCalculationLink()
        create_calculation_meta_data_link = CreateCalculationMetaDataLink()

        calculate_non_profiled_consumption_per_es_step = (
            CalculateNonProfiledConsumptionPerEsLink(
                calculator_args, non_profiled_consumption_per_es
            )
        )
        calculate_non_profiled_consumption_per_brp_step = (
            CalculateNonProfiledConsumptionPerBrpLink(
                calculator_args, non_profiled_consumption_per_es
            )
        )
        calculate_non_profiled_consumption_per_grid_area_step = (
            CalculateNonProfiledConsumptionPerGridAreaLink(
                calculator_args, non_profiled_consumption_per_es
            )
        )

        end_step = CalculationLink()

        # Set up the calculation chain
        (
            start_link.set_next(create_calculation_meta_data_link)
            .set_next(GetMeteringPointPeriodsLink())
            .set_next(CalculateTotalEnergyConsumptionLink())
            .set_next(calculate_non_profiled_consumption_per_es_step)
            .set_next(calculate_non_profiled_consumption_per_brp_step)
            .set_next(calculate_non_profiled_consumption_per_grid_area_step)
            .set_next(end_step)
        )

        # Execute calculation chain
        start_link.execute(CalculationOutput())

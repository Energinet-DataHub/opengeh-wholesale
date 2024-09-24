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
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_es_step import (
    CalculateNonProfiledConsumptionPerEsStep,
)

from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_grid_area_step import (
    CalculateNonProfiledConsumptionPerGridAreaLink,
)
from package.calculation.domain.calculation_links.calculate_not_profiled_consumption_per_brp_step import (
    CalculateNonProfiledConsumptionPerBrpStep,
)
from package.calculation.domain.calculation_links.create_calculation_meta_data_step import (
    CreateCalculationMetaDataLink,
    SaveCalculationMetaDataStep,
)
from package.calculation.domain.calculation_links.energy_total_consumption_step import (
    CalculateTotalEnergyConsumptionStep,
)
from package.calculation.domain.calculation_links.start_link import StartCalculationLink
from package.calculation.energy.data_structures.energy_results import EnergyResults
from package.calculation.wholesale.links.calculation_link import (
    CalculationLink,
)
from package.calculation.wholesale.links.get_metering_point_periods_link import (
    GetMeteringPointPeriodsLink,
)
from package.calculation.wholesale.links.metering_point_period_repository import (
    CalculationMetaData,
    IMeteringPointPeriodRepository,
)


class CacheBucket:

    def __init__(self):
        self._metering_points = None

    calculator_args: CalculationMetaData

    @property
    def metering_points(self):
        return self._metering_points

    @metering_points.setter
    def metering_points(self, value):
        if self._metering_points is not None:
            raise AttributeError("metering_points can only be set once.")
        self._metering_points = value


class Chain:

    def __init__(
        self,
        calculator_args: CalculatorArgs,
        metering_point_period_repository: IMeteringPointPeriodRepository,
    ):

        prepared_data_reader: PreparedDataReader
        bucket = CacheBucket()
        non_profiled_consumption_per_es = EnergyResults()

        start_link = StartCalculationLink()
        create_calculation_meta_data_link = CreateCalculationMetaDataLink()
        save_calculation_meta_data_step = SaveCalculationMetaDataStep()
        calculate_total_energy_consumption_step = CalculateTotalEnergyConsumptionStep(
            calculator_args,
        )

        calculate_non_profiled_consumption_per_es_step = (
            CalculateNonProfiledConsumptionPerEsStep(
                calculator_args, non_profiled_consumption_per_es
            )
        )
        calculate_non_profiled_consumption_per_brp_step = (
            CalculateNonProfiledConsumptionPerBrpStep(
                calculator_args, non_profiled_consumption_per_es
            )
        )
        calculate_non_profiled_consumption_per_grid_area_step = (
            CalculateNonProfiledConsumptionPerGridAreaLink(
                calculator_args, non_profiled_consumption_per_es
            )
        )

        get_metering_point_periods_link = GetMeteringPointPeriodsLink()

        end_step = CalculationLink()

        # Set up the calculation chain
        (
            start_link.set_next(create_calculation_meta_data_link)
            .set_next(get_metering_point_periods_link)
            .set_next(save_calculation_meta_data_step)
            .set_next(calculate_total_energy_consumption_step)
            .set_next(calculate_non_profiled_consumption_per_es_step)
            .set_next(calculate_non_profiled_consumption_per_brp_step)
            .set_next(calculate_non_profiled_consumption_per_grid_area_step)
            .set_next(end_step)
        )

        # Execute calculation chain
        start_link.execute(CalculationOutput())

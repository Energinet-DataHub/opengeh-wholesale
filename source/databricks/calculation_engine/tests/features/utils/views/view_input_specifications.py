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
import package.calculation.basis_data.schemas as basis_data_schemas
from package.calculation.output.schemas import (
    energy_results_schema,
    wholesale_results_schema,
)
from package.calculation.output.schemas.total_monthly_amounts_schema import (
    total_monthly_amounts_schema,
)


def get_input_specifications() -> dict[str, tuple]:
    """
    Contains the specifications for view scenario inputs.
    The key is the name of the file to be read.
    The value is a tuple containing the schema, the name of the method that reads the data,
    the method that to corrects the dataframe types, and the database name.
    """
    return {
        # basis data
        "basis_data.metering_point_periods.csv": (
            basis_data_schemas.metering_point_period_schema,
        ),
        "basis_data.time_series_points.csv": (
            basis_data_schemas.time_series_point_schema,
        ),
        "basis_data.calculations.csv": (basis_data_schemas.calculations_schema,),
        "basis_data.charge_price_information_periods.csv": (
            basis_data_schemas.charge_price_information_periods_schema,
        ),
        "basis_data.charge_link_periods.csv": (
            basis_data_schemas.charge_link_periods_schema,
        ),
        "basis_data.charge_price_points.csv": (
            basis_data_schemas.charge_price_points_schema,
        ),
        # results
        "wholesale_output.energy_results.csv": (energy_results_schema,),
        "wholesale_output.wholesale_results.csv": (wholesale_results_schema,),
        "wholesale_output.total_monthly_amounts.csv": (total_monthly_amounts_schema,),
    }

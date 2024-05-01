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
from features.utils.dataframes import create_energy_result_dataframe
from features.utils.dataframes.basis_data.basis_data_dataframes import (
    create_metering_point_periods,
    create_time_series_points,
)
from features.utils.dataframes.basis_data.calculations_dataframe import (
    create_calculations_dataframe,
)
from features.utils.readers import BasisDataTableReader, EnergyResultViewReader

from package.calculation.basis_data.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
    calculations_schema,
)
from package.calculation.output.schemas import energy_results_schema
from package.infrastructure.paths import (
    BASIS_DATA_DATABASE_NAME,
    OUTPUT_DATABASE_NAME,
)


def get_input_specifications() -> dict[str, tuple]:
    """
    Contains the specifications for view scenario inputs.
    The key is the name of the file to be read.
    The value is a tuple containing the schema, the name of the method that reads the data,
    the method that to corrects the dataframe types, and the database name.
    """
    return {
        "metering_point_periods.csv": (
            metering_point_period_schema,
            BasisDataTableReader.read_metering_point_periods,
            create_metering_point_periods,
            BASIS_DATA_DATABASE_NAME,
        ),
        "time_series_points.csv": (
            time_series_point_schema,
            BasisDataTableReader.read_time_series_points,
            create_time_series_points,
            BASIS_DATA_DATABASE_NAME,
        ),
        "energy_results.csv": (
            energy_results_schema,
            EnergyResultViewReader.read_energy_results,
            create_energy_result_dataframe,
            OUTPUT_DATABASE_NAME,
        ),
        "calculations.csv": (
            calculations_schema,
            BasisDataTableReader.read_calculations,
            create_calculations_dataframe,
            BASIS_DATA_DATABASE_NAME,
        ),
    }

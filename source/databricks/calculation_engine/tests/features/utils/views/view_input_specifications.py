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
from package.calculation.basis_data.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)


def get_input_specifications() -> dict[str, tuple]:
    """
    Contains the specifications for scenario inputs.
    """
    return {
        "metering_point_periods.csv": (metering_point_period_schema, None),
        "time_series_points.csv": (time_series_point_schema, None),
    }

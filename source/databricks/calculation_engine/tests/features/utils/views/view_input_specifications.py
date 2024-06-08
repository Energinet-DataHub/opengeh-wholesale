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
def get_input_specifications() -> dict[str, None]:
    return {
        # basis data
        "basis_data.metering_point_periods.csv": None,
        "basis_data.time_series_points.csv": None,
        "basis_data.calculations.csv": None,
        "basis_data.charge_price_information_periods.csv": None,
        "basis_data.charge_link_periods.csv": None,
        "basis_data.charge_price_points.csv": None,
        # results
        "wholesale_output.energy_results.csv": None,
        "wholesale_output.wholesale_results.csv": None,
        "wholesale_output.total_monthly_amounts.csv": None,
        "wholesale_output.monthly_amounts.csv": None,
    }

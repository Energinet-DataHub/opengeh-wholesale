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


class MeteringPointPeriodColname:
    calculation_id = "calculation_id"

    # Master data
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    grid_area = "grid_area"
    resolution = "resolution"
    settlement_method = "settlement_method"
    energy_supplier_id = "energy_supplier_id"
    balance_responsible_id = "balance_responsible_id"

    # Exchange
    from_grid_area = "from_grid_area"
    to_grid_area = "to_grid_area"

    # Period
    from_date = "from_date"
    to_date = "to_date"


class TimeSeriesColname:
    calculation_id = "calculation_id"
    metering_point_id = "metering_point_id"
    observation_time = "observation_time"
    quantity = "quantity"
    quality = "quality"

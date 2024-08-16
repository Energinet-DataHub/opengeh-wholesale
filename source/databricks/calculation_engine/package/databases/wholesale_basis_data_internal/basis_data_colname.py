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


class ChargePricePointsColname:
    """Column names for the charge price points storage model"""

    calculation_id = "calculation_id"
    charge_key = "charge_key"
    charge_code = "charge_code"
    charge_type = "charge_type"
    charge_owner_id = "charge_owner_id"
    charge_price = "charge_price"
    charge_time = "charge_time"


class ChargeLinkPeriodsColname:
    """Column names for the charge link periods storage model"""

    calculation_id = "calculation_id"
    charge_key = "charge_key"
    charge_code = "charge_code"
    charge_type = "charge_type"
    charge_owner_id = "charge_owner_id"
    metering_point_id = "metering_point_id"
    quantity = "quantity"
    from_date = "from_date"
    to_date = "to_date"


class GridLossMeteringPointsColName:
    """
    Column names for the grid loss metering points storage model.
    Be aware that two different delta tables exist with the same table name `grid_loss_metering_points`.
    """

    calculation_id = "calculation_id"
    metering_point_id = "metering_point_id"


class CalculationsColumnName:
    """Column names for the calculations storage model"""

    calculation_id = "calculation_id"
    calculation_type = "calculation_type"
    period_start = "period_start"
    period_end = "period_end"
    execution_time_start = "execution_time_start"
    created_by_user_id = "created_by_user_id"
    version = "version"
    is_internal_calculation = "is_internal_calculation"
    """True if the calculation is an internal calculation, False otherwise."""

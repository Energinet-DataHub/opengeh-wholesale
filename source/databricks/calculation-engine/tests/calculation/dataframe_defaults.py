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
from decimal import Decimal
from datetime import datetime
from typing import Optional
from package.codelists import (
    ChargeQuality,
    ChargeResolution,
    ChargeType,
    MeteringPointType,
    MeteringPointResolution,
    SettlementMethod,
)

const_charge_id = "001"
const_charge_type = ChargeType.SUBSCRIPTION
const_charge_owner = "001"


class DataframeDefaults:
    default_job_id: str = "1"
    default_result_id: str = "1"
    default_result_name: str = "1"
    default_result_path: str = "1"
    default_positive_grid_loss: Decimal = Decimal("1.234")
    default_negative_grid_loss: Decimal = Decimal("1.234")
    default_balance_responsible_id: str = "1"
    default_calculation_type: Optional[str] = None
    default_charge_id: str = const_charge_id
    default_charge_key: str = (
        f"{const_charge_id}-{const_charge_type}-{const_charge_owner}"
    )
    default_charge_owner: str = const_charge_owner
    default_charge_price: Decimal = Decimal("1.123456")
    default_charge_tax: bool = True
    default_charge_type: str = const_charge_type.value
    default_charge_resolution: str = ChargeResolution.DAY.value
    default_charge_time: datetime = datetime(2020, 1, 1, 0, 0)
    default_currency: str = "DDK"
    default_energy_supplier_id: str = "1"
    default_from_grid_area: str = "chargea"
    default_from_date: datetime = datetime(2020, 1, 1, 0, 0)
    default_grid_area: str = "500"
    default_metering_point_id: str = "some-metering-point-id"
    default_metering_point_type: str = MeteringPointType.CONSUMPTION.value
    default_parent_metering_point_id: str = "1"
    default_product: str = "chargea"
    default_quality: str = ChargeQuality.CALCULATED.value
    default_quantity: Decimal = Decimal("1.123")
    default_metering_point_resolution: str = MeteringPointResolution.HOUR.value
    default_settlement_method: str = SettlementMethod.FLEX.value
    default_sum_quantity: Decimal = Decimal("1.234")
    default_time_window_end: datetime = datetime(2020, 1, 1, 1, 0)
    default_time_window_start: datetime = datetime(2020, 1, 1, 0, 0)
    default_to_grid_area: str = "1"
    default_to_date: datetime = datetime(2020, 1, 2, 0, 0)
    default_unit: str = "chargea"

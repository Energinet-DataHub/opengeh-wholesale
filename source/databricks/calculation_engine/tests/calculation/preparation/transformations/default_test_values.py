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
from datetime import datetime
from decimal import Decimal

import package.codelists as e


class DefaultTestValues:
    DEFAULT_GRID_AREA = "543"
    DEFAULT_CHARGE_CODE = "4000"
    DEFAULT_CHARGE_OWNER = "001"
    DEFAULT_CHARGE_TAX = True
    DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
    DEFAULT_CHARGE_PRICE = Decimal("2.000005")
    DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
    DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
    DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
    DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX
    DEFAULT_QUANTITY = Decimal("1.005")
    DEFAULT_QUALITY = e.ChargeQuality.CALCULATED
    DEFAULT_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)
    DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
    DEFAULT_FROM_GRID_AREA = None
    DEFAULT_TO_GRID_AREA = None
    DEFAULT_PARENT_METERING_POINT_ID = None
    DEFAULT_CALCULATION_TYPE = None

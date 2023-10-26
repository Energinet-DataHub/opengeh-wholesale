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


from enum import Enum


class AmountType(Enum):
    """
    This type is used to distinguish wholesale result. There are three groups:

    AMOUNT_PER_CHARGE: fee/tariff/subscription per hour/day (later maybe also quarter) per charge key (code, type,
    and owner). The amounts are calculated per metering point type.

    MONTHLY_AMOUNT_PER_CHARGE: fee/tariff/subscription per month per charge key (code, type, and owner).
    The amounts are summed across metering point types.

    TOTAL_MONTHLY_AMOUNT: Total sum of amounts per month across charge codes and charge types
    """

    AMOUNT_PER_CHARGE = "amount_per_charge"
    MONTHLY_AMOUNT_PER_CHARGE = "monthly_amount_per_charge"
    TOTAL_MONTHLY_AMOUNT = "total_monthly_amount"

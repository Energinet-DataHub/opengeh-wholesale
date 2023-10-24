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


from .result_column_names import ResultColumnNames


class WholesaleResultColumnNames(ResultColumnNames):
    quantity_unit = "quantity_unit"
    resolution = "resolution"

    metering_point_type = "metering_point_type"
    settlement_method = "settlement_method"
    price = "price"
    amount = "amount"
    is_tax = "is_tax"

    charge_code = "charge_code"
    charge_type = "charge_type"
    charge_owner_id = "charge_owner_id"

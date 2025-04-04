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


from geh_wholesale.calculation.preparation.data_structures.prepared_tariffs import (
    PreparedTariffs,
)
from geh_wholesale.calculation.wholesale.calculate_total_quantity_and_amount import (
    calculate_total_quantity_and_amount,
)
from geh_wholesale.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)
from geh_wholesale.codelists import ChargeType


def calculate_tariff_price_per_co_es(
    prepared_tariffs: PreparedTariffs,
) -> WholesaleResults:
    """Calculate tariff amount time series.

    A result is calculated per
    - grid area
    - charge key (charge id, charge type, charge owner)
    - settlement method
    - metering point type (except exchange metering points)
    - energy supplier

    Resolution has already been filtered, so only one resolution is present
    in the tariffs data frame. So responsibility of creating results per
    resolution is managed outside this module.
    """
    df = calculate_total_quantity_and_amount(prepared_tariffs.df, ChargeType.TARIFF)
    return WholesaleResults(df)

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


class WholesaleBasisDataDatabase:
    DATABASE_NAME = "wholesale_basis_data"
    METERING_POINT_PERIODS_VIEW_NAME = "metering_point_periods_v1"
    TIME_SERIES_POINTS_VIEW_NAME = "time_series_points_v1"
    CHARGE_PRICE_POINTS_VIEW_NAME = "charge_price_points_v1"
    CHARGE_LINK_PERIODS_VIEW_NAME = "charge_link_periods_v1"
    CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME = "charge_price_information_periods_v1"


class WholesaleResultsDatabase:
    DATABASE_NAME = "wholesale_results"
    LATEST_CALCULATIONS_BY_DAY_VIEW_NAME = "latest_calculations_by_day_v1"
    ENERGY_V1_VIEW_NAME = "energy_v1"
    ENERGY_PER_ES_V1_VIEW_NAME = "energy_per_es_v1"
    AMOUNTS_PER_CHARGE_VIEW_NAME = "amounts_per_charge_v1"  # for some reason we call amounts per charge for wholesale results
    MONTHLY_AMOUNTS_PER_CHARGE_VIEW_NAME = "monthly_amounts_per_charge_v1"
    TOTAL_MONTHLY_AMOUNTS_VIEW_NAME = "total_monthly_amounts_v1"

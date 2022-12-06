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


def get_time_series_dataframe(time_series_df, metering_point_df, market_roles_df, es_brp_relations_df):
    metering_point_join_conditions = \
        [
            time_series_df.metering_point_id == metering_point_df.metering_point_id,
            time_series_df.time >= metering_point_df.from_date,
            time_series_df.time < metering_point_df.to_date
        ]

    time_series_with_metering_point = time_series_df \
        .join(metering_point_df, metering_point_join_conditions, "inner") \
        .drop(metering_point_df.metering_point_id) \
        .drop(metering_point_df.from_date) \
        .drop(metering_point_df.to_date)

    market_roles_join_conditions = \
        [
            time_series_with_metering_point.metering_point_id == market_roles_df.metering_point_id,
            time_series_with_metering_point.time >= market_roles_df.from_date,
            time_series_with_metering_point.time < market_roles_df.to_date
        ]

    time_series_with_metering_point_and_market_roles = time_series_with_metering_point \
        .join(market_roles_df, market_roles_join_conditions, "left") \
        .drop(market_roles_df.metering_point_id) \
        .drop(market_roles_df.from_date) \
        .drop(market_roles_df.to_date)

    es_brp_relations_join_conditions = \
        [
            time_series_with_metering_point_and_market_roles.energy_supplier_id == es_brp_relations_df.energy_supplier_id,
            time_series_with_metering_point_and_market_roles.grid_area == es_brp_relations_df.grid_area,
            time_series_with_metering_point_and_market_roles.metering_point_type == es_brp_relations_df.metering_point_type,
            time_series_with_metering_point_and_market_roles.time >= es_brp_relations_df.from_date,
            time_series_with_metering_point_and_market_roles.time < es_brp_relations_df.to_date,
            time_series_with_metering_point_and_market_roles.metering_point_type == es_brp_relations_df.metering_point_type
        ]

    time_series_with_metering_point_and_market_roles_and_brp = time_series_with_metering_point_and_market_roles \
        .join(es_brp_relations_df, es_brp_relations_join_conditions, "left") \
        .drop(es_brp_relations_df.energy_supplier_id) \
        .drop(es_brp_relations_df.grid_area) \
        .drop(es_brp_relations_df.from_date) \
        .drop(es_brp_relations_df.to_date) \
        .drop(es_brp_relations_df.metering_point_type)

    # Add charges for BRS-027
    # charges_with_prices_and_links = charges_df \
    #     .join(charge_prices_df, ["charge_key"], "left") \
    #     .filter((col("time") >= col("from_date"))) \
    #     .filter((col("time") <= col("to_date"))) \
    #     .join(charge_links_df, ["charge_key", "from_date", "to_date"], "inner")
    # charges_with_prices_and_links.show()

    # time_series_with_metering_point_and_charges = time_series_with_metering_point \
    #     .join(charges_with_prices_and_links, ["metering_point_id", "from_date", "to_date"], "inner")

    return time_series_with_metering_point_and_market_roles_and_brp

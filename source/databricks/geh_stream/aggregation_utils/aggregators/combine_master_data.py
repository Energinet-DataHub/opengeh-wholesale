# # Copyright 2020 Energinet DataHub A/S
# #
# # Licensed under the Apache License, Version 2.0 (the "License2");
# # you may not use this file except in compliance with the License.
# # You may obtain a copy of the License at
# #
# #     http://www.apache.org/licenses/LICENSE-2.0
# #
# # Unless required by applicable law or agreed to in writing, software
# # distributed under the License is distributed on an "AS IS" BASIS,
# # WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# # See the License for the specific language governing permissions and
# # limitations under the License.
from geh_stream.codelists import Colname, ResultKeyName
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when
from geh_stream.shared.data_classes import Metadata


metering_grid_area_domain_mrid_drop = "MeteringGridArea_Domain_mRID_drop"


def combine_added_system_correction_with_master_data(results: dict, metadata: Metadata) -> DataFrame:
    added_system_correction_df = results[ResultKeyName.added_system_correction]
    grid_loss_sys_cor_master_data_df = results[ResultKeyName.grid_loss_sys_cor_master_data]
    return combine_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df, Colname.added_system_correction, Colname.is_system_correction)


def combine_added_grid_loss_with_master_data(results: dict, metadata: Metadata) -> DataFrame:
    added_grid_loss_df = results[ResultKeyName.added_grid_loss]
    grid_loss_sys_cor_master_data_df = results[ResultKeyName.grid_loss_sys_cor_master_data]
    return combine_master_data(added_grid_loss_df, grid_loss_sys_cor_master_data_df, Colname.added_grid_loss, Colname.is_grid_loss)


def combine_master_data(timeseries_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame, quantity_column_name, mp_check):
    df = timeseries_df.withColumnRenamed(quantity_column_name, Colname.quantity)
    mddf = grid_loss_sys_cor_master_data_df.withColumnRenamed(Colname.grid_area, metering_grid_area_domain_mrid_drop)
    return df.join(
        mddf,
        when(
            col(Colname.to_date).isNotNull(),
            col(Colname.time_window_start) <= col(Colname.to_date),
        ).otherwise(True)
        & (col(Colname.time_window_start) >= col(Colname.from_date))
        & (
            col(Colname.to_date).isNull()
            | (col(Colname.time_window_end) <= col(Colname.to_date))
        )
        & (
            col(Colname.grid_area)
            == col(metering_grid_area_domain_mrid_drop)
        )
        & (col(mp_check)), "inner"
    ).select(
        df[Colname.grid_area],
        df[Colname.quantity],
        df[Colname.time_window],
        mddf[Colname.metering_point_id],
        mddf[Colname.from_date],
        mddf[Colname.to_date],
        df[Colname.resolution],
        df[Colname.energy_supplier_id],
        df[Colname.balance_responsible_id],
        df[Colname.in_grid_area],
        df[Colname.out_grid_area],
        df[Colname.metering_point_type],
        df[Colname.settlement_method],
        mddf[Colname.is_grid_loss],
        mddf[Colname.is_system_correction]
    )

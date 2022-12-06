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
from pyspark.sql.dataframe import DataFrame, Column
from pyspark.sql.functions import col, when, row_number, lead
from geh_stream.codelists import Colname
from pyspark.sql.window import Window
from typing import List


def period_mutations(target_dataframe: DataFrame, event_df: DataFrame, cols_to_change: List[str]):

    event_df = __rename_event_columns_to_update(event_df, cols_to_change)

    # Join event dataframe with target dataframe
    joined = target_dataframe \
        .join(event_df, [Colname.metering_point_id], "inner")

    # Periods that should not be updated
    df_periods_to_keep = __get_periods_to_keep(joined)

    # Periods that should be updated
    df_periods_to_update = __get_periods_to_update(joined)

    # Count periods to update to be able to select update logic to apply
    periods_to_update_count = df_periods_to_update.count()

    # Generic update function to find out if period to_date should be set to effective_date from event
    update_func_to_date = (
        when(
            (col(Colname.from_date) < col(Colname.effective_date))
            & (col(Colname.to_date) > col(Colname.effective_date)),
            col(Colname.effective_date))
        .otherwise(col(Colname.to_date)))

    # Use this logic to update multiple periods with event data
    if periods_to_update_count > 1:
        df_periods_to_update = __update_multiple_periods(df_periods_to_update, update_func_to_date)

    # Use this update logic if only one period should be updated and a new period should be added
    else:
        df_periods_to_update = __update_single_period(df_periods_to_update, update_func_to_date)

    # If period exists that have to_date equal to effective_date,
    # then it's held in new variable (this is added to the periods_to_keep to ensure that no data is updated)
    split_period = __split_out_first_period(df_periods_to_update, target_dataframe.columns)

    # Add split_periods to periods_to_keep
    df_periods_to_keep = df_periods_to_keep \
        .select(target_dataframe.columns) \
        .union(split_period)

    # Filter on periods that have to_date after effective_date
    df_periods_to_update = __filter_out_split_period(split_period, df_periods_to_update)

    # Update periods_to_update with event data
    df_periods_to_update = __update_column_values(cols_to_change, df_periods_to_update)

    # Union periods_to_keep and updated_periods
    result = __create_result(df_periods_to_keep, df_periods_to_update, target_dataframe.columns)

    return result


def __update_multiple_periods(periods: DataFrame, update_func: Column) -> DataFrame:
    # Window function to be used in row_number and lead functions
    windowSpec = Window \
        .partitionBy(Colname.metering_point_id) \
        .orderBy(Colname.to_date)

    # Set row_number on periods to update
    periods = periods \
        .withColumn("row_number", row_number().over(windowSpec))

    # Set to_date based on update_func_to_date logic
    periods = periods \
        .withColumn(Colname.to_date, update_func)

    # Add column next_from_date with next row's from_date
    periods = periods \
        .withColumn("next_from_date", lead(col(Colname.from_date), 1).over(windowSpec))

    # Select first period in new variable, used to add new period to master dataframe
    new_row = periods \
        .filter(col("row_number") == 1)

    # Set from_date and to_date on new_row
    new_row = new_row \
        .withColumn(Colname.from_date, col(Colname.to_date)) \
        .withColumn(Colname.to_date, col("next_from_date"))

    # Only add new_row if from_date and to_date are not equal
    if(new_row.filter(col(Colname.from_date) != col(Colname.to_date)).count() == 1):
        periods = periods \
            .union(new_row)

    # Drop column row_number as we don't need the column going forward
    periods = periods \
        .drop("row_number") \
        .orderBy(col(Colname.from_date))

    return periods


def __update_single_period(periods: DataFrame, update_func: Column) -> DataFrame:
    # Set to_date based on update_func_to_date logic and add new column to hold old_to_date
    periods = periods \
        .withColumn("old_to_date", col(Colname.to_date)) \
        .withColumn(Colname.to_date, update_func)
    # new_row to be added, updated with to_date equal to old_to_date
    new_row = periods \
        .withColumn(Colname.from_date, col(Colname.effective_date)) \
        .withColumn(Colname.to_date, col("old_to_date"))
    # Add new_row to existing periods_to_update
    periods = periods \
        .union(new_row)

    return periods


def __split_out_first_period(periods_to_split: DataFrame, columns_to_select: List[Column]) -> DataFrame:
    split_period = periods_to_split \
        .filter(col(Colname.to_date) == col(Colname.effective_date)) \
        .select(columns_to_select)

    return split_period


def __filter_out_split_period(split_period: DataFrame, updated_periods: DataFrame) -> DataFrame:
    if not split_period.rdd.isEmpty():
        updated_periods = updated_periods \
            .filter(col(Colname.to_date) > split_period.first()[Colname.to_date])

    return updated_periods


def __update_column_values(cols_to_change: List[str], updated_periods: DataFrame) -> DataFrame:
    for col_to_change in cols_to_change:
        updated_periods = updated_periods \
            .withColumn(col_to_change, col(f"updated_{col_to_change}"))

    return updated_periods


def __create_result(periods_to_keep: DataFrame, updated_periods: DataFrame, columns_to_select: List[Column]) -> DataFrame:
    # Select only columns that corresponds to columns in target_dataframe
    updated_periods = updated_periods \
        .select(columns_to_select)

    # Union periods_to_keep with updated periods
    result = periods_to_keep \
        .select(columns_to_select) \
        .union(updated_periods)

    return result


def __get_periods_to_keep(df: DataFrame) -> DataFrame:
    return df.filter(col(Colname.to_date) <= col(Colname.effective_date))


def __get_periods_to_update(df: DataFrame) -> DataFrame:
    return df \
        .filter(col(Colname.to_date) > col(Colname.effective_date)) \
        .orderBy(col(Colname.from_date))


def __rename_event_columns_to_update(event_df: DataFrame, cols_to_change: List[str]) -> DataFrame:
    # Update col names to update on event dataframe
    for col_to_change in cols_to_change:
        event_df = event_df \
            .withColumnRenamed(col_to_change, f"updated_{col_to_change}")

    return event_df

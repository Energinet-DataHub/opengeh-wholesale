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
from geh_stream.codelists import Colname
from geh_stream.shared.period import Period
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col
from typing import List


def filter_on_date(df: DataFrame, period: Period) -> DataFrame:
    return (df
            .filter(col(Colname.time) < period.to_date)
            .filter(col(Colname.time) >= period.from_date))


def filter_on_period(df: DataFrame, period: Period) -> DataFrame:
    return (df
            .filter(col(Colname.from_date) < period.to_date)
            .filter(col(Colname.to_date) > period.from_date))


def filter_on_grid_areas(df: DataFrame, grid_area_col: str, grid_areas: List[str]) -> DataFrame:
    if grid_areas is not None and len(grid_areas):
        return df.filter(col(grid_area_col).isin(grid_areas))
    return df


def time_series_points_where_date_condition(period: Period) -> str:
    from_condition = f"Year >= {period.from_date.year} AND Month >= {period.from_date.month} AND Day >= {period.from_date.day}"
    to_condition = f"Year <= {period.to_date.year} AND Month <= {period.to_date.month} AND Day <= {period.to_date.day}"
    return f"{from_condition} AND {to_condition}"

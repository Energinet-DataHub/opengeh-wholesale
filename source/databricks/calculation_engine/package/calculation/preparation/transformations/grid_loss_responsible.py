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
"""
By having a conftest.py in this directory, we are able to add all packages
defined in the geh_stream directory in our tests.
"""
import datetime

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col

from package.calculation.preparation.grid_loss_responsible import (
    GridLossResponsible,
    grid_area_responsible_schema,
)
from package.codelists import MeteringPointType
from package.constants import Colname
from package.calculation_input import TableReader

DEFAULT_FROM_TIME = "2000-01-01"


def _utc(date: str | None) -> datetime.datetime | None:
    """Helper to convert danish date string to UTC date time."""

    if date is None:
        return None

    danish = datetime.datetime.strptime(date, "%Y-%m-%d")
    utc = datetime.datetime.fromtimestamp(danish.timestamp(), tz=datetime.timezone.utc)
    return utc


# fmt: off
GRID_AREA_RESPONSIBLE = [
    # u-001 and t-001
    ('571313180480500149',),
    ('570715000000682292',),
    ('571313154313676325',),
    ('571313153313676335',),
    ('571313154391364862',),
    ('579900000000000026',),
    ('571313180300014979',),
    ('571313180400100657',),
    ('578030000000000012',),
    ('571313154312753911',),
    ('571313153308031507',),
    ('571313158410300060',),

    # test-001
    ("7428190000000000",),
    ("3905041000000000",),
    ("3166564000000000",),
    ("4334573000000000",),
    ("3705066000000000",),
    ("3792714000000000",),
    ("3662188000000000",),
    ("3489686000000000",),
    ("2838688000000000",),
    ("1398856000000000",),
    ("3073766000000000",),
    ("2406190000000000",),
    ("8437670000000000",),
    ("2674238000000000",),
    ("1754270000000000",),
    ("1270128000000000",),
    ("2745419000000000",),
    ("2176335000000000",),
    ("3538981000000000",),
    ("2668410000000000",),
    ("1590486000000000",),
    ("3850382000000000",),
    ("3178600000000000",),
    ("2925420000000000",),
    ("7069010000000000",),
    ("4210390000000000",),
    ("1206301000000000",),
    ("3870502000000000",),
    ("4081808000000000",),
    ("1561940000000000",),
    ("2634733000000000",),
    ("1323815000000000",),
    ("7473830000000000",),
    ("4031354000000000",),
    ("9197130000000000",),
    ("3861176000000000",),
    ("3564652000000000",),
    ("3644064000000000",),
    ("9694030000000000",),
    ("2897200000000000",),
    ("6555670000000000",),
    ("3307447000000000",),
    ("8585970000000000",),
    ("4170984000000000",),
    ("2504835000000000",),
    ("2959396000000000",),
    ("1285221000000000",),
    ("3022279000000000",),
    ("4299157000000000",),
    ("8539450000000000",),
    ("1837633000000000",),
    ("2152598000000000",),
    ("2277074000000000",),
    ("3406257000000000",),
    ("4009132000000000",),
    ("1578886000000000",),
    ("1005497000000000",),
    ("2792763000000000",),
    ("2730171000000000",),
    ("3788057000000000",),
    ("3047193000000000",),
    ("2349226000000000",),
    ("3630754000000000",),
    ("3406034000000000",),
    ("3518998000000000",),
    ("2961004000000000",),
    ("1730226000000000",),
    ("2729908000000000",),
    ("4202341000000000",),
    ("7696290000000000",),
    ("2432875000000000",),
    ("2586697000000000",),
    ("7925000000000000",),
    ("1199313000000000",),
    ("3554540000000000",),
    ("6240250000000000",),
    ("1390664000000000",),
    ("1147331000000000",),
    ("1445322000000000",),
    ("1914284000000000",),
    ("1219860000000000",),
    ("3697388000000000",),
    ("4030710000000000",),
    ("1315297000000000",),
    ("2004831000000000",),
    ("2525094000000000",),
    ("4531630000000000",),
    ("1814105000000000",),
    ("6649780000000000",),
    ("1660310000000000",),
    ("3474360000000000",),
    ("4251915000000000",),
    ("1294254000000000",),
    ("3891922000000000",),
    ("3498110000000000",),
    ("2317324000000000",),
    ("4297135000000000",),
    ("2834522000000000",),
    ("1207543000000000",),
    ("2071017000000000",),
    ("2119017000000000",),
    ("2920882000000000",),
    ("3318924000000000",),
    ("1581021000000000",),
    ("3678454000000000",),
    ("1797183000000000",),
]
# fmt: on


def _get_grid_loss_responsible(
    grid_areas: list[str],
    metering_point_periods_df: DataFrame,
    grid_loss_responsible_df: DataFrame,
) -> GridLossResponsible:
    grid_loss_responsible_df = grid_loss_responsible_df.join(
        metering_point_periods_df,
        Colname.metering_point_id,
        "inner",
    )

    grid_loss_responsible_df = grid_loss_responsible_df.select(
        col(Colname.metering_point_id),
        col(Colname.grid_area),
        col(Colname.from_date),
        col(Colname.to_date),
        col(Colname.metering_point_type),
        col(Colname.energy_supplier_id),
    )

    _throw_if_no_grid_loss_responsible(grid_areas, grid_loss_responsible_df)

    return GridLossResponsible(grid_loss_responsible_df)


def get_grid_loss_responsible(
    grid_areas: list[str], metering_point_periods_df: DataFrame
) -> GridLossResponsible:
    grid_loss_responsible_df = _get_all_grid_loss_responsible()
    return _get_grid_loss_responsible(
        grid_areas, metering_point_periods_df, grid_loss_responsible_df
    )


def read_grid_loss_responsible(
    grid_areas: list[str],
    metering_point_periods_df: DataFrame,
    table_reader: TableReader,
) -> GridLossResponsible:
    grid_loss_responsible_df = table_reader.read_grid_loss_responsible()
    return _get_grid_loss_responsible(
        grid_areas, metering_point_periods_df, grid_loss_responsible_df
    )


def _throw_if_no_grid_loss_responsible(
    grid_areas: list[str], grid_loss_responsible_df: DataFrame
) -> None:
    for grid_area in grid_areas:
        current_grid_area_responsible = grid_loss_responsible_df.filter(
            col(Colname.grid_area) == grid_area
        )
        if (
            current_grid_area_responsible.filter(
                col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value
            ).count()
            == 0
        ):
            raise ValueError(
                f"No responsible for negative grid loss found for grid area {grid_area}"
            )
        if (
            current_grid_area_responsible.filter(
                col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value
            ).count()
            == 0
        ):
            raise ValueError(
                f"No responsible for positive grid loss found for grid area {grid_area}"
            )


def _get_all_grid_loss_responsible() -> DataFrame:
    spark = SparkSession.builder.getOrCreate()
    return spark.createDataFrame(GRID_AREA_RESPONSIBLE, grid_area_responsible_schema)

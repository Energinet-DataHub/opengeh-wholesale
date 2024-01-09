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
    grid_loss_responsible_metering_point_schema,
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
    # dev-001 (Subsystem and Manual test) and test-001 (Manual test)
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

    # test-001 (Subsystem/Performance test)
    ("742819000000000000",),
    ("390504100000000000",),
    ("316656400000000000",),
    ("433457300000000000",),
    ("370506600000000000",),
    ("379271400000000000",),
    ("366218800000000000",),
    ("348968600000000000",),
    ("283868800000000000",),
    ("139885600000000000",),
    ("307376600000000000",),
    ("240619000000000000",),
    ("843767000000000000",),
    ("267423800000000000",),
    ("175427000000000000",),
    ("127012800000000000",),
    ("274541900000000000",),
    ("217633500000000000",),
    ("353898100000000000",),
    ("266841000000000000",),
    ("159048600000000000",),
    ("385038200000000000",),
    ("317860000000000000",),
    ("292542000000000000",),
    ("706901000000000000",),
    ("421039000000000000",),
    ("120630100000000000",),
    ("387050200000000000",),
    ("408180800000000000",),
    ("156194000000000000",),
    ("263473300000000000",),
    ("132381500000000000",),
    ("747383000000000000",),
    ("403135400000000000",),
    ("919713000000000000",),
    ("386117600000000000",),
    ("356465200000000000",),
    ("364406400000000000",),
    ("969403000000000000",),
    ("289720000000000000",),
    ("655567000000000000",),
    ("330744700000000000",),
    ("858597000000000000",),
    ("417098400000000000",),
    ("250483500000000000",),
    ("295939600000000000",),
    ("128522100000000000",),
    ("302227900000000000",),
    ("429915700000000000",),
    ("853945000000000000",),
    ("183763300000000000",),
    ("215259800000000000",),
    ("227707400000000000",),
    ("340625700000000000",),
    ("400913200000000000",),
    ("157888600000000000",),
    ("100549700000000000",),
    ("279276300000000000",),
    ("273017100000000000",),
    ("378805700000000000",),
    ("304719300000000000",),
    ("234922600000000000",),
    ("363075400000000000",),
    ("340603400000000000",),
    ("351899800000000000",),
    ("296100400000000000",),
    ("173022600000000000",),
    ("272990800000000000",),
    ("420234100000000000",),
    ("769629000000000000",),
    ("243287500000000000",),
    ("258669700000000000",),
    ("792500000000000000",),
    ("119931300000000000",),
    ("355454000000000000",),
    ("624025000000000000",),
    ("139066400000000000",),
    ("114733100000000000",),
    ("144532200000000000",),
    ("191428400000000000",),
    ("121986000000000000",),
    ("369738800000000000",),
    ("403071000000000000",),
    ("131529700000000000",),
    ("200483100000000000",),
    ("252509400000000000",),
    ("453163000000000000",),
    ("181410500000000000",),
    ("664978000000000000",),
    ("166031000000000000",),
    ("347436000000000000",),
    ("425191500000000000",),
    ("129425400000000000",),
    ("389192200000000000",),
    ("349811000000000000",),
    ("231732400000000000",),
    ("429713500000000000",),
    ("283452200000000000",),
    ("120754300000000000",),
    ("207101700000000000",),
    ("211901700000000000",),
    ("292088200000000000",),
    ("331892400000000000",),
    ("158102100000000000",),
    ("367845400000000000",),
    ("179718300000000000",),
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
    return spark.createDataFrame(
        GRID_AREA_RESPONSIBLE, grid_loss_responsible_metering_point_schema
    )

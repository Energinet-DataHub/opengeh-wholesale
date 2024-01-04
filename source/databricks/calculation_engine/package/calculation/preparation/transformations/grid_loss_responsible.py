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
import pyspark.sql.types as t

from package.calculation.preparation.grid_loss_responsible import (
    GridLossResponsible,
    grid_loss_responsible_schema,
)
from package.codelists import MeteringPointType
from package.constants import Colname

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
    # ('571313154312753911',),  # Use data from b-001
    # ('571313153308031507'),,  # Obs: differs from b-001
    ('571313158410300060',),

    # b-001 grid loss
    ("571313100300001601",),
    ("571313100300001618",),
    ("571313101700010521",),
    ("571313103103493154",),
    ("571313104200312683",),
    ("571313105100150436",),
    ("571313108400995004",),
    ("571313108500133856",),
    ("571313113161091759",),
    ("571313114184534117",),
    ("571313115101999996",),
    ("571313115200000012",),
    ("571313115410102339",),
    ("571313123200000017",),
    ("571313124488888885",),
    ("571313124501999994",),
    ("571313100300001625",),
    ("571313133100307383",),
    ("571313134100068311",),
    ("571313134200090014",),
    ("571313144501999992",),
    ("571313134706400003",),
    ("571313134809000018",),
    ("571313135100208363",),
    ("571313135799000019",),
    ("571313137000802658",),
    ("571313137100041391",),
    ("571313138100018789",),
    ("571313138400042385",),
    ("571313138500021990",),
    ("571313139600025994",),
    ("571313139800021383",),
    ("571313100300001632",),
    ("571313153100305318",),
    ("571313153200126097",),
    ("571313153308031507",),
    ("571313154312753911",),
    ("571313158410000052",),
    ("571313174000000011",),
    ("571313175599999908",),
    ("571313175711314138",),
    ("571313174115776740",),
    ("571313185300000069",),
    ("571313185400000051",),
    ("571313186000118597",),
    ("571313191100273671",),
    ("579209209200000371",),
    ("571313100300001663",),
    ("571313100300001731",),
    ("571313100300001748",),
    ("571313100300001755",),
    ("570715000001655677",),
    ("571313100300001700",),
    ("571313100300001724",),
    ("570715000001657169",),
    ("570715000001741271",),

    # b-001 system correction
    ("570715000001741851",),
    ("570715000001741868",),
    ("571313101700010514",),
    ("571313103190493426",),
    ("571313104210660743",),
    ("571313105100232941",),
    ("571313108405000796",),
    ("571313108500135263",),
    ("571313113162145215",),
    ("571313114184550322",),
    ("571313115101888887",),
    ("571313115200554232",),
    ("571313115410113908",),
    ("571313123300000016",),
    ("570715000001536402",),
    ("571313124501888885",),
    ("571313100400000771",),
    ("571313133122466594",),
    ("571313134190048934",),
    ("571313134290021271",),
    ("571313134499089393",),
    ("571313134706400010",),
    ("571313134890188879",),
    ("571313135100243715",),
    ("571313135799003744",),
    ("571313137090018878",),
    ("571313137190016804",),
    ("571313138190019239",),
    ("571313138490010417",),
    ("571313138590019006",),
    ("571313139690015967",),
    ("571313139890018454",),
    ("571313151260043491",),
    ("571313153190142510",),
    ("571313153299054097",),
    ("571313153398096295",),
    ("571313154390392460",),
    ("571313158490373084",),
    ("571313174001698224",),
    ("571313175500229216",),
    ("571313175711331463",),
    ("571313179100348995",),
    ("571313185301698210",),
    ("571313185430133880",),
    ("571313186000260685",),
    ("571313191191394194",),
    ("570715000001741875",),
    ("570715000001741882",),
    ("570715000001741899",),
    ("570715000001741264",),
]
# fmt: on


def get_grid_loss_responsible(
    grid_areas: list[str], metering_point_periods_df: DataFrame
) -> GridLossResponsible:
    grid_loss_responsible_df = _get_all_grid_loss_responsible()

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
    ).distinct()

    _throw_if_no_grid_loss_responsible(grid_areas, grid_loss_responsible_df)

    return GridLossResponsible(grid_loss_responsible_df)


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
    schema = t.StructType(
        [
            t.StructField(Colname.metering_point_id, t.StringType(), False),
        ]
    )
    spark = SparkSession.builder.getOrCreate()
    return spark.createDataFrame(GRID_AREA_RESPONSIBLE, schema)

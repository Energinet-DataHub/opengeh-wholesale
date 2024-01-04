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
    grid_loss_responsible_schema,
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
    ('571313180480500149', "804", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.PRODUCTION.value, '8100000000108'),
    ('570715000000682292', "512", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.PRODUCTION.value, '5790002437717'),
    ('571313154313676325', "543", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.PRODUCTION.value, '5790002437717'),
    ('571313153313676335', "533", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.PRODUCTION.value, '5790002437717'),
    ('571313154391364862', "584", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.PRODUCTION.value, '5790002437717'),
    ('579900000000000026', "990", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.PRODUCTION.value, '4260024590017'),
    ('571313180300014979', "803", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.PRODUCTION.value, '8100000000108'),
    ('571313180400100657', "804", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.CONSUMPTION.value, '8100000000115'),
    ('578030000000000012', "803", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.CONSUMPTION.value, '8100000000108'),
    # ('571313154312753911', "543", d(DEFAULT_FROM_TIME), None, MeteringPointType.CONSUMPTION.value, '5790001103095'),  # Use data from b-001
    # ('571313153308031507', "533", d(DEFAULT_FROM_TIME), None, MeteringPointType.CONSUMPTION.value, '5790001102357'),  # Obs: differs from b-001
    ('571313158410300060', "584", _utc(DEFAULT_FROM_TIME), None, MeteringPointType.CONSUMPTION.value, '5790001103095'),

    # b-001 grid loss
    ("571313100300001601", "003", _utc("2020-06-11"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001601", "003", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001618", "007", _utc("2020-02-13"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001618", "007", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313101700010521", "016", _utc("2021-01-01"), _utc("2021-06-09"), MeteringPointType.CONSUMPTION.value, "5790001103040"),
    ("571313101700010521", "016", _utc("2021-06-09"), _utc("2022-02-01"), MeteringPointType.CONSUMPTION.value, "5790001103040"),
    ("571313101700010521", "016", _utc("2022-02-01"), _utc("2022-10-25"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313101700010521", "016", _utc("2022-10-25"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313101700010521", "016", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313103103493154", "031", _utc("2021-01-01"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313103103493154", "031", _utc("2022-01-01"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313103103493154", "031", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313104200312683", "042", _utc("2020-12-03"), _utc("2022-06-03"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313104200312683", "042", _utc("2022-06-03"), _utc("2023-02-02"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313104200312683", "042", _utc("2023-02-02"), _utc("2023-02-28"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313104200312683", "042", _utc("2023-02-28"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313104200312683", "042", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313105100150436", "051", _utc("2021-01-19"), _utc("2021-03-30"), MeteringPointType.CONSUMPTION.value, "5790000703388"),
    ("571313105100150436", "051", _utc("2021-03-30"), _utc("2021-09-22"), MeteringPointType.CONSUMPTION.value, "5790000703388"),
    ("571313105100150436", "051", _utc("2021-09-22"), _utc("2022-02-01"), MeteringPointType.CONSUMPTION.value, "5790000703388"),
    ("571313105100150436", "051", _utc("2022-02-01"), _utc("2022-04-06"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313105100150436", "051", _utc("2022-04-06"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313105100150436", "051", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313108400995004", "084", _utc("2020-11-24"), _utc("2021-02-11"), MeteringPointType.CONSUMPTION.value, "5790002388309"),
    ("571313108400995004", "084", _utc("2021-02-11"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790002388309"),
    ("571313108400995004", "084", _utc("2022-01-01"), _utc("2022-02-01"), MeteringPointType.CONSUMPTION.value, "5790002388309"),
    ("571313108400995004", "084", _utc("2022-02-01"), _utc("2022-11-15"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313108400995004", "084", _utc("2022-11-15"), _utc("2022-11-23"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313108400995004", "084", _utc("2022-11-23"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313108400995004", "084", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313108500133856", "085", _utc("2021-01-18"), _utc("2021-03-23"), MeteringPointType.CONSUMPTION.value, "5790000703388"),
    ("571313108500133856", "085", _utc("2021-03-23"), _utc("2023-03-08"), MeteringPointType.CONSUMPTION.value, "5790000703388"),
    ("571313108500133856", "085", _utc("2023-03-08"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313108500133856", "085", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313113161091759", "131", _utc("2020-12-09"), _utc("2021-02-03"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313113161091759", "131", _utc("2021-02-03"), _utc("2021-11-25"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313113161091759", "131", _utc("2021-11-25"), _utc("2023-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313113161091759", "131", _utc("2023-01-01"), _utc("2023-04-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313113161091759", "131", _utc("2023-04-01"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790002477751"),
    ("571313113161091759", "131", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790002477751"),
    ("571313114184534117", "141", _utc("2020-11-24"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001103040"),
    ("571313114184534117", "141", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001103040"),
    ("571313115101999996", "151", _utc("2021-02-16"), _utc("2021-03-28"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2021-03-28"), _utc("2021-11-23"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2021-11-23"), _utc("2021-11-24"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2021-11-24"), _utc("2021-12-10"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2021-12-10"), _utc("2022-04-05"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2022-04-05"), _utc("2022-04-06"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2022-04-06"), _utc("2022-04-28"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2022-04-28"), _utc("2022-06-17"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2022-06-17"), _utc("2022-06-30"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2022-06-30"), _utc("2022-09-15"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2022-09-15"), _utc("2022-09-26"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2022-09-26"), _utc("2023-01-19"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2023-01-19"), _utc("2023-04-21"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2023-04-21"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115101999996", "151", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313115200000012", "152", _utc("2020-12-09"), _utc("2021-02-03"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313115200000012", "152", _utc("2021-02-03"), _utc("2022-03-24"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313115200000012", "152", _utc("2022-03-24"), _utc("2022-09-29"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313115200000012", "152", _utc("2022-09-29"), _utc("2023-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313115200000012", "152", _utc("2023-01-01"), _utc("2023-03-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313115410102339", "154", _utc("2020-12-22"), _utc("2021-12-01"), MeteringPointType.CONSUMPTION.value, "5790002134302"),
    ("571313115410102339", "154", _utc("2021-12-01"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790001103040"),
    ("571313115410102339", "154", _utc("2022-01-01"), _utc("2022-03-03"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313115410102339", "154", _utc("2022-03-03"), _utc("2022-06-08"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313115410102339", "154", _utc("2022-06-08"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313115410102339", "154", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313123200000017", "233", _utc("2021-01-01"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313123200000017", "233", _utc("2022-01-01"), _utc("2022-02-08"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313123200000017", "233", _utc("2022-02-08"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313123200000017", "233", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313124488888885", "244", _utc("2021-01-01"), _utc("2021-11-30"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313124488888885", "244", _utc("2021-11-30"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313124488888885", "244", _utc("2022-01-01"), _utc("2022-02-08"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313124488888885", "244", _utc("2022-02-08"), _utc("2022-05-19"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313124488888885", "244", _utc("2022-05-19"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313124488888885", "244", _utc("2023-05-01"), _utc("2023-05-24"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313124488888885", "244", _utc("2023-05-24"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313124501999994", "245", _utc("2020-03-31"), _utc("2021-11-23"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2021-11-23"), _utc("2021-11-24"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2021-11-24"), _utc("2021-12-10"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2021-12-10"), _utc("2021-12-13"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2021-12-13"), _utc("2022-02-27"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-02-27"), _utc("2022-03-19"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-03-19"), _utc("2022-04-05"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-04-05"), _utc("2022-04-06"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-04-06"), _utc("2022-04-07"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-04-07"), _utc("2022-04-28"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-04-28"), _utc("2022-06-17"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-06-17"), _utc("2022-06-30"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-06-30"), _utc("2022-09-15"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-09-15"), _utc("2022-09-26"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2022-09-26"), _utc("2023-01-19"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2023-01-19"), _utc("2023-04-21"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2023-04-21"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313124501999994", "245", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001687137"),
    ("571313100300001625", "312", _utc("2020-04-29"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313100300001625", "312", _utc("2022-01-01"), _utc("2022-01-04"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313100300001625", "312", _utc("2022-01-04"), _utc("2023-01-09"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313100300001625", "312", _utc("2023-01-09"), _utc("2023-01-16"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313100300001625", "312", _utc("2023-01-16"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313100300001625", "312", _utc("2023-05-01"), _utc("2023-05-26"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313100300001625", "312", _utc("2023-05-26"), _utc("2023-07-14"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313100300001625", "312", _utc("2023-07-14"), None, MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313133100307383", "331", _utc("2020-12-03"), _utc("2022-06-03"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313133100307383", "331", _utc("2022-06-03"), _utc("2023-02-02"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313133100307383", "331", _utc("2023-02-02"), _utc("2023-02-28"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313133100307383", "331", _utc("2023-02-28"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313133100307383", "331", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134100068311", "341", _utc("2020-12-10"), _utc("2021-03-25"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134100068311", "341", _utc("2021-03-25"), _utc("2022-01-17"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134100068311", "341", _utc("2022-01-17"), _utc("2022-06-14"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134100068311", "341", _utc("2022-06-14"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134100068311", "341", _utc("2023-05-01"), _utc("2023-05-03"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134100068311", "341", _utc("2023-05-03"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134200090014", "342", _utc("2021-01-06"), _utc("2021-03-10"), MeteringPointType.CONSUMPTION.value, "5706552000028"),
    ("571313134200090014", "342", _utc("2021-03-10"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5706552000028"),
    ("571313134200090014", "342", _utc("2022-01-01"), _utc("2022-05-01"), MeteringPointType.CONSUMPTION.value, "5790001330385"),
    ("571313134200090014", "342", _utc("2022-05-01"), _utc("2022-06-18"), MeteringPointType.CONSUMPTION.value, "5790001330385"),
    ("571313134200090014", "342", _utc("2022-06-18"), _utc("2022-08-11"), MeteringPointType.CONSUMPTION.value, "5790001330385"),
    ("571313134200090014", "342", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001330385"),
    ("571313134200090014", "342", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001330385"),
    ("571313144501999992", "344", _utc("2020-12-19"), _utc("2022-05-04"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313144501999992", "344", _utc("2022-05-04"), _utc("2023-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313144501999992", "344", _utc("2023-01-01"), _utc("2023-04-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313144501999992", "344", _utc("2023-04-01"), _utc("2023-04-18"), MeteringPointType.CONSUMPTION.value, "5790002477751"),
    ("571313144501999992", "344", _utc("2023-04-18"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790002477751"),
    ("571313144501999992", "344", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790002477751"),
    ("571313134706400003", "347", _utc("2021-01-12"), _utc("2021-02-04"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313134706400003", "347", _utc("2021-02-04"), _utc("2021-02-05"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313134706400003", "347", _utc("2021-02-05"), _utc("2021-02-06"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313134706400003", "347", _utc("2021-02-06"), _utc("2023-01-01"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313134706400003", "347", _utc("2023-01-01"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313134706400003", "347", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313134809000018", "348", _utc("2020-04-30"), _utc("2022-05-03"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134809000018", "348", _utc("2022-05-03"), _utc("2022-05-10"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134809000018", "348", _utc("2022-05-10"), _utc("2022-07-27"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134809000018", "348", _utc("2022-07-27"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134809000018", "348", _utc("2023-05-01"), _utc("2023-05-03"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313134809000018", "348", _utc("2023-05-03"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313135100208363", "351", _utc("2020-12-30"), _utc("2022-12-08"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313135100208363", "351", _utc("2022-12-08"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313135100208363", "351", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313135799000019", "357", _utc("2021-01-01"), _utc("2021-04-20"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313135799000019", "357", _utc("2021-04-20"), _utc("2022-02-21"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313135799000019", "357", _utc("2022-02-21"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313135799000019", "357", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313137000802658", "370", _utc("2021-01-21"), _utc("2022-12-27"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313137000802658", "370", _utc("2022-12-27"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313137000802658", "370", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313137100041391", "371", _utc("2021-01-28"), _utc("2021-02-04"), MeteringPointType.CONSUMPTION.value, "5790002529283"),
    ("571313137100041391", "371", _utc("2021-02-04"), _utc("2021-03-19"), MeteringPointType.CONSUMPTION.value, "5790002529283"),
    ("571313137100041391", "371", _utc("2021-03-19"), _utc("2022-02-01"), MeteringPointType.CONSUMPTION.value, "5790002529283"),
    ("571313137100041391", "371", _utc("2022-02-01"), _utc("2023-03-24"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313137100041391", "371", _utc("2023-03-24"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313137100041391", "371", _utc("2023-05-01"), _utc("2023-07-12"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313137100041391", "371", _utc("2023-07-12"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313138100018789", "381", _utc("2021-01-20"), _utc("2022-03-11"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138100018789", "381", _utc("2022-03-11"), _utc("2022-04-23"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138100018789", "381", _utc("2022-04-23"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138100018789", "381", _utc("2023-05-01"), _utc("2023-05-02"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138100018789", "381", _utc("2023-05-02"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138400042385", "384", _utc("2021-01-01"), _utc("2021-04-19"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138400042385", "384", _utc("2021-04-19"), _utc("2022-01-14"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138400042385", "384", _utc("2022-01-14"), _utc("2022-12-23"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138400042385", "384", _utc("2022-12-23"), _utc("2023-01-12"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138400042385", "384", _utc("2023-01-12"), _utc("2023-04-20"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138400042385", "384", _utc("2023-04-20"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138400042385", "384", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138500021990", "385", _utc("2021-01-01"), _utc("2021-04-22"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138500021990", "385", _utc("2021-04-22"), _utc("2022-12-20"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138500021990", "385", _utc("2022-12-20"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313138500021990", "385", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313139600025994", "396", _utc("2021-01-18"), _utc("2021-03-30"), MeteringPointType.CONSUMPTION.value, "5790002243196"),
    ("571313139600025994", "396", _utc("2021-03-30"), _utc("2022-12-30"), MeteringPointType.CONSUMPTION.value, "5790002243196"),
    ("571313139600025994", "396", _utc("2022-12-30"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790002243196"),
    ("571313139600025994", "396", _utc("2023-05-01"), _utc("2023-09-11"), MeteringPointType.CONSUMPTION.value, "5790002243196"),
    ("571313139600025994", "396", _utc("2023-09-11"), None, MeteringPointType.CONSUMPTION.value, "5790002243196"),
    ("571313139800021383", "398", _utc("2020-12-10"), _utc("2021-03-10"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313139800021383", "398", _utc("2021-03-10"), _utc("2022-04-07"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313139800021383", "398", _utc("2022-04-07"), _utc("2022-12-22"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313139800021383", "398", _utc("2022-12-22"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313139800021383", "398", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313100300001632", "512", _utc("2020-12-18"), _utc("2021-05-27"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313100300001632", "512", _utc("2021-05-27"), _utc("2021-07-10"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313100300001632", "512", _utc("2021-07-10"), _utc("2022-03-01"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313153100305318", "531", _utc("2021-01-01"), _utc("2023-03-05"), MeteringPointType.CONSUMPTION.value, "5790000609185"),
    ("571313153100305318", "531", _utc("2023-03-05"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000609185"),
    ("571313153100305318", "531", _utc("2023-05-01"), _utc("2023-07-20"), MeteringPointType.CONSUMPTION.value, "5790000609185"),
    ("571313153100305318", "531", _utc("2023-07-20"), None, MeteringPointType.CONSUMPTION.value, "5790000609185"),
    ("571313153200126097", "532", _utc("2020-11-24"), _utc("2021-11-18"), MeteringPointType.CONSUMPTION.value, "5790001660239"),
    ("571313153200126097", "532", _utc("2021-11-18"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790001660239"),
    ("571313153200126097", "532", _utc("2022-01-01"), _utc("2022-02-08"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153200126097", "532", _utc("2022-02-08"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153200126097", "532", _utc("2023-05-01"), _utc("2023-06-12"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153200126097", "532", _utc("2023-06-12"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153308031507", "533", _utc("2021-01-01"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313153308031507", "533", _utc("2022-01-01"), _utc("2022-02-08"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153308031507", "533", _utc("2022-02-08"), _utc("2022-08-19"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153308031507", "533", _utc("2022-08-19"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153308031507", "533", _utc("2023-05-01"), _utc("2023-05-24"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153308031507", "533", _utc("2023-05-24"), _utc("2023-07-18"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313153308031507", "533", _utc("2023-07-18"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313154312753911", "543", _utc("2020-12-01"), _utc("2021-03-30"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313154312753911", "543", _utc("2021-03-30"), _utc("2022-02-17"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313154312753911", "543", _utc("2022-02-17"), _utc("2022-04-06"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313154312753911", "543", _utc("2022-04-06"), _utc("2023-03-29"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313154312753911", "543", _utc("2023-03-29"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313154312753911", "543", _utc("2023-05-01"), _utc("2023-08-04"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313154312753911", "543", _utc("2023-08-04"), None, MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313158410000052", "584", _utc("2020-12-04"), _utc("2021-03-30"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313158410000052", "584", _utc("2021-03-30"), _utc("2022-09-26"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313158410000052", "584", _utc("2022-09-26"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313158410000052", "584", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001103095"),
    ("571313174000000011", "740", _utc("2020-10-13"), _utc("2021-06-11"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313174000000011", "740", _utc("2021-06-11"), _utc("2021-06-16"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313174000000011", "740", _utc("2021-06-16"), _utc("2021-09-10"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313174000000011", "740", _utc("2021-09-10"), _utc("2023-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313174000000011", "740", _utc("2023-01-01"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313174000000011", "740", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313175599999908", "755", _utc("2020-12-09"), _utc("2021-02-09"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313175599999908", "755", _utc("2021-02-09"), _utc("2021-02-10"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313175599999908", "755", _utc("2021-02-10"), _utc("2021-02-27"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313175599999908", "755", _utc("2021-02-27"), _utc("2021-02-28"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313175599999908", "755", _utc("2021-02-28"), _utc("2022-03-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313175711314138", "757", _utc("2020-12-07"), _utc("2022-02-03"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313175711314138", "757", _utc("2022-02-03"), _utc("2022-05-05"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313175711314138", "757", _utc("2022-05-05"), _utc("2023-01-03"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313175711314138", "757", _utc("2023-01-03"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313175711314138", "757", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701278"),
    ("571313174115776740", "791", _utc("2021-01-04"), _utc("2021-09-10"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313174115776740", "791", _utc("2021-09-10"), _utc("2021-12-08"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313174115776740", "791", _utc("2021-12-08"), _utc("2023-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313174115776740", "791", _utc("2023-01-01"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313174115776740", "791", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313185300000069", "853", _utc("2020-11-13"), _utc("2021-02-04"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313185300000069", "853", _utc("2021-02-04"), _utc("2021-02-05"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313185300000069", "853", _utc("2021-02-05"), _utc("2022-09-20"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313185300000069", "853", _utc("2022-09-20"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313185300000069", "853", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790002416927"),
    ("571313185400000051", "854", _utc("2020-09-11"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790000701414"),
    ("571313185400000051", "854", _utc("2022-01-01"), _utc("2022-01-02"), MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313185400000051", "854", _utc("2022-01-02"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313185400000051", "854", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313186000118597", "860", _utc("2020-12-04"), _utc("2021-02-05"), MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313186000118597", "860", _utc("2021-02-05"), _utc("2021-10-19"), MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313186000118597", "860", _utc("2021-10-19"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313186000118597", "860", _utc("2023-05-01"), _utc("2023-05-25"), MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313186000118597", "860", _utc("2023-05-25"), None, MeteringPointType.CONSUMPTION.value, "5790001089382"),
    ("571313191100273671", "911", _utc("2021-01-01"), _utc("2021-05-11"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313191100273671", "911", _utc("2021-05-11"), _utc("2021-12-08"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313191100273671", "911", _utc("2021-12-08"), _utc("2022-01-01"), MeteringPointType.CONSUMPTION.value, "5790001095390"),
    ("571313191100273671", "911", _utc("2022-01-01"), _utc("2022-01-02"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313191100273671", "911", _utc("2022-01-02"), _utc("2022-02-08"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313191100273671", "911", _utc("2022-02-08"), _utc("2022-03-04"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313191100273671", "911", _utc("2022-03-04"), _utc("2022-09-22"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313191100273671", "911", _utc("2022-09-22"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("571313191100273671", "911", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790001102890"),
    ("579209209200000371", "920", _utc("2022-05-06"), _utc("2022-08-10"), MeteringPointType.CONSUMPTION.value, "5799994000213"),
    ("571313100300001663", "950", _utc("2020-02-13"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001663", "950", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001731", "951", _utc("2020-02-13"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001731", "951", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001748", "952", _utc("2020-02-13"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001748", "952", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001755", "953", _utc("2020-02-13"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001755", "953", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("570715000001655677", "954", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001700", "960", _utc("2020-02-13"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001700", "960", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001724", "962", _utc("2020-02-13"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("571313100300001724", "962", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("570715000001657169", "963", _utc("2020-02-05"), _utc("2022-09-01"), MeteringPointType.CONSUMPTION.value, "5790000701964"),
    ("570715000001741271", "990", _utc("2021-01-01"), _utc("2023-05-01"), MeteringPointType.CONSUMPTION.value, "4260024590017"),
    ("570715000001741271", "990", _utc("2023-05-01"), None, MeteringPointType.CONSUMPTION.value, "4260024590017"),

    # b-001 system correction
    ("570715000001741851", "003", _utc("2021-02-22"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741851", "003", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741868", "007", _utc("2021-02-22"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741868", "007", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313101700010514", "016", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313101700010514", "016", _utc("2023-05-01"), _utc("2023-08-17"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313101700010514", "016", _utc("2023-08-17"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313103190493426", "031", _utc("2020-08-04"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313103190493426", "031", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313103190493426", "031", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313104210660743", "042", _utc("2020-11-17"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313104210660743", "042", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313104210660743", "042", _utc("2023-05-01"), _utc("2023-05-02"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313104210660743", "042", _utc("2023-05-02"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313105100232941", "051", _utc("2020-10-29"), _utc("2021-01-10"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313105100232941", "051", _utc("2021-01-10"), _utc("2021-09-22"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313105100232941", "051", _utc("2021-09-22"), _utc("2021-10-05"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313105100232941", "051", _utc("2021-10-05"), _utc("2021-10-27"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313105100232941", "051", _utc("2021-10-27"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313105100232941", "051", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313105100232941", "051", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108405000796", "084", _utc("2020-12-10"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108405000796", "084", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108405000796", "084", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108500135263", "085", _utc("2020-11-22"), _utc("2021-12-19"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108500135263", "085", _utc("2021-12-19"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108500135263", "085", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108500135263", "085", _utc("2023-05-01"), _utc("2023-05-03"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313108500135263", "085", _utc("2023-05-03"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313113162145215", "131", _utc("2020-08-25"), _utc("2021-09-30"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313113162145215", "131", _utc("2021-09-30"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313113162145215", "131", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313113162145215", "131", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313114184550322", "141", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313114184550322", "141", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313115101888887", "151", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313115101888887", "151", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313115200554232", "152", _utc("2020-06-29"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313115200554232", "152", _utc("2022-08-11"), _utc("2023-03-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313115410113908", "154", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313115410113908", "154", _utc("2023-05-01"), _utc("2023-05-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313115410113908", "154", _utc("2023-05-11"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313123300000016", "233", _utc("2020-02-19"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313123300000016", "233", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313123300000016", "233", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001536402", "244", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001536402", "244", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313124501888885", "245", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313124501888885", "245", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313100400000771", "312", _utc("2021-02-01"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313100400000771", "312", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313133122466594", "331", _utc("2020-11-17"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313133122466594", "331", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313133122466594", "331", _utc("2023-05-01"), _utc("2023-05-02"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313133122466594", "331", _utc("2023-05-02"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134190048934", "341", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134190048934", "341", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134290021271", "342", _utc("2022-06-20"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134290021271", "342", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134290021271", "342", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134499089393", "344", _utc("2020-12-21"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134499089393", "344", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134499089393", "344", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134706400010", "347", _utc("2021-01-07"), _utc("2022-01-04"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134706400010", "347", _utc("2022-01-04"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134706400010", "347", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134706400010", "347", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134890188879", "348", _utc("2020-05-18"), _utc("2021-05-27"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134890188879", "348", _utc("2021-05-27"), _utc("2021-07-15"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134890188879", "348", _utc("2021-07-15"), _utc("2021-07-28"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134890188879", "348", _utc("2021-07-28"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134890188879", "348", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134890188879", "348", _utc("2023-05-01"), _utc("2023-05-25"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313134890188879", "348", _utc("2023-05-25"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313135100243715", "351", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313135100243715", "351", _utc("2023-05-01"), _utc("2023-05-08"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313135100243715", "351", _utc("2023-05-08"), _utc("2023-05-09"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313135100243715", "351", _utc("2023-05-09"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313135799003744", "357", _utc("2022-02-22"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313135799003744", "357", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313135799003744", "357", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313137090018878", "370", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313137090018878", "370", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313137190016804", "371", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313137190016804", "371", _utc("2023-05-01"), _utc("2023-05-02"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313137190016804", "371", _utc("2023-05-02"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313138190019239", "381", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313138190019239", "381", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313138490010417", "384", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313138490010417", "384", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313138590019006", "385", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313138590019006", "385", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313139690015967", "396", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313139690015967", "396", _utc("2023-05-01"), _utc("2023-08-14"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313139690015967", "396", _utc("2023-08-14"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313139890018454", "398", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313139890018454", "398", _utc("2023-05-01"), _utc("2023-06-05"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313139890018454", "398", _utc("2023-06-05"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313151260043491", "512", _utc("2021-02-23"), _utc("2022-02-03"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313151260043491", "512", _utc("2022-02-03"), _utc("2022-03-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153190142510", "531", _utc("2020-12-22"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153190142510", "531", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153190142510", "531", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153299054097", "532", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153299054097", "532", _utc("2023-05-01"), _utc("2023-05-02"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153299054097", "532", _utc("2023-05-02"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153398096295", "533", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313153398096295", "533", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2020-03-19"), _utc("2020-04-21"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2021-01-27"), _utc("2021-02-03"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2021-02-03"), _utc("2022-02-03"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2022-02-03"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2023-05-01"), _utc("2023-06-22"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2023-06-22"), _utc("2023-06-27"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313154390392460", "543", _utc("2023-06-27"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313158490373084", "584", _utc("2020-12-04"), _utc("2022-02-03"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313158490373084", "584", _utc("2022-02-03"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313158490373084", "584", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313158490373084", "584", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313174001698224", "740", _utc("2020-02-28"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313174001698224", "740", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313174001698224", "740", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175500229216", "755", _utc("2020-03-27"), _utc("2022-02-14"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175500229216", "755", _utc("2022-02-14"), _utc("2022-03-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175711331463", "757", _utc("2020-12-11"), _utc("2021-12-15"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175711331463", "757", _utc("2021-12-15"), _utc("2022-02-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175711331463", "757", _utc("2022-02-01"), _utc("2022-04-12"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175711331463", "757", _utc("2022-04-12"), _utc("2022-05-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175711331463", "757", _utc("2022-05-11"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175711331463", "757", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313175711331463", "757", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313179100348995", "791", _utc("2020-11-19"), _utc("2021-02-18"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313179100348995", "791", _utc("2021-02-18"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313179100348995", "791", _utc("2022-08-11"), _utc("2023-03-16"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313179100348995", "791", _utc("2023-03-16"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313179100348995", "791", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313185301698210", "853", _utc("2020-02-28"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313185301698210", "853", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313185301698210", "853", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313185430133880", "854", _utc("2020-06-24"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313185430133880", "854", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313185430133880", "854", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313186000260685", "860", _utc("2020-11-30"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313186000260685", "860", _utc("2022-08-11"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313186000260685", "860", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313191191394194", "911", _utc("2020-12-22"), _utc("2022-08-11"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313191191394194", "911", _utc("2022-08-11"), _utc("2023-04-14"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313191191394194", "911", _utc("2023-04-14"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("571313191191394194", "911", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741875", "950", _utc("2021-02-23"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741875", "950", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741882", "951", _utc("2021-02-23"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741882", "951", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741899", "960", _utc("2021-02-23"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741899", "960", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "5790002437717"),
    ("570715000001741264", "990", _utc("2021-01-01"), _utc("2023-05-01"), MeteringPointType.PRODUCTION.value, "4260024590017"),
    ("570715000001741264", "990", _utc("2023-05-01"), None, MeteringPointType.PRODUCTION.value, "4260024590017"),
]
# fmt: on


def _get_grid_loss_responsible(
    grid_areas: list[str],
    metering_point_periods_df: DataFrame,
    grid_loss_responsible_df: DataFrame,
) -> GridLossResponsible:
    grid_loss_responsible_df = grid_loss_responsible_df.select(
        Colname.metering_point_id
    )
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
    return spark.createDataFrame(GRID_AREA_RESPONSIBLE, grid_loss_responsible_schema)

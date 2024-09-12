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
import itertools

from pyspark.sql import Column
import pyspark.sql.functions as F


def map_from_dict(d: dict) -> Column:
    """Converts a dictionary to a Spark map column
    Args:
        d (dict): Dictionary to convert to a Spark map column
    Returns:
        Column: Spark map column
    """
    return F.create_map([F.lit(x) for x in itertools.chain(*d.items())])

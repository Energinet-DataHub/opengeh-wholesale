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

from inspect import stack
from pyspark.sql import DataFrame
from typing import Optional

loglevel = "information"


def _log(level: str, message: str, df: Optional[DataFrame]) -> None:
    print(f"============ {level} ============")
    # Frame 2 because: 1 is the calling function in this module, 2 is the caller of the functions in the module
    frame = stack()[2]
    print(f"{frame.filename}:{frame.lineno} - {frame.function}(): {message}")
    if df is not None:
        df.printSchema()
        df.show(5000, False)
        print(f"Number of rows in data frame: {df.count()}")
    print("===========================")


def log(message: str, df: Optional[DataFrame] = None) -> None:
    _log("LOG", message, df)


def debug(message: str, df: Optional[DataFrame] = None) -> None:
    if loglevel != "debug":
        return
    _log("DEBUG", message, df)

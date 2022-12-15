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

import pytest
import glob
import os
from pathlib import Path
from pyspark.sql.types import (
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    LongType,
)
from pyspark.sql.functions import col
from typing import Callable
from pyspark.sql import SparkSession


@pytest.fixture(scope="session")
def json_lines_reader() -> Callable[[str], str]:
    def f(path: str) -> str:
        return Path(path).read_text()

    return f


@pytest.fixture(scope="session")
def find_first_file() -> Callable[[str, str], str]:
    "The path of the first file matching the pattern."

    def f(path: str, pattern: str) -> str:
        os.chdir(path)
        for filePath in glob.glob(pattern):
            return filePath
        raise Exception("Target test file not found.")

    return f

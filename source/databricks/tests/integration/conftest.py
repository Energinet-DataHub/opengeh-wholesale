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
import os
from pyspark.sql import SparkSession


@pytest.fixture(scope="session")
def spark() -> SparkSession:
    return (SparkSession
            .builder
            .config("spark.sql.streaming.schemaInference", True)
            .getOrCreate())


@pytest.fixture(scope="session")
def integration_tests_path() -> str:
    """
    Returns the integration tests folder path.
    Please note that this only works if current folder haven't been changed prior using us.chdir().
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the integration tests folder.
    """
    return os.path.dirname(os.path.realpath(__file__))


@pytest.fixture(scope="session")
def delta_lake_path(integration_tests_path) -> str:
    return f"{integration_tests_path}/__delta__"


@pytest.fixture(scope="session")
def databricks_path() -> str:
    """
    Returns the source/databricks folder path.
    Please note that this only works if current folder haven't been changed prior using us.chdir().
    The correctness also relies on the prerequisite that this function is actually located in a
    file located directly in the integration tests folder.
    """
    return os.path.dirname(os.path.realpath(__file__)) + "/../.."


@pytest.fixture(scope="session")
def delta_reader(spark: SparkSession, delta_lake_path: str):
    def f(path: str):
        data = spark.sparkContext.emptyRDD()
        try:
            data = spark.read.format("delta").load(f"{delta_lake_path}/{path}")
            data.show()
        except Exception:
            pass
        return data

    return f

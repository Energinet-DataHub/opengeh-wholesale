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
from tests.helpers.spark_sql_migration_helper import MigrationsExecution


class MigrationsConfiguration:
    def __init__(self, configuration: dict):
        configuration.setdefault("execute", MigrationsExecution.ALL.name)
        self.execute = MigrationsExecution[configuration["execute"]]


class FeatureTestsConfiguration:
    def __init__(self, configuration: dict):
        configuration.setdefault("show_actual_and_expected", False)
        configuration.setdefault(
            "show_columns_when_actual_and_expected_are_equal", False
        )
        configuration.setdefault("show_actual_and_expected_count", False)
        self.show_actual_and_expected = configuration["show_actual_and_expected"]
        self.show_columns_when_actual_and_expected_are_equal = configuration[
            "show_columns_when_actual_and_expected_are_equal"
        ]
        self.show_actual_and_expected_count = configuration[
            "show_actual_and_expected_count"
        ]


class TestSessionConfiguration:

    # Pytest test classes will fire a warning if it has a constructor (__init__).
    # To avoid a class being treated as a test class set the attribute __test__  to False.
    __test__ = False

    def __init__(self, configuration: dict):
        configuration.setdefault("migrations", {})
        configuration.setdefault("feature_tests", {})
        self.migrations = MigrationsConfiguration(configuration["migrations"])
        self.feature_tests = FeatureTestsConfiguration(configuration["feature_tests"])

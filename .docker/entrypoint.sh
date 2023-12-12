#!/bin/sh -l

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

# Configure Azure CLI to use token cache which must be mapped as volume from host machine
export AZURE_CONFIG_DIR=/root/.azure

cd source/databricks/calculation_engine/tests/

# There env vars are important to ensure that the driver and worker nodes in spark are alligned
export PYSPARK_PYTHON=/opt/conda/bin/python
export PYSPARK_DRIVER_PYTHON=/opt/conda/bin/python

# Writing output to log with 'tee' cause exit code to be '0' even if tests fails
coverage run --branch -m pytest --junitxml=pytest-results.xml . | tee pytest-results.log

################################################
# Scenario: Test errors (exceptions) occured
################################################

# EXAMPLE 1 test summary which our regex can match:
# ================== 65 passed, 195 errors in 94.15s (0:01:34) ===================

# EXAMPLE 2 test summary which our regex can match:
# ================== 65 passed, 1 error in 94.15s (0:01:34) ===================

# If test summary contains errors we return exit code 2 to signal we want to retry
matchTestErrors=$(grep -Po '^=+.* [[:digit:]]+ error.* in .*=+$' pytest-results.log)
if [ ! -z "$matchTestErrors" ]; then
  echo "Test errors occured, which is typically caused by network issues (download failure). We should retry."
  exit 2
fi

################################################
# Scenario: Only 'entry point tests' failed
################################################

# EXAMPLE test summary which our regex can match:
# =========================== short test summary info ============================
# FAILED entry_points/test_entry_points.py::test__entry_point__start_calculator__returns_0
# FAILED entry_points/test_entry_points.py::test__entry_point__uncommitted_migrations_count__returns_0
# FAILED entry_points/test_entry_points.py::test__entry_point__unlock_storage__returns_0
# FAILED entry_points/test_entry_points.py::test__entry_point__lock_storage__returns_0
# FAILED entry_points/test_entry_points.py::test__entry_point__migrate_data_lake__returns_0
# ============= 5 failed, 264 passed, 3 skipped in 191.60s (0:03:11) =============

# If test summary only contains 5 failing 'entry point tests' return exit code 2 to signal we want to retry
# See https://unix.stackexchange.com/questions/637959/regex-matching-multi-line-search for understanding use of
#   z parameter to grep
#   (?m) in regex
#   pipe to remove NULL
matchFailedEntryPointTests=$(grep -Pzo '(?m)=+ short test summary info =+\n(FAILED .*test_entry_points.py::.*\n){5}=+ 5 failed' pytest-results.log | tr -d '\0')
if [ ! -z "$matchFailedEntryPointTests" ]; then
  echo "Only 'entry point tests' failed. We should retry."
  exit 2
fi

################################################
# Scenario: Not only 'entry point tests' failed
################################################

# If test summary contains other combination of failed tests we return exit code 1 to signal we DO NOT want to retry
matchFailedTests=$(grep -Po '^=+.* [[:digit:]]+ failed.* in .*=+$' pytest-results.log)
if [ ! -z "$matchFailedTests" ]; then
  echo "Not only 'entry point tests' failed. We should not retry."
  exit 1
fi

# Exit immediately with failure status if any command fails
set -e

# Create data for threshold evaluation
coverage json
# Create human reader friendly HTML report
coverage html
coverage-threshold --line-coverage-min 25

#!/bin/bash -l

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

# $1: Mandatory test folder path
# $2: (Optional) Can be set to specify a filter for running python tests by using 'keyword expressions'.
# See use of '-k' and 'keyword expressions' here: https://docs.pytest.org/en/7.4.x/how-to/usage.html#specifying-which-tests-to-run
echo "Tests folder path: '$1'"
echo "Filter (paths): '$2'"

# Configure Azure CLI to use token cache which must be mapped as volume from host machine
export AZURE_CONFIG_DIR=/home/joyvan/.azure

# There env vars are important to ensure that the driver and worker nodes in spark are alligned
export PYSPARK_PYTHON=/opt/conda/bin/python
export PYSPARK_DRIVER_PYTHON=/opt/conda/bin/python

# Exit immediately with failure status if any command fails
set -e

# Enable extended globbing. E.g. see https://stackoverflow.com/questions/8525437/list-files-not-matching-a-pattern
shopt -s extglob

cd $1
coverage run --branch -m pytest -vv --junitxml=pytest-results.xml $2

# Create data for threshold evaluation
coverage json
# Create human reader friendly HTML report
coverage html

coverage-threshold --line-coverage-min 25

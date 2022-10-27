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
from setuptools import setup, find_packages

setup(
    name="package",
    version=1.0,
    description="Tools for wholesale",
    long_description="",
    long_description_content_type="text/markdown",
    license="MIT",
    packages=find_packages(),
    install_requires=[
        "ConfigArgParse==1.5.3",
        "pyspark==3.3.0",
        "azure-storage-blob==12.7.1",
    ],
    entry_points={
        "console_scripts": [
            "start_stream = package.integration_events_persister_streaming:start",
            "start_calculator = package.calculator_job:start",
            "begin_migration = package.datamigration.begin_migration:start",
        ]
    },
)

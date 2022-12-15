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
    packages=find_packages(where="src"),
    package_dir={"": "src"},
    install_requires=[
        "ConfigArgParse==1.5.3",
        "pyspark==3.3.0",
        "azure-identity==1.11.0",
        "azure-storage-file-datalake==12.9.1",
        "azure-storage-blob==12.14.1",
        "databricks-cli==0.17.3",
        "python-dateutil==2.8.2",
    ],
    entry_points={
        "console_scripts": [
            "start_calculator = package.calculator_job:start",
            "lock_storage = package.datamigration.lock_storage:lock",
            "unlock_storage = package.datamigration.lock_storage:unlock",
            "migrate_data_lake = package.datamigration.migration:migrate_data_lake",
            "uncommitted_migrations_count = package.datamigration.uncommitted_migrations:print_count",
        ]
    },
)

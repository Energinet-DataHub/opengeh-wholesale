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
    package_data={"package": ["datamigration/migration_scripts/*.sql"]},
    packages=find_packages(exclude=["tests*"]),
    # Make sure these packages are added to the docker container and pinned to the same versions
    install_requires=[
        "ConfigArgParse==1.5.3",
        "pyspark==3.3.*",
        "azure-identity==1.12.0",
        "azure-storage-file-datalake==12.11.0",
        "databricks-cli==0.17.6",
        "urllib3==1.26.11",
        # urllib3 needs to be specific version because of bug https://community.databricks.com/s/topic/0TO8Y000000mOi5WAE/method-whitelist
        "delta-spark==2.2.0",
        "python-dateutil==2.8.2",
        "azure-monitor-opentelemetry==1.0.0",
    ],
    entry_points={
        "console_scripts": [
            "start_calculator = package.calculator_job:start",
            "start_basis_data_writer = package.calculator_job:start_basis_data_writer",
            "lock_storage = package.infrastructure.storage_account_access.lock_storage:lock",
            "unlock_storage = package.infrastructure.storage_account_access.lock_storage:unlock",
            "migrate_data_lake = package.datamigration.migration:migrate_data_lake",
            "uncommitted_migrations_count = package.datamigration.uncommitted_migrations:print_count",
            # Entry point used for integration testing
            "list_migrations_in_package = package.datamigration.uncommitted_migrations:print_all_migrations_in_package",
        ]
    },
)

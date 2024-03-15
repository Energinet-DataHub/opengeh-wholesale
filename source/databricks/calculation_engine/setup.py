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
    package_data={
        "package": [
            "datamigration/migration_scripts/*.sql",
            "datamigration/current_state_scripts/schemas/*.sql",
            "datamigration/current_state_scripts/tables/*.sql",
        ]
    },
    packages=find_packages(exclude=["tests*"]),
    # Make sure these packages are added to the docker container and pinned to the same versions
    install_requires=[
        "ConfigArgParse==1.5.3",
        "pyspark==3.5.0",
        "azure-identity==1.12.0",
        "azure-storage-file-datalake==12.11.0",
        "databricks-cli==0.18",
        "dependency_injector==4.41.0",
        "urllib3==2.2.*",
        "delta-spark==3.1.0",
        "python-dateutil==2.8.2",
        "azure-monitor-opentelemetry==1.2.0",
        "opengeh-spark-sql-migrations @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@1.4.1#subdirectory=source/spark_sql_migrations",
    ],
    entry_points={
        "console_scripts": [
            "start_calculator = package.calculator_job:start",
            "lock_storage = package.infrastructure.storage_account_access.lock_storage:lock",
            "unlock_storage = package.infrastructure.storage_account_access.lock_storage:unlock",
            "migrate_data_lake = package.datamigration.migration:migrate_data_lake",
        ]
    },
)

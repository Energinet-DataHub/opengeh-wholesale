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
        ]
    },
    packages=find_packages(exclude=["tests*"]),
    # Make sure these packages are added to the docker container and pinned to the same versions
    install_requires=[
        "ConfigArgParse==1.5.3",
        "pyspark==3.5.1",
        "azure-identity==1.19.0",
        "dependency_injector==4.43.0",
        "urllib3==2.2.*",
        "delta-spark==3.1.0",
        "python-dateutil==2.9.0",
        "azure-monitor-opentelemetry==1.6.4",
        "azure-core==1.32.0",
        "opengeh-spark-sql-migrations @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@1.9.0#subdirectory=source/spark_sql_migrations",
        "opengeh-telemetry @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@2.1.0#subdirectory=source/telemetry",
    ],
    entry_points={
        "console_scripts": [
            "start_calculator = package.calculator_job:start",
            "migrate_data_lake = package.datamigration.migration:migrate_data_lake",
            "optimize_delta_tables = package.optimize_job.delta_optimization:optimize_tables",
        ]
    },
)

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
        "ConfigArgParse==1.7.0",
        "pyspark==3.5.1",  # This needs to be same version pyspark-slim in docker file
        "azure-identity==1.17.1",
        "dependency_injector==4.43.0",
        "urllib3==2.2.*",
        "delta-spark==3.2.0",
        "python-dateutil==2.8.2",
        "azure-monitor-opentelemetry==1.6.4",
        "azure-core==1.32.0",
        "opengeh-spark-sql-migrations @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@2.4.2#subdirectory=source/spark_sql_migrations",
        "opengeh-telemetry @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@2.4.2#subdirectory=source/telemetry",
        "opengeh-testcommon @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@3.3.0#subdirectory=source/testcommon",
        "opengeh-pyspark @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@3.3.1#subdirectory=source/pyspark_functions",
    ],
    entry_points={
        "console_scripts": [
            "start_calculator = package.calculator_job:start",
            "migrate_data_lake = package.datamigration.migration:migrate_data_lake",
            "optimize_delta_tables = package.optimize_job.delta_optimization:optimize_tables",
        ]
    },
)

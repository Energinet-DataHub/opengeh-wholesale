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
    name="opengeh-settlement-report",
    version=1.0,
    description="Tools for settlement report",
    long_description="",
    long_description_content_type="text/markdown",
    license="MIT",
    packages=find_packages(),
    # Make sure these packages are added to the docker container and pinned to the same versions
    install_requires=[
        "ConfigArgParse==1.5.3",
        "pyspark==3.5.1",
        "delta-spark==3.1.0",
        "python-dateutil==2.8.2",
        "azure-monitor-opentelemetry==1.6.0",
        "azure-core==1.30.0",
        "opengeh-telemetry @ git+https://git@github.com/Energinet-DataHub/opengeh-python-packages@2.1.0#subdirectory=source/telemetry",
    ],
    entry_points={
        "console_scripts": [
            "create_hourly_time_series    = settlement_report_job.entry_points.entry_point:start_hourly_time_series_points",
            "create_quarterly_time_series = settlement_report_job.entry_points.entry_point:start_quarterly_time_series_points",
            "create_metering_point_periods = settlement_report_job.entry_points.entry_point:start_metering_point_periods",
            "create_charge_links          = settlement_report_job.entry_points.entry_point:start_charge_link_periods",
            "create_charge_price_points   = settlement_report_job.entry_points.entry_point:start_charge_price_points",
            "create_energy_results        = settlement_report_job.entry_points.entry_point:start_energy_results",
            "create_monthly_amounts       = settlement_report_job.entry_points.entry_point:start_monthly_amounts",
            "create_wholesale_results     = settlement_report_job.entry_points.entry_point:start_wholesale_results",
            "create_zip                   = settlement_report_job.entry_points.entry_point:start_zip",
        ]
    },
)

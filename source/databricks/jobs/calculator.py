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

import click


@click.command()
@click.option("--data-storage-account-name", type=str, required=True)
@click.option("--data-storage-account-key", type=str, required=True)
@click.option("--integration-events-path", type=str, required=True)
@click.option("--time-series-points-path", type=str, required=True)
@click.option("--process-results-path", type=str, required=True)
def run(
    data_storage_account_name,
    data_storage_account_key,
    integration_events_path,
    time_series_points_path,
    process_results_path
):
    pass


if __name__ == "__main__":
    run()

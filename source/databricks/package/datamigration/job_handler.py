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

import time
from databricks_cli.sdk.api_client import ApiClient
from databricks_cli.jobs.api import JobsApi
from databricks_cli.runs.api import RunsApi
from databricks_cli.sdk import JobsService
from package import log


def get_api_client(host, token):
    return ApiClient(
        host=host,  # example: "https://adb-5870161604877074.14.azuredatabricks.net",
        token=token,  # the databricks token,
    )


def stop_databricks_jobs(api_client, job_names_to_stop):
    jobService = JobsService(api_client)
    jobs_api = JobsApi(api_client)

    jobs_list = jobs_api.list_jobs()["jobs"]  # get all information about all jobs
    job_list_with_ids = _get_job_ids(jobs_list)  # dict key: job_name and value: job_id

    # stop all runs for the jobs that should be stopped
    for job_name in job_names_to_stop:
        log(f"stopping ${job_name_to_stop}")
        jobService.cancel_all_runs(job_list_with_ids[job_name])

    # wait until all runs has stoped
    all_jobs_has_been_stopped = False
    while not all_jobs_has_been_stopped:
        all_jobs_has_been_stopped = True
        for job_name in job_names_to_stop:
            job_id = job_list_with_ids[job_name]
            if "runs" in jobService.list_runs(job_id, True):
                all_jobs_has_been_stopped = False
                break
        time.sleep(1)


def start_databricks_jobs(api_client, jobs_to_start):
    jobService = JobsService(api_client)
    jobs_api = JobsApi(api_client)

    jobs_list = jobs_api.list_jobs()["jobs"]
    job_list_with_ids = _get_job_ids(jobs_list)

    for job_name in jobs_to_start:
        job_id = job_list_with_ids[job_name]
        jobService.run_now(job_id)


def _get_job_ids(jobs_list):
    jobs_dict = {}
    for job in jobs_list:
        jobs_dict[job["settings"]["name"]] = job["job_id"]

    return jobs_dict

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

import requests
import gzip
import datetime


class CoordinatorService:

    def __init__(self, args):
        args_dict = vars(args)

        self.job_id = args.job_id
        self.process_type = None

        if args_dict.get('process_type') is not None:
            self.process_type = args.process_type

    def __endpoint(self, path, endpoint, snapshot_id: str):
        TIMESTRING = "%Y-%m-%d %H:%M:%S"

        try:
            bytes = path.encode()
            headers = {'job-id': self.job_id,
                       'snapshot-id': snapshot_id,
                       'process-type': self.process_type,
                       'Content-Type': 'application/json',
                       'Content-Encoding': 'gzip'}

            request_body = gzip.compress(bytes)
            now = datetime.datetime.now()
            print("Just about to post " + str(len(request_body)) + " bytes at time " + now.strftime(TIMESTRING))
            response = requests.post(endpoint, data=request_body, headers=headers)
            now = datetime.datetime.now()
            print("We have posted the result and time is now " + now.strftime(TIMESTRING))
            if response.status_code != requests.codes['ok']:
                error = "Could not communicate with coordinator due to " + response.reason
                print(error)
                print(response.text)
                now = datetime.datetime.now()
                print(now.strftime(TIMESTRING))
                raise Exception(error)
        except Exception:
            print(Exception)
            raise Exception

    def notify_snapshot_coordinator(self, snapshot_notify_url, path, snapshot_id):
        self.__endpoint(path, snapshot_notify_url, snapshot_id)

    def notify_coordinator(self, result_url, path):
        self.__endpoint(path, result_url, "")

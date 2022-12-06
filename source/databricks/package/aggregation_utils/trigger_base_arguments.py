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

# Uncomment the lines below to include modules distributed by wheel

import configargparse


def trigger_base_arguments():
    p = configargparse.ArgParser(description='Green Energy Hub Tempory aggregation triggger', formatter_class=configargparse.ArgumentDefaultsHelpFormatter)
    p.add('--data-storage-account-name', type=str, required=True, help='Azure Storage account name for master data')
    p.add('--data-storage-account-key', type=str, required=True, help='Azure Storage key for master data')
    p.add('--data-storage-container-name', type=str, required=True, default='data', help='Azure Storage container name for master data')
    p.add('--result-url', type=str, required=True, help="The target url to post result json")
    p.add('--job-id', type=str, required=False, default="", help="Postback id that will be added to header. The id is unique")
    p.add('--snapshot-id', type=str, required=True, help="Id to mark snapshots The id is unique")
    p.add('--snapshot-path', type=str, required=True, default="snapshots")
    return p

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

from azure.storage.filedatalake import FileSystemClient
from os import path


def is_directory_empty(file_system_client: FileSystemClient, path: str) -> bool:
    sub_paths = file_system_client.get_paths(path)
    # ItemPaged has no __len__ method, so we need to iterate in order to determine whether there are any items
    for unused in sub_paths:
        return False
    return True


def move_and_rename_folder(
    file_system_client: FileSystemClient, source_path: str, target_path: str
) -> None:
    directory_client = file_system_client.get_directory_client(source_path)
    if not directory_client.exists():
        print(
            f"Skipping migration ({__file__}). Source directory not found:{source_path}"
        )
        return

    target_path = path.join(file_system_client.file_system_name, target_path)
    directory_client.rename_directory(new_name=target_path)

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

import zipfile

from typing import Any
from telemetry_logging import use_span


@use_span()
def create_zip_file(
    dbutils: Any, report_id: str, save_path: str, files_to_zip: list[str]
) -> None:
    """Creates a zip file from a list of files and saves it to the specified path.

    Notice that we have to create the zip file in /tmp and then move it to the desired
    location. This is done as `direct-append` or `non-sequential` writes are not
    supported in Databricks.

    Args:
        dbutils (Any): The DBUtils object.
        report_id (str): The report ID.
        save_path (str): The path to save the zip file.
        files_to_zip (list[str]): The list of files to zip.

    Raises:
        Exception: If there are no files to zip.
        Exception: If the save path does not end with .zip.
    """
    if len(files_to_zip) == 0:
        raise Exception("No files to zip")
    if not save_path.endswith(".zip"):
        raise Exception("Save path must end with .zip")

    tmp_path = f"/tmp/{report_id}.zip"
    with zipfile.ZipFile(tmp_path, "a", zipfile.ZIP_DEFLATED) as ref:
        for fp in files_to_zip:
            file_name = fp.split("/")[-1]
            ref.write(fp, arcname=file_name)
    dbutils.fs.mv(f"file:{tmp_path}", save_path)

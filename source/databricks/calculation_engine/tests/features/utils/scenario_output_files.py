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
import inspect
import os
from pathlib import Path


def get_output_names() -> list[str]:
    filename = inspect.stack()[1].filename
    folder = os.path.dirname(filename)
    output_folder_path = Path(folder + "/then/")
    csv_files = list(output_folder_path.rglob("*.csv"))
    return [Path(file).stem for file in csv_files]

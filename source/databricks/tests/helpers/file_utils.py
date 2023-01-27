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

import glob
import os


def find_file(path: str, pattern: str) -> str:
    """The path of the first file matching the Unix style pathname pattern expansion (globbing).
    Raises an exception if no file is found.
    """
    os.chdir(path)
    for file_path in glob.glob(pattern):
        return file_path
    raise Exception("Target test file not found.")


def create_file_path_expression(directory_expression: str, extension: str) -> str:
    """Create file path regular expression from a directory expression
    and a file extension.
    The remaining base file name can be one or more characters except for forward slash ("/").
    """
    return f"{directory_expression}[^/]+{extension}"

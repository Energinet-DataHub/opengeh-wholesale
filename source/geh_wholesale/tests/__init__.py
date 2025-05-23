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
"""
By having a __init__.py in this root directory, we can use the test explorer in VS Code.
"""

from pathlib import Path

PROJECT_PATH = Path(__file__).parent.parent
TESTS_PATH = Path(__file__).parent
SPARK_CATALOG_NAME = "spark_catalog"

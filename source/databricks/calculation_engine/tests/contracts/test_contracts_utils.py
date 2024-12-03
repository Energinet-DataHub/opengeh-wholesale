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


def get_database_objects(module):
    result = {}
    for name, obj in inspect.getmembers(module):
        if inspect.isclass(obj) and obj.__name__.startswith("Wholesale"):
            database_name = getattr(obj, "DATABASE_NAME", None)
            table_names = getattr(obj, "TABLE_NAMES", [])
            view_names = getattr(obj, "VIEW_NAMES", [])
            if database_name:
                result[database_name] = {"tables": table_names, "views": view_names}
    return result

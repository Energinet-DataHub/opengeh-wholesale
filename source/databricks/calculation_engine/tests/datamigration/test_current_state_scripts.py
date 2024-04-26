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

from unittest.mock import Mock
from pyspark.sql import SparkSession
import package.datamigration.migration as sut
import tests.helpers.mock_helper as mock_helper
import tests.helpers.spark_helper as spark_helper
import package.datamigration.schema_config as schema_config


def test__current_state_scripts__when_migration_scripts_executed_and_deleted__then_current_state_should_run_and_create_tables(
    spark: SparkSession,
    mocker: Mock,
) -> None:
    # Arrange
    storage_account = "storage_account_5"
    mocker.patch.object(
        sut.paths,
        sut.paths.get_storage_account_url.__name__,
        side_effect=mock_helper.base_path_helper,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_storage_account_name.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.env_vars,
        sut.env_vars.get_calculation_input_folder_name.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_spark_sql_migrations_path.__name__,
        return_value=storage_account,
    )

    mocker.patch.object(
        sut.paths,
        sut.paths.get_container_root_path.__name__,
        return_value=storage_account,
    )

    # Act 1 - Migration Scripts
    sut.migrate_data_lake()

    # Remove the tables
    spark_helper.remove_all_tables(spark)

    # Act 2 - Current State Scripts
    sut.migrate_data_lake()

    # Assert
    schemas = spark.catalog.listDatabases()
    for schema in schema_config.schema_config:
        assert schema.name in [schema.name for schema in schemas]
        for table in schema.tables:
            actual_table = spark.table(f"{schema.name}.{table.name}")
            assert actual_table.schema == table.schema

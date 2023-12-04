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
import pytest

from package.calculator_job import start, start_with_deps


class TestWhenInvokedWithInvalidArguments:
    def test_exits_with_code_2(self):
        """The exit code 2 originates from the argparse library."""
        with pytest.raises(SystemExit) as system_exit:
            start()

        assert system_exit.value.code == 2


@pytest.fixture()
def setup_env_variables():
    with open():
        os.environ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = XXX


class TestWhenInvokedWithValidArguments:
    @pytest.mark.usefixtures(setup_env_variables.__name__)
    def test_does_not_raise(self, any_calculator_args):
        start_with_deps(
            cmd_line_args_reader=lambda: any_calculator_args,
            calculation_executor=lambda args, reader: None,
            is_storage_locked_checker=lambda name, cred: False,
            applicationinsights_connection_string=XXX,
        )

        logs = get_logs(xxx)  # log analytics query
        assert logs[0].message == "Calculator job started"

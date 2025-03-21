from pathlib import Path

from geh_common.testing.covernator.commands import create_all_cases_file, create_result_and_all_scenario_files

output_folder = Path("/workspace/source/geh_wholesale/run_outputs")
create_all_cases_file(
    output_folder,
    Path("/workspace/source/geh_wholesale/tests/coverage/all_cases_wholesale.yml"),
)

create_result_and_all_scenario_files(
    output_folder,
    Path("/workspace/source/geh_wholesale/tests/scenarios"),
)

from geh_common.testing.job.project_script import assert_pyproject_toml_project_script_exists

from tests import PROJECT_PATH


def test__entry_point_exists() -> None:
    assert_pyproject_toml_project_script_exists(
        pyproject_toml_path=PROJECT_PATH / "pyproject.toml",
    )

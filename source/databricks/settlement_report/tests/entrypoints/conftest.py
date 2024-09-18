import os
import subprocess
from typing import Generator

import pytest


@pytest.fixture(scope="session")
def virtual_environment() -> Generator:
    """Fixture ensuring execution in a virtual environment.
    Uses `virtualenv` instead of conda environments due to problems
    activating the virtual environment from pytest."""

    # Create and activate the virtual environment
    subprocess.call(["virtualenv", ".wholesale-pytest"])
    subprocess.call(
        "source .wholesale-pytest/bin/activate", shell=True, executable="/bin/bash"
    )

    yield None

    # Deactivate virtual environment upon test suite tear down
    subprocess.call("deactivate", shell=True, executable="/bin/bash")


@pytest.fixture(scope="session")
def installed_package(
    virtual_environment: Generator, settlement_report_job_container_path: str
) -> None:
    """Ensures that the wholesale package is installed (after building it)."""

    # Build the package wheel
    os.chdir(settlement_report_job_container_path)
    subprocess.call("python -m build --wheel", shell=True, executable="/bin/bash")

    # Uninstall the package in case it was left by a cancelled test suite
    subprocess.call(
        "pip uninstall -y package",
        shell=True,
        executable="/bin/bash",
    )

    # Install wheel, which will also create console scripts for invoking
    # the entry points of the package
    subprocess.call(
        f"pip install {settlement_report_job_container_path}/dist/opengeh_settlement_report-1.0-py3-none-any.whl",
        shell=True,
        executable="/bin/bash",
    )

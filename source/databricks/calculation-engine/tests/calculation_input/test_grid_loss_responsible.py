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
from package.calculation_input.grid_loss_responsible import get_grid_loss_responsible


def test__get_grid_loss_responsible__returns_returns_non_empty_list() -> None:
    # Arrange
    grid_areas = ["804"]

    # Act
    grid_loss_responsible = get_grid_loss_responsible(grid_areas)

    # Assert
    assert grid_loss_responsible.count() > 0


@pytest.mark.acceptance_test
def test__get_grid_loss_responsible__when_no_grid_loss_responsible_in_grid_area__raise_exception() -> (
    None
):
    # Arrange
    grid_areas = ["grid_area_without_grid_loss_responsible"]

    # Act and Assert
    with pytest.raises(Exception):
        get_grid_loss_responsible(grid_areas)

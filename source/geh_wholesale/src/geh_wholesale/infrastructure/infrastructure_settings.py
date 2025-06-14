from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class InfrastructureSettings(BaseSettings):  # type: ignore
    """InfrastructureSettings class uses Pydantic BaseSettings to configure and validate parameters.

    Parameters can come from both runtime (CLI) or from environment variables.
    The priority is CLI parameters first and then environment variables.
    """

    model_config = SettingsConfigDict(
        cli_prog_name="infrastructure",
        cli_parse_args=True,
        cli_kebab_case=True,
        cli_ignore_unknown_args=True,
        cli_implicit_flags=True,
    )

    catalog_name: str = Field(init=False)
    calculation_input_database_name: str = Field(init=False)
    data_storage_account_name: str = Field(init=False)

    # (repr=False) prevents the field from being printed in the repr of the model
    tenant_id: str = Field(init=False, repr=False)
    spn_app_id: str = Field(init=False, repr=False)
    spn_app_secret: str = Field(init=False, repr=False)

    calculation_input_folder_name: str | None = Field(init=False, default=None)
    time_series_points_table_name: str | None = Field(init=False, default=None)
    metering_point_periods_table_name: str | None = Field(init=False, default=None)
    grid_loss_metering_point_ids_table_name: str | None = Field(init=False, default=None)

    azure_app_configuration_endpoint: str = Field(init=False)

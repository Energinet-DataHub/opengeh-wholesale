from settlement_report_job.infrastructure.environment_variables import get_catalog_name


def get_metering_point_time_series_view() -> str:
    CATALOG_NAME = get_catalog_name()
    return f"{CATALOG_NAME}.wholesale_settlement_reports.metering_point_time_series_v1"


def get_output_volume() -> str:
    CATALOG_NAME = get_catalog_name()
    return (
        f"/Volumes/{CATALOG_NAME}/wholesale_settlement_report_output/settlement_reports"
    )


METERING_POINT_TYPE_DICT = {
    "ve_production": "D01",
    "net_production": "D05",
    "supply_to_grid": "D06",
    "consumption_from_grid": "D07",
    "wholesale_services_information": "D08",
    "own_production": "D09",
    "net_from_grid": "D10",
    "net_to_grid": "D11",
    "total_consumption": "D12",
    "electrical_heating": "D14",
    "net_consumption": "D15",
    "effect_settlement": "D19",
    "consumption": "E17",
    "production": "E18",
    "exchange": "E20",
}

RESOLUTION_NAMES = {"PT1H": "TSSD60", "PT15M": "TSSD15"}

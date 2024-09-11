from package.settlement_report_job.environment_variables import get_catalog_name


def get_energy_view() -> str:
    CATALOG_NAME = get_catalog_name()
    return f"{CATALOG_NAME}.wholesale_results.energy_v1"


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
    "total_consumption": "D12",
    "electrical_heating": "D14",
    "net_consumption": "D15",
    "effect_settlement": "D19",
    "consumption": "E17",
    "production": "E18",
    "exchange": "E20",
}

RESOLUTION_NAMES = {"PT1H": "TSSD60", "PT15M": "TSSD15"}

WORST_CASE_GRID_AREA_CODES = {
    "003": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "007": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "016": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "031": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "042": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "051": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "084": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "085": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "131": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "141": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "151": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "154": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "233": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "244": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "245": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "331": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "341": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "342": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "344": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "347": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "348": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "351": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "357": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "370": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "371": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "381": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "384": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "385": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "396": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "531": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "532": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "533": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "543": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "584": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "740": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "757": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "791": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "853": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "854": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "860": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "911": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "950": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "951": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "952": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "953": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "954": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "960": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "962": "32e49805-20ef-4db2-ac84-c4455de7a373",
    "990": "32e49805-20ef-4db2-ac84-c4455de7a373",
}

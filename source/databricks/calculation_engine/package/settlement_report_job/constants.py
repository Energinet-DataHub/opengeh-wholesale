from package.settlement_report_job.environment_variables import get_catalog_name


def get_energy_view() -> str:
    CATALOG_NAME = get_catalog_name()
    return f"{CATALOG_NAME}.wholesale_results.energy_v1"


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

SETTLEMENT_METHOD_DICT = {
    "non_profiled": "E02",
    "flex": "D01",
    None: None,
}

CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS = {
    "balance_fixing": "D04",
    "wholesale_fixing": "D05",
    "first_correction_settlement": "D32",
    "second_correction_settlement": "D32",
    "third_correction_settlement": "D32",
}

RESOLUTION_NAMES = {"PT1H": "TSSD60", "PT15M": "TSSD15"}

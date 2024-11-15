from pyspark.sql import functions as F
from settlement_report_job.domain.utils.csv_column_names import CsvColumnNames


def order_by_columns() -> list:
    return [
        F.col(CsvColumnNames.grid_area_code),
        F.col(CsvColumnNames.energy_supplier_id),
        F.col(CsvColumnNames.metering_point_type),
        F.col(CsvColumnNames.settlement_method),
        F.col(CsvColumnNames.time),
        F.col(CsvColumnNames.charge_owner_id),
        F.col(CsvColumnNames.charge_type),
        F.col(CsvColumnNames.charge_code),
    ]

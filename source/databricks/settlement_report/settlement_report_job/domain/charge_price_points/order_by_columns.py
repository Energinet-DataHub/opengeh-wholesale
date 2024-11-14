from settlement_report_job.domain.utils.csv_column_names import CsvColumnNames


def order_by_columns() -> list[str]:
    return [
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.resolution,
        CsvColumnNames.is_tax,
        CsvColumnNames.time,
    ]

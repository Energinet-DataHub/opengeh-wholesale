class CsvColumnNames:
    amount = "AMOUNT"
    charge_id = "CHARGEID"
    charge_occurrences = "CHARGEOCCURRENCES"
    charge_owner = "CHARGEOWNER"
    charge_type = "CHARGETYPE"
    energy_business_process = "ENERGYBUSINESSPROCESS"
    energy_currency = "ENERGYCURRENCY"
    energy_quantity = "ENERGYQUANTITY"
    energy_supplier_id = "ENERGYSUPPLIERID"
    grid_area_code = "METERINGGRIDAREAID"
    measure_unit = "MEASUREUNIT"
    metering_point_id = "METERINGPOINTID"
    period_start = "PERIODSTART"
    period_end = "PERIODEND"
    price = "PRICE"
    process_variant = "PROCESSVARIANT"
    resolution_duration = "RESOLUTIONDURATION"
    settlement_method = "SETTLEMENTMETHOD"
    start_date_time = "STARTDATETIME"
    type_of_mp = "TYPEOFMP"


class EphemeralColumns:
    # Columns that are added to the DataFrame for processing
    # but not part of the input or output schema.

    grid_area_code = "grid_area_code_partition"
    chunk_index = "chunk_index_partition"
    start_of_day = "start_of_day"
    quantities = "quantities"

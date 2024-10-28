class CsvColumnNames:
    energy_business_process = "ENERGYBUSINESSPROCESS"
    energy_quantity = "ENERGYQUANTITY"
    energy_supplier_id = "ENERGYSUPPLIERID"
    grid_area_code = "METERINGGRIDAREAID"
    type_of_mp = "TYPEOFMP"
    metering_point_id = "METERINGPOINTID"
    resolution_duration = "RESOLUTIONDURATION"
    settlement_method = "SETTLEMENTMETHOD"
    start_date_time = "STARTDATETIME"
    # When ENERGYBUSINESSPROCESS is Correction Settlement - D32
    # PROCESSVARIANT is either First - D01 - 1ST, Second - D02 - 2ND, Third - D03 - 3RD
    process_variant = "PROCESSVARIANT"
    measure_unit = "MEASUREUNIT"
    energy_currency = "ENERGYCURRENCY"
    price = "PRICE"
    amount = "AMOUNT"
    charge_type = "CHARGETYPE"
    charge_id = "CHARGEID"
    charge_owner = "CHARGEOWNER"


class EphemeralColumns:
    # Columns that are added to the DataFrame for processing
    # but not part of the input or output schema.

    grid_area_code = "grid_area_code_partition"
    chunk_index = "chunk_index_partition"
    start_of_day = "start_of_day"
    quantities = "quantities"

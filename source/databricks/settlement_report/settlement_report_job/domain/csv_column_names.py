class CsvColumnNames:
    amount = "AMOUNT"
    calculation_type = "ENERGYBUSINESSPROCESS"
    charge_code = "CHARGEID"
    charge_link_from_date = "PERIODSTART"
    charge_link_to_date = "PERIODEND"
    charge_occurrences = "CHARGEOCCURRENCES"
    charge_owner_id = "CHARGEOWNER"
    charge_type = "CHARGETYPE"
    correction_settlement_number = "PROCESSVARIANT"
    currency = "ENERGYCURRENCY"
    energy_supplier_id = "ENERGYSUPPLIERID"
    grid_area_code = "METERINGGRIDAREAID"
    metering_point_id = "METERINGPOINTID"
    metering_point_type = "TYPEOFMP"
    period_start = "period_start"
    price = "PRICE"
    quantity = "ENERGYQUANTITY"
    quantity_unit = "MEASUREUNIT"
    resolution = "RESOLUTIONDURATION"
    settlement_method = "SETTLEMENTMETHOD"
    time = "STARTDATETIME"


class EphemeralColumns:
    # Columns that are added to the DataFrame for processing
    # but not part of the input or output schema.

    grid_area_code = "grid_area_code_partition"
    chunk_index = "chunk_index_partition"
    start_of_day = "start_of_day"
    quantities = "quantities"

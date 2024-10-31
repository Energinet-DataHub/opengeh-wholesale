class CsvColumnNames:
    amount = "AMOUNT"
    calculation_type = "ENERGYBUSINESSPROCESS"
    charge_code = "CHARGEID"
    charge_link_from_date = "PERIODSTART"
    charge_link_to_date = "PERIODEND"
    charge_quantity = "CHARGEOCCURRENCES"
    charge_owner_id = "CHARGEOWNER"
    charge_type = "CHARGETYPE"
    correction_settlement_number = "PROCESSVARIANT"
    currency = "ENERGYCURRENCY"
    energy_quantity = "ENERGYQUANTITY"
    energy_supplier_id = "ENERGYSUPPLIERID"
    grid_area_code = "METERINGGRIDAREAID"
    metering_point_id = "METERINGPOINTID"
    metering_point_type = "TYPEOFMP"
    price = "PRICE"
    quantity_unit = "MEASUREUNIT"
    resolution = "RESOLUTIONDURATION"
    settlement_method = "SETTLEMENTMETHOD"
    time = "STARTDATETIME"


class EphemeralColumns:
    # Columns that are added to the DataFrame for processing
    # but not part of the input or output schema.

    grid_area_code_partitioning = "grid_area_code_partitioning"
    chunk_index = "chunk_index_partition"
    start_of_day = "start_of_day"

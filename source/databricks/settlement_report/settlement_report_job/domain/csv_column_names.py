class CsvColumnNames:
    calculation_type = "ENERGYBUSINESSPROCESS"
    energy_prefix = "ENERGYQUANTITY"
    energy_supplier_id = "ENERGYSUPPLIERID"
    grid_area_code = "METERINGGRIDAREAID"
    metering_point_type = "TYPEOFMP"
    metering_point_id = "METERINGPOINTID"
    quantity = "ENERGYQUANTITY"
    resolution = "RESOLUTIONDURATION"
    settlement_method = "SETTLEMENTMETHOD"
    start_of_day = "STARTDATETIME"
    time = "STARTDATETIME"
    # When ENERGYBUSINESSPROCESS is Correction Settlement - D32
    # PROCESSVARIANT is either First - D01 - 1ST, Second - D02 - 2ND, Third - D03 - 3RD
    process_variant = "PROCESSVARIANT"
    quantity_unit = "MEASUREUNIT"
    currency = "ENERGYCURRENCY"
    price = "PRICE"
    amount = "AMOUNT"
    charge_type = "CHARGETYPE"
    charge_code = "CHARGEID"
    charge_owner_id = "CHARGEOWNER"


class EphemeralColumns:
    """Columns that are added to the DataFrame for processing but not part of the input
    or output schema."""

    chunk_index = "chunk_index_partition"
    start_of_day = "start_of_day"
    quantities = "quantities"

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


class EphemeralColumns:
    """Columns that are added to the DataFrame for processing but not part of the input
    or output schema."""

    chunk_index = "chunk_index_partition"
    start_of_day = "start_of_day"
    quantities = "quantities"

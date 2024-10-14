class EnergyResultsCsvColumnNames:
    grid_area_code = "METERINGGRIDAREAID"
    calculation_type = "ENERGYBUSINESSPROCESS"
    time = "STARTDATETIME"
    resolution = "RESOLUTIONDURATION"
    metering_point_type = "TYPEOFMP"
    settlement_method = "SETTLEMENTMETHOD"
    quantity = "ENERGYQUANTITY"


class TimeSeriesPointCsvColumnNames:
    metering_point_id = "METERINGPOINTID"
    metering_point_type = "TYPEOFMP"
    energy_supplier_id = "ENERGYSUPPLIERID"
    start_of_day = "STARTDATETIME"
    energy_prefix = "ENERGYQUANTITY"


class EphemeralColumns:
    """Columns that are added to the DataFrame for processing but not part of the input
    or output schema."""

    uid = "uid"
    start_of_day = "start_of_day"
    quantities = "quantities"
    chunk_index = "chunk_index_partition"

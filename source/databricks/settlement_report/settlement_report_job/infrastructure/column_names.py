class DataProductColumnNames:
    calculation_id = "calculation_id"
    calculation_period_start = "calculation_period_start"
    calculation_period_end = "calculation_period_end"
    calculation_type = "calculation_type"
    calculation_version = "calculation_version"
    charge_key = "charge_key"
    charge_code = "charge_code"
    charge_owner_id = "charge_owner_id"
    charge_type = "charge_type"
    energy_supplier_id = "energy_supplier_id"
    from_date = "from_date"
    grid_area_code = "grid_area_code"
    is_tax = "is_tax"
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    observation_time = "observation_time"
    quantity = "quantity"
    quantity_unit = "quantity_unit"
    quantity_qualities = "quantity_qualities"
    resolution = "resolution"
    result_id = "result_id"
    settlement_method = "settlement_method"
    time = "time"
    to_date = "to_date"


class EnergyResultsCsvColumnNames:
    grid_area_code = "METERINGGRIDAREAID"
    calculation_type = "ENERGYBUSINESSPROCESS"
    time = "STARTDATETIME"
    resolution = "RESOLUTIONDURATION"
    metering_point_type = "TYPEOFMP"
    settlement_method = "SETTLEMENTMETHOD"
    quantity = "ENERGYQUANTITY"
    energy_supplier_id = "ENERGYSUPPLIERID"


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

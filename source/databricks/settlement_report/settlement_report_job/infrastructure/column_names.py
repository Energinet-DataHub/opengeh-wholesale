class DataProductColumnNames:
    grid_area_code = "grid_area_code"
    observation_time = "observation_time"
    resolution = "resolution"
    calculation_type = "calculation_type"
    calculation_id = "calculation_id"
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    quantity = "quantity"
    settlement_method = "settlement_method"
    time = "time"


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
    start_of_day = "STARTDATETIME"
    energy_prefix = "ENERGYQUANTITY"


class EphemeralColumns:
    """Columns that are added to the DataFrame for processing but not part of the input
    or output schema."""

    uid = "uid"
    start_of_day = "start_of_day"
    quantities = "quantities"
    large_files_split_column = "large_files_split"

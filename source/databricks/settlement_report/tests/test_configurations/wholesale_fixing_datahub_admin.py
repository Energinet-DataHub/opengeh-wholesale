from functools import reduce

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.domain.calculation_type import CalculationType

GRID_AREAS = ["804", "805"]
CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"
ENERGY_SUPPLIER_IDS = ["1000000000000", "2000000000000"]



def create_metering_point_time_series(spark: SparkSession) -> DataFrame:

    dataframes = []
    hourly_metering_point_time_series[
        data_spec = (
            metering_point_time_series_factory.MeteringPointTimeSeriesTestDataSpec(
                calculation_id=CALCULATION_ID,
                calculation_type=CalculationType.WHOLESALE_FIXING,
                calculation_version="1",
                metering_point_id="123456789012345678901234567",
                metering_point_type="consumption",
                resolution=MeteringPointResolution.HOUR,
                grid_area_code=grid_area_code,
                energy_supplier_id=energy_supplier_id)
        )
        df = metering_point_time_series_factory.create(spark, data_spec)
        dataframes.append(df)
        for grid_area_code in GRID_AREAS
        for energy_supplier_id in ENERGY_SUPPLIER_IDS
    ]
    return DataFrame.unionAll(dataframes)

    # dataframes = []
    # for grid_area in GRID_AREAS:
    #     for energy_supplier_id in ENERGY_SUPPLIER_IDS:
    #         data_spec = (
    #             metering_point_time_series_factory.MeteringPointTimeSeriesTestDataSpec(
    #                 calculation_id=CALCULATION_ID,
    #                 calculation_type=CalculationType.WHOLESALE_FIXING,
    #                 calculation_version="1",
    #                 metering_point_id="123456789012345678901234567",
    #                 metering_point_type="consumption",
    #                 resolution="PT15M",
    #                 grid_area_code=grid_area,
    #                 energy_supplier_id=energy_supplier_id,
    #             )
    #         )
    #         df = metering_point_time_series_factory.create(spark, data_spec)
    #         dataframes.append(df)

    return spark.union(dataframes)

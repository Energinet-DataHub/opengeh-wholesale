class AddMeteringPointPeriodsToBucketStep(CalculationLink):

    def __init__(
        self, metering_point_period_repository: IMeteringPointPeriodRepository
    ):
        super().__init__()
        self.metering_point_period_repository = metering_point_period_repository

    def handle(
        self, bucket: CacheBucket, output: CalculationOutput
    ) -> [CacheBucket, CalculationOutput]:
        metering_point_periods = self.metering_point_period_repository.get_by(
            bucket.calculator_args.calculation_grid_areas
        )

        metering_point_periods = metering_point_periods.select(
            Colname.metering_point_id,
            Colname.metering_point_type,
            Colname.calculation_type,
            Colname.settlement_method,
            Colname.grid_area_code,
            Colname.resolution,
            Colname.from_grid_area_code,
            Colname.to_grid_area_code,
            Colname.parent_metering_point_id,
            Colname.energy_supplier_id,
            Colname.balance_responsible_id,
            Colname.from_date,
            Colname.to_date,
        )

        return super().handle(bucket, output)

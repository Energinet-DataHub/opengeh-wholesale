TODO: Delete?

# Settlement Reports

The model supports creating and delivering settlement reports to the market actors.

The model is in the database `settlement_report`.

## metering_point_periods_v1

Master data of metering points included in the calculation.

| Column name | NOT NULL | Constraints |
|-|-|-|
| calculation_id | :heavy_check_mark: | See [here](columns.md) |
| metering_point_id | :heavy_check_mark: | See [here](columns.md) |

## metering_point_time_series_v1

The time series points for each metering point.

| Column name | NOT NULL | Constraints |
|-|-|-|
| calculation_id | :heavy_check_mark: | See [here](columns.md) |
| metering_point_id | :heavy_check_mark: | See [here](columns.md) |

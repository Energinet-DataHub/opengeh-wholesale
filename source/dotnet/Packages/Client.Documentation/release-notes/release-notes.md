# Wholesale Client Package Release notes

## Version 5.0.25

No functional changes

## Version 5.0.24

No functional changes

## Version 5.0.23

No functional changes

## Version 5.0.22

No functional changes

## Version 5.0.21

No functional changes

## Version 5.0.20

No functional changes

## Version 5.0.19

No functional changes

## Version 5.0.18

No functional changes

## Version 5.0.17

Added `ProcessType` to `BatchDtoV2`

## Version 5.0.16

No functional changes

## Version 5.0.15

No functional changes

## Version 5.0.14

Added aggregation process type to contract.

## Version 5.0.13

No functional changes

## Version 5.0.12

Added v3 client registration.

## Version 5.0.11

Added a method to get settlement reports by batch id and grid area code.

## Version 5.0.10

Functionality to also filter by period, grid areas and execution state in batch searches has been added.

## Version 5.0.9

Breaking bug fix. `ProcessStepResultDto` now has a `TimeSeriesType` property instead of a `ProcessStepMeteringPointType` property.
The latter type has been removed.

## Version 5.0.8

Added a new `WholesaleClient.GetProcessStepResultAsync(ProcessStepResultRequestDtoV3)`

## Version 5.0.7

No functional changes.

## Version 5.0.6

No functional changes.

## Version 5.0.5

- Bump version as part of pipeline change.

## Version 5.0.4

Expose `GetProcessStepActorsAsync`

## Version 5.0.3

Added `GetProcessStepActorsAsync`

## Version 5.0.2

Rename `ProcessProcessResultRequestDto` to `ProcessStepResultRequestDtoV2`

## Version 5.0.1

No functional changes.

## Version 5.0.0

Namespaces has changed.

## Version 4.2.2

Bump version as part of pipeline change.

## Version 4.2.1

Remove unused types.

## Version 4.2.0

Add quality to process time series type results. See `TimeSeriesPointDto.Quality`.

## Version 4.1.1

Internal refactoring. No changed behavior.

## Version 4.1.0

Changes to `BatchDtoV2`.

## Version 4.0.0

Renamed the `MeteringPointType` to `ProcessStepMeteringPointType`.

## Version 3.4.0

Return batch ID from `WholesaleClient.CreateBatchAsync()`

## Version 3.3.0

Added a new `WholesaleClient.GetProcessStepResultAsync()` which returns a result for a step for a given batch grid area.

## Version 3.2.0

Added a new `WholesaleClient.GetBatchAsync(batchId)` which returns a single batch

## Version 3.1.0

Added `ProcessDto`, `ProcessSteopDto`, `ProcessStepResultDto`, `TimeSeriesPointDto`, `ProcessStep` and `MeteringPointType` to help navigation and presentation of results.

## Version 3.0.1

Changed `namespace` for files placed in `\Contracts` to better reflect they are placed in contracts, and not in `\Application`.

## Version 3.0.0

Rename `BatchGridAreaDto[] BatchGridAreas` to `GridAreaDto[] GridAreas`.

## Version 2.3.0

Added `BatchGridAreaDto[] BatchGridAreas` to `BatchDtoV2`. `BatchGridAreaDto` represents a grid area in the batch.

## Version 2.2.0

Added `IsBasisDataDownloadAvailable` a `boolean` that indicates if basis data is available for download.

## Version 2.1.0

Added `GetZippedBasisDataStreamAsync` that makes it possible to get the basis data for a specific batch as a stream.

## Version 2.0.0

BatchNumber changed form long type to Guid.

## Version 1.0.0

Necessary functionality to replace the current backend communication from the BFF to the wholesale domain.

## Version 0.0.2

Bump version as part of pipeline change.

## Version 0.0.1

Initial release with no functionality.

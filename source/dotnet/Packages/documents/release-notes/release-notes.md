# Wholesale Client Package Release notes

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

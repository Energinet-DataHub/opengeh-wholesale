# Wholesale Contracts Release notes

## Version 7.0.0

Updated .NET framework to .NET 8.

## Version 6.2.0

Changed `CalculationResultVersion` to type int64.

## Version 6.1.0

Added `CalculationResultVersion` property to `EnergyResultProducedV2`

## Version 6.0.2

No changes.

## Version 6.0.1

No changes.

## Version 6.0.0

- Use `Common.DecimalValue` for `AmountPerChargeResultProducedV1` and `MonthlyAmountPerChargeResultProducedV1`. This is a breaking change because the namespace has changes in these two events.

## Version 5.4.0

- Added `EnergyResultProducedV2` with `EventMinorVersion=0`. We also deleted `EnergyResultProducedV1`, but as it was never used by anyone we only bump our minor version of the package.

## Version 5.3.0

- Updated `AmountPerChargeResultProducedV1`: `quantity` is not nullable. Bumped `EventMinorVersion` to `3`.

## Version 5.2.0

- Bump versions of NuGet package dependencies.

## Version 5.1.0

Added 'Currency' to 'AmountPerChargeResultProducedV1' and 'MonthlyAmountPerChargeResultProducedV1

## Version 5.0.0

Updated produced events to use an interface `IEventMessage` for exposing `EventName` and `EventMinorVersion`.
Added integration event types `AmountPerChargeResultProducedV1` and `MonthlyAmountPerChargeResultProducedV1`.

## Version 4.4.1

Update realease-notes

## Version 4.4.0

Changed ProcessType to CalculationType in EnergyResultProducedV1

## Version 4.3.0

Use nested types in Added 'energy_result_produced_v1.proto'

## Version 4.2.0

Added 'EnergyResultProducedV1'  as a new event

## Version 4.1.0

Add wholesale process types to process type enum of the integration event contract.

## Version 4.0.1

No functional changes.

## Version 4.0.0

Renamed `CalculationResultCompleted.MessageType` to `CalculationResultCompleted.MessageName` in order to align with ADR-008.

Added `CalculationResultCompleted.MessageVersion`.

## Version 3.1.0

Added nullable property `CalculationResultCompleted.FromGridAreaCode`.

## Version 3.0.1

No functional changes.

## Version 3.0.0

Renamed BalanceFixingEventName to MessageType and its value from BalanceFixingCalculationResultCompleted to CalculationResultCompleted.

## Version 2.2.10

No functional changes.

## Version 2.2.9

No functional changes.

## Version 2.2.8

No functional changes.

## Version 2.2.7

No functional changes.

## Version 2.2.6

No functional changes.

## Version 2.2.5

No functional changes.

## Version 2.2.4

No functional changes.

## Version 2.2.3

No functional changes.

## Version 2.2.2

No functional changes.

## Version 2.2.1

No functional changes.

## Version 2.2.0

Removed ProcessCompleted.

## Version 2.1.6

No functional changes.

## Version 2.1.5

removed `AggregationEventName` in `CalculationResultCompleted.cs`

## Version 2.1.4

Partial class for `CalculationResultCompleted` for `messagetype` names.

## Version 2.1.3

No functional changes.

## Version 2.1.2

No functional changes.

## Version 2.1.1

A few fixes to the contract `CalculationResultCompleted`.

## Version 2.1.0

Add integration event contract `CalculationResultCompleted`.

## Version 2.0.14

No functional changes.

## Version 2.0.13

No functional changes.

## Version 2.0.12

No functional changes.

## Version 2.0.11

No functional changes.

## Version 2.0.10

No functional changes.

## Version 2.0.9

No functional changes.

## Version 2.0.6

No functional changes.

## Version 2.0.5

- Bump version as part of pipeline change.

## Version 2.0.4

No functional changes.

## Version 2.0.3

Add `BatchActorDto`, `BatchActorRequestDto` and `MarketRoleType`

## Version 2.0.2

Rename `ProcessProcessResultRequestDto` to `ProcessStepResultRequestDtoV2`

## Version 2.0.1

No functional changes.

## Version 2.0.0

Namespaces has changed.

## Version 1.0.3

Bump version as part of pipeline change.

## Version 1.0.2

No functional changes.

## Version 1.0.1

Administrative release. No changes.

## Version 1.0.0

Add `ProcessCompleted` event type.

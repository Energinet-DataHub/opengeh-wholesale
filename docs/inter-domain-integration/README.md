# Inter Domain Integration

In order to better understand the reasoning behind the contracts the following elements are important to understand.

- UI requirements doesn't necessary require data to be present in the domain. For example if the UI must show a grid area name
should be solved by the UI or BFF fetching the grid area in question from the domain owning grid areas.
- Future may diverge in any yet unknown direction. There may come separate metering point and charges domains and more.
These may publish data in a wide range of ways that we don't know yet. However, we strive to identify the most proper
contracts for supporting and optimizing the calculations regarding performance and complexity. So what is likely to
change is not the defined Delta tables but _how_ they are populated.
- The domain is not responsible for B2B messaging. The responsibility is
delegated to the EDI domain.

The consequences might be surprising but currently it seems as there are no
need to acquire:

- Actors and roles
- Organisations
- Users
- Grid areas
- Delegations

Other consequence is that the contracts should not focus on all possible future context map but rather on the "well-known" requirements of the calculation engine.

The contracts are designed with the anticipation that it is safest to support audit and providing basis data by storing "snapshots". "Snapshots" is the idea that calculation input is captured and stored when (or before) a calculation starts. This is contrast to relying on data sources never being modified or tampered, and never exhibit race conditions regarding data writes at the same time as a calculation is started.

## Contracts

The contracts defines data owned by the wholesale domain and the Delta tables will thus be located in the wholesale domain.
For now the population of the tables is outsourced to the migration domain. That will, however, change in the future
when the upstream sources changes.

The following contracts are used in both balance fixing and settlement:

- [metering_point_period_schema](metering-point-period-schema.py)
- [time_series_point_schema](time-series-point-schema.py)

Please note that it may be desirable to move system correction and grid loss metering points to the metering point periods table.

The following contracts are used only in settlement:

- [imbalance_price_schema](imbalance-price-schema.py)
- [charge_period_schema](charge-link-period-schema.py)
- [charge_price_point_schema](charge-price-point-schema.py)

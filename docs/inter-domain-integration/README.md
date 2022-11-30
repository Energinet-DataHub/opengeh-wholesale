# Inter Domain Integration

In order to better understand the reasoning behind the contracts the following elements are important to understand.

- UI requirements doesn't necessary require data to be present in the domain. For example if the UI must show a grid area name
should be solved by the UI or BFF fetching the grid area in question from the domain owning grid areas.
- Future may diverge in any yet unknow direction. There may come separate metering point and charges domains and more.
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

Other consequense is that the contracts should not focus on all possible future context map but rather on the "well-known" requirements of the calculation engine.

## TODOs

- provide examples for all columns
- optimize storage by using smaller types?
- SCMP+GLMP
- EDI integration
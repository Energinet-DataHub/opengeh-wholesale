# Contracts

This folder contains internal contracts in the `/internal` folder.
They are used to ensure conformance between the .NET services and the `pyspark`
services where sharing of types by e.g. file linking is not possible.

The `/{domain-name}-domain` folders like `/metering-point-domain` contain contracts defined by the specified domain.

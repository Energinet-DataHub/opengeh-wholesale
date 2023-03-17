# Wholesale Contracts Documentation

A package containing contracts for the wholesale domain.

## `ProcessCompleted` Event

`ProcessCompleted` is a .NET class generated from a protobuf contract.
The type has methods to support serialization and deserialization.

A process completed event is published whenever a process has completed.

A process may be any of a set of process types. These types include _balance fixing_, _settlement_ and more.
A process is uniquely identified by the batch ID and the grid area code.

# Wholesale Public Data Models

## About the Data

It is anticipated that the reader/developer has a basic understanding of calculations.
This section describes a few aspects that may be important for developers to be aware of.

### The 1sts of May 2023

The 1st of May 2023 is a cut-off date. All energy calculations executed before creates results with an hourly resolution. All energy calculations executed after creates results with a quarterly resolution. All wholesale calculation results are always with an hourly resolution.

### Completion of the Business Process

It is important to distinquish between whether the calculation is completed or the whole business process (BRS-023/BRS-027) is complete.

When the calculation is complete the RSM-014/RSM-019 messages for the market actors are created and can be peeked/dequeued. It is, however, important to be aware that the results cannot be accessed in any other way until the whole process is completed. The process is completed when all actor messages are ready. Only then can the data be accessed in other ways like requests (BRS-027) and download of settlement reports and more.

### Calculation vs Process Related Data

There are basically two ways of accessing data.

One is by calculation. This allows to access data related to a specific calculation. This applies to e.g. settlement reports.

The other way is by the business process. When you don't care from which calculation the data origins but care about the process (e.g. balance fixing and the period) then you'll be reading the _latest_ data created by all calculations of the given process type within the specified period.

## Data Models

- [Settlement reports](settlement-reports.md)

The complete list of column descriptions is [here](columns.md).

## Naming

A data model is located in a Databricks database/schema named according to the intended purpose of the model. An example is `settlement_report`.

## Versioning

Each data source of the model has its own versioning. The version is a major version. See semver.org for more on version principles.

Non-breaking changes are delivered in place. This is similar to the Google API versioning principles.

These changes are considered non-breaking and are delivered in-place:

- Adding a column
- Changing a column nullability from nullable to non-nullable
- Reducing the range of possible values in a column

These changes are consided breaking and will be delivered as a new version of the data source:

- Renaming a column
- Changing the type of a column
- Changing a column nullability from non-nullable to nullable
- Extending the range of possible values in a column

Currently there is no specific retention policy. The team will work with the consumers of the public data models to negotiate when old versions can be removed.

## How to Access the Data Models

TBW

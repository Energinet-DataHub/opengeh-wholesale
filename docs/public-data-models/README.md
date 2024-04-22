# Wholesale Public Data Models

## Data Models

- [Settlement reports](settlement-reports.md)

The complete list of column descriptions is [here](.columns.md).

## Data Model Principles

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

## How to Access

TBW

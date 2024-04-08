# Tests Description

A standard scenario is a scenario that is defined as follows:

```gherkin
Given an energy calculation
When there is a production metering point
    And a consumption metering point
    And a grid loss metering point
    And an exchange metering point
    And a time series of energy values of the production and consumption metering points
Then the actual output equals the expected output
```

What has to be asserted in the "then-clause" is inferred from the files in the folders
`basis_data` and `energy_results`.

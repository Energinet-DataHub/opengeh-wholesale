# Feature Test Framework

Feature tests check individual calculations performed by the calculation engine. It uses synthetic test data in
csv-files as input and checks against an expected outcome - also in csv-files for each calculation checked. The test
span is from actual calculation execution to the resulting output. If basis-data expected results are present in the
output folder, these are also checked.

## How to add a feature test

1. Generate the readme file (md). Remember to format the file correct as the CI check is strict.
2. Generate the various data input files (csv)
3. Generate the expected results file (csv)
4. Generate the calculation arguments file (yml)

- Create a “given_” folder with the scenario.
- Input files go into a "When" subfolder
- Expected results go into a "Then" subfolder

Example structure:

- features
    - energy_calculation
        - given_exchange_sends_to_own_ga
            - When
                - Input csv-files
            - Then
                - Expected results csv-files

A useful starting point for creating an energy logic test is the test when_minimal_standard_scenario which contains all
result files and result basis data. Make a copy of this folder and modify metering_point_periods.csv,
time_series_points.csv and the desired result output files to create the scenario you wish to check.

## Formatting Rules

- Columns are separated with “;”. This applies also for headers.
- The data format is åååå-mm-dd hh:mm:ss fx. 2020-12-03 13:00:00.
- Decimals use period fx. 10.090
- Elements in lists are separated with “,” fx. ['measured', 'calculated']
- Decimal number are with 3 decimals in the results

## Coverage - all_test_cases.py and Coverage.py

The class coverage/all_test_cases.py contains all test-cases that we want to test in the feature tests.

The cases listed in this file can be referenced from Coverage.py files in scenarios. Under the "Cases Tested" heading, list the testcases covered by the scenario as bullet items. These will be scanned as part
of coverage overview.

**All test scenarios must have a Coverage.py file**. Easiest is to copy and modify an existing Coverage-file from another scenario.

See also [Confluece](https://energinet.atlassian.net/wiki/spaces/D3/pages/1081999392/Coverage).

## Local test config settings

Make a copy of [test.local.settings.sample.yml](..%2Ftest.local.settings.sample.yml) and remove ".sample" from the file
name. This is your local config settings where you set various settings that affect the log output. See file for
descriptions.

## Oracle Excel-sheet

- In the standard scenario for both energy and wholesale tests there is an Excel-sheet which was used to generate
  expected results. Example in
  when_minimal_standard_scenario test. It can be used to help generate the expected results if input data is complex
  enough to make calculating results
  manually tedious.
- It is not mandatory to supply an oracle Excel-sheet. Use it if it’s useful for you. The sheet can generate results for
  all energy calculations. Update the white cells with data matching exactly the data
  in your metering_point_periods.csv and time_series_points.csv input files. Update white cells ONLY though, or the
  sheet
  logic can break.
- Use the dropdowns in cells B2 and B3 to change between calculation types.
- The sheet can only generate results for one hour and there is a limitation for how many metering points, energy
  suppliers, balance responsibles etc. it can handle. Make a copies of the first tab if you need results for more hours.
- Use with caution. If there is a mismatch between sheet and actual results, the problem is most likely that the data in
  the sheet and in metering_point_periods.csv and time_series_points.csv do not match completely.

## Additional notes

- The rows in expected result files do not have to be listed in exactly the same order as the actual result generated.
  This is both a curse and a blessing. The latter because it makes the framework a bit more forgiving about results
  looking exactly the generated actual results. But also annoying because it can make it difficult to locate exactly
  which
  rows have a problem - especially if there are hundreds of results rows (e.g. if calculation period is more than one
  hour).
- The framework doesn’t handle very well if cells are not which it doesn’t expected to be null. In many cases it will
  not
  even generate result logs if that is the case.

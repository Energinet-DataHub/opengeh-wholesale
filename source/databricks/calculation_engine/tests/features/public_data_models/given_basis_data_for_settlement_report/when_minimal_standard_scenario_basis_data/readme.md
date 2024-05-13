# When Minimal Standard Scenario

The purpose of this test is to test views for settlement reports.

## Design considerations

- The input basis data is taken from the calculation logic test
  features/given_an_energy_calculation/when_minimal_standard_scenario/output/basis_data. However, one extra row with a different calculation_id is added to the basis data. This calculation_id is not part of the 'calculations' table. The purpose is to test that the view does not include this row.
- Details in the readme.md for that test

## Coverage

All metering point types relevant for energy calculations

- E20 (exchange energy in)
- E20 (exchange energy out)
- E17 (flex)
- E17 (nonprofiled)
- E17 (glmp)
- E18
- E18 (skmp)

Each of the metering point types both have a metering point with resolution 15M and 1H.

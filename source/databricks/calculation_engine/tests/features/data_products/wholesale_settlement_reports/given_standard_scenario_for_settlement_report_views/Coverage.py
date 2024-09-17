from test_coverage.all_test_cases import Cases

"""
The purpose is checking all current views used for settlement reports.

The input is taken from features/given_a_wholesale_calculation/when_minimal_standard_scenario. See the readme-file for
that test so see makeup of input data. But the intent is that it should be a small set that still provides a broad
coverage of metering point types and subscriptions, fees and tariffs.

## Design considerations

- The input basis data is taken from the test
  features/given_a_wholesale_calculation/when_minimal_standard_scenario. See the readme-file for that test so see makeup of input data. But the intent is that it should be a small set that.
  still provides a broad coverage of metering point types and subscriptions, fees and tariffs.

Two small corner cases are included:
- Verify that multiple metering point periods with the same id, metering point type, grid area code, calculation id and energy supplier id (but period start/stop) only have one entry in charge prices.
- One extra row with a different calculation_id is added to the basis data. This calculation_id is not part of the 'calculations' table. The purpose is
  to test that the view does not include this row.

## CASES TESTED
"""
Cases.CalculationTests.Typical_wholesale_scenario
Cases.SettlementReportsTests.Calculation_versions_different
Cases.SettlementReportsTests.MP_with_same_masterdata_but_different_to_and_from_only_one_entry_in_charge_prices

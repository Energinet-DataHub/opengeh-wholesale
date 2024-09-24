from coverage.all_test_cases.py import Cases

"""
## PURPOSE
The purpose is to test that we can properly withdraw energy results based on changes in metering_point_periods table between test runs.

## DESIGN CONSIDERATIONS
- Input data for 1st run is based on minimal standard energy scenario, except that grid area 111 is included in the calculation.
- Changes between 1st and 2nd run:
  - An MP changes energy supplier
  - An MP changes energy supplier and balance responsible
  - An MP is closed down
- In the 2nd run, the changes are propagated to results where the appropriate result rows are marked as withdrawn based on the changes in combinations of input parameters

## CASES TESTED
"""
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_mp_changes_es
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_mp_changes_es_and_brp
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_mp_changes_settlement_method
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_mp_is_closed_down
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_combination_previously_missing_reintroduced
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_time_series_removed_which_were_previously_withdrawn

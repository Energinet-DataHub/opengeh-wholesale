from coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose is to test that we can properly withdraw wholesale results based on changes in metering_point_periods, charge_price_information_periods and charge_link_periods tables between test runs.

## DESIGN CONSIDERATIONS
- Input data for 1st run is based on minimal standard wholesale scenario, except that grid area 111 is included in the calculation.
- Changes between 1st and 2nd run:
  - An MP changes energy supplier
  - An MP changes settlement method
  - A child MP ends in calculation period, but charge link not updated
  - A price element is removed
  - An MP is closed down
- In the 2nd run, the changes are propagated to results where the appropriate result rows are marked as withdrawn based on the changes in combinations of input parameters

## CASES TESTED
"""
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_mp_changes_es
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_mp_changes_settlement_method
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_child_mp_relation_to_parent_ends_in_period_but_chargelink_not_updated
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_price_element_disappears_but_chargelink_remains
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_price_element_disappears

from coverage.all_test_cases import Cases

"""
## Purpose
The purpose is checking the latest calculation by day and grid area.

## DESIGN CONSIDERATIONS

GA      | Type              | Day 1     | Day 2     | Day 3     |
---------------------------------------------------------
                                calculation_id: 1
001     | aggregation       |-----------------------|

                                calculation_id: 2
001     | balance_fixing    |-----------------------|

                                calculation_id: 2
002     | balance_fixing    |-----------------------|

                                calculation_id: 3
001     | balance_fixing    |-----------------------|

                                calculation_id: 4
001     | balance_fixing                |-----------------------|

                                calculation_id: 5 (calculation not succeeded)
001     | balance_fixing    |-----------------------|


## CASES TESTED
"""
Cases.DataProductTests.Latest_calculation_by_day_splits_period_in_correct_amount_of_days
Cases.DataProductTests.Latest_calculation_by_day_sets_correct_latest_calculation_when_periods_overlap
Cases.DataProductTests.Latest_calculation_by_day_includes_correct_calculation_ids_in_output
Cases.DataProductTests.Latest_calculation_by_day_only_includes_succeeded_external_calculations

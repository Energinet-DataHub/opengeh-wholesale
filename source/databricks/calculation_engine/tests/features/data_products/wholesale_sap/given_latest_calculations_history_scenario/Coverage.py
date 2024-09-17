from coverage.all_test_cases import Cases

"""
## Purpose
The purpose is checking the latest calculation history view used for SAP.

## DESIGN CONSIDERATIONS

GA      | Type              | Day 1     | Day 2     | Day 3     |
---------------------------------------------------------
                                calculation_id: 1
001     | balance_fixing    |-----------------------|

                                calculation_id: 1
002     | balance_fixing    |-----------------------|

                                calculation_id: 2
001     | balance_fixing    |-----------------------|

                                calculation_id: 3
001     | balance_fixing                |-----------------------|

                                calculation_id: 4 (calculation id not in grid areas table)
001     | balance_fixing    |-----------------------|

                                calculation_id: 5
001     | aggregation       |-----------------------|

## CASES TESTED
"""
Cases.DataProductTests.SapResultsTests.Correct_calculation_ids_included_in_output
Cases.DataProductTests.SapResultsTests.Calculation_history_splits_period_in_correct_amount_of_days
Cases.DataProductTests.SapResultsTests.Calculation_history_sets_correct_latest_calculation_when_periods_overlap

from coverage.all_test_cases import Cases

"""
## Purpose
The purpose is checking views related to amount per charge view.

## DESIGN CONSIDERATIONS
- wholesale_internal.calculations table has three calculation_id's, only one of which is valid.
- The two others are either internal or doesn't have an end date.

## CASES TESTED
"""
Cases.DataProductTests.WholesaleResultsTests.Only_calculation_ids_in_internal_calculations_included
Cases.DataProductTests.WholesaleResultsTests.Only_external_calculation_ids_included
Cases.DataProductTests.WholesaleResultsTests.Calculation_ids_without_calculation_succeeded_time_not_included

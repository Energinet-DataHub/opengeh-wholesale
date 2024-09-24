from coverage.all_test_cases import Cases

"""
## Purpose
The purpose is checking views related to amount per charge view.

## DESIGN CONSIDERATIONS
- wholesale_internal.calculations table has three calculation_id's, only one of which is valid.
- The two others are either internal or doesn't have an end date.
- These three ids are used across all 6 basis_data tables, with only 1 being filtered for the data product.

## CASES TESTED
"""
Cases.DataProductTests.WholesaleBasisDataTests.Only_calculation_ids_in_internal_calculations_included
Cases.DataProductTests.WholesaleBasisDataTests.Only_external_calculation_ids_included
Cases.DataProductTests.WholesaleBasisDataTests.Calculation_ids_without_calculation_succeeded_time_not_included

from test_coverage.all_test_cases import Tests

"""
## Purpose
The purpose is checking the latest calculation history view used for SAP.

## DESIGN CONSIDERATIONS ## 
- The test has 5 calculations where each calculation covers 1 or 2 grid area codes.
- The 5 calculations is a mix of internal and external.
- The 5 calculations have a mix of succeeded and not succeeded.

## CASES TESTED ##
"""
Tests.SettlementReportsTests.Calculation_versions_different
Tests.SettlementReportsTests.Calculation_types_different

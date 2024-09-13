from coverage.all_test_cases import Cases

"""
## Purpose
The purpose is checking the latest calculation history view used for SAP.

## DESIGN CONSIDERATIONS
- The test has 5 calculations where each calculation covers 1 or 2 grid area codes.
- The 5 calculations is a mix of internal and external.
- The 5 calculations have a mix of succeeded and not succeeded.

## CASES TESTED
"""
Cases.SettlementReportsTests.Calculation_versions_different
Cases.SettlementReportsTests.Calculation_types_different

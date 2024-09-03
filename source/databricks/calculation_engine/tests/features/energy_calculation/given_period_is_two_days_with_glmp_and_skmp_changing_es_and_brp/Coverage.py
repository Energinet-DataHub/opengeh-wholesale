from test_coverage.all_test_cases import Tests

"""
## PURPOSE ##
The purpose is to test that grid loss and system correction metering points can change energy supplier and balance
responsible.

## DESIGN CONSIDERATIONS ## 
- Input period is post May 2023 so that results are quarterly
- Input period is two days with change of energy supplier/balance responsible on the grid loss and system correction
  metering points between the two days
- Quantities are set so that both grid loss and system correction metering point have values both days

## CASES TESTED ##
"""
Tests.CalculationTests.MeteringPointMasterDataUpdates.Energy_Supplier_change_on_Grid_Loss_MP
Tests.CalculationTests.MeteringPointMasterDataUpdates.Energy_Supplier_change_on_System_Correction_MP
Tests.CalculationTests.MeteringPointMasterDataUpdates.Balance_Responsible_change_on_Grid_Loss_MP
Tests.CalculationTests.MeteringPointMasterDataUpdates.Balance_Responsible_change_on_System_Correction_MP

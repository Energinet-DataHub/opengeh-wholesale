from test_coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose is to test every resolution (from 1H to 15M), es and brp change for metering point types relevant for energy
results.

## DESIGN CONSIDERATIONS
- Input period is post May 2023 so that results are quarterly
- Metering point id's denote the type
    - Production metering points start with e.g. '18' for production metering points
    - Metering points with resolution 15M have a 15 in them
    - Nonprofiled metering points end with 1xx, flex with 2xx
    - Example - flex metering point with resolution 15M: '**17**000000**15**00000**2**01'

## CASES TESTED
"""
Cases.CalculationTests.MeteringPointMasterDataUpdates.Change_of_balance_responsible_on_an_MP
Cases.CalculationTests.MeteringPointMasterDataUpdates.Change_of_energy_supplier_on_an_MP
Cases.CalculationTests.MeteringPointMasterDataUpdates.Change_of_resolution_on_an_MP
Cases.CalculationTests.MeteringPointMasterDataUpdates.Energy_Supplier_change_on_Grid_Loss_MP
Cases.CalculationTests.MeteringPointMasterDataUpdates.Energy_Supplier_change_on_System_Correction_MP
Cases.CalculationTests.MeteringPointMasterDataUpdates.Balance_Responsible_change_on_Grid_Loss_MP
Cases.CalculationTests.MeteringPointMasterDataUpdates.Balance_Responsible_change_on_System_Correction_MP

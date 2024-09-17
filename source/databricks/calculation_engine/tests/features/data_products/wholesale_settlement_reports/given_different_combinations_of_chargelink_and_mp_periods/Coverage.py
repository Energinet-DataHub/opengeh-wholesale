from test_coverage.all_test_cases import Cases

"""
## PURPOSE ##
The purpose is to test a calculation on a period before the transition to quarterly results.

## DESIGN CONSIDERATIONS
- Period is before the calculation result resolution change
- Input data has two of each production MP, a consumption MP of both types of settlement methods and exchange MP - one with resolution 15M and one with resolution 1H
- Time series for each MP

 1. Missing metering point period.
      2023-02-02                     2023-02-10
   MMP      MISSING
   CLP      |-----------------------------|
   CPIP     |-----------------------------|
   EXPECTED NONE


2. Missing charge link period.
       2023-02-02                    2023-02-10
   MMP      |-----------------------------|
   CLP      MISSING
   CPIP     |-----------------------------|
   EXPECTED NONE


3. Standard example.
       2023-02-02                    2023-02-10
   MMP      |-----------------------------|
   CLP      |-----------------------------|
   CPIP     |-----------------------------|
   EXPECTED |-----------------------------|


4. Metering point period and charge link period overlap.
       2023-02-02                    2023-02-10
   MMP      |-----------------------------|
            2023-02-06                       2023-02-14
   CLP          |---------------------------------|
   CPIP     |-----------------------------|
   EXPECTED     |-------------------------|


5. Metering point period and charge link period overlap.
                      2023-02-02                  2023-02-10
   MMP                     |---------------------------|
       2023-01-25                   2023-02-05
   CLP      |----------------------------|
   CPIP     |----------------------------|
   EXPECTED                |-------------|


6. Metering point period and charge link period do not overlap.
                          2023-02-02               2023-02-10
    MMP                         |------------------------|
        2023-01-25    2023-01-28
    CLP     |--------------|
    CPIP                        |------------------------|
    EXPECTED NONE


7. Metering point period and charge link period do not overlap.
        2023-02-02              2023-02-10
    MMP     |------------------------|
                                        2023-02-12    2023-02-28
    CLP                                     |--------------|
    CPIP    |------------------------|
    EXPECTED NONE


8. Metering point period is a subset of charge link period.
                2023-02-02              2023-02-10
    MMP             |------------------------|
        2023-01-25                                  2023-02-15
    CLP     |-------------------------------------------|
    CPIP            |------------------------|
    EXPECTED        |------------------------|


9. Charge link period is a subset of the metering point period.
       2023-02-02               2023-02-10
    MMP     |------------------------|
           2023-02-04       2023-02-08
    CLP         |----------------|
    CPIP    |------------------------|
    EXPECTED    |----------------|


10. Two metering point periods due to a change of energy supplier results.
           023-02-02           2023-02-10            2023-02-18
    MMP         |-------------------|----------------------|
    CLP         |------------------------------------------|
    CPIP        |------------------------------------------|
    EXPECTED    |-------------------|----------------------|


11. Charge link periods have a gap.
       2023-02-02                                 2023-02-18
    MMP     |------------------------------------------|
           2023-02-04  2023-02-06  2023-02-08 2023-02-10
    CLP          |-----------|        |-----------|
    CPIP    |------------------------------------------|
    EXPECTED     |-----------|        |-----------|


12. Metering point periods are overlapping a charge link period.
        2023-02-02      2023-02-06   2023-02-08     2023-02-12
    MMP     |----------------|          |----------------|
                2023-02-04                  2023-02-10
    CLP             |---------------------------|
    CPIP    |----------------------------------------------|
    EXPECTED        |--------|          |-------|


13. Two charge link periods.
            2023-02-02                      2023-02-10
    MMP         |--------------------------------|
                            2023-02-04
    CLP         |----------------|---------------|
    CPIP        |--------------------------------|
    EXPECTED    |--------------------------------|


14. Charge link period between metering point periods.
        2023-02-02      2023-02-04    2023-02-08        2023-02-10
    MMP     |----------------|            |----------------|
    CLP                      |------------|
    CPIP    |----------------------------------------------|
    EXPECTED NONE


15. Multiple metering point periods due to for example changing of energy supplier.
           2023-02-02      2023-02-04      2023-02-08       2023-02-12
    MMP         |----------------|--------------|----------------|
    CLP         |------------------------------------------------|
    CPIP        |------------------------------------------------|
    EXPECTED    |----------------|--------------|----------------|


16. Two charge price information periods.
            2023-02-02      2023-02-10
    MMP         |----------------|
    CLP         |----------------|
                    2023-02-05
    CPIP        |-------|--------|
    EXPECTED    |----------------|


17. Two metering point periods due to a change of balance responsible.
           2023-02-02       2023-02-06           2023-02-10
    MMP         |-----------------|-------------------|
    CLP         |-------------------------------------|
    CPIP        |-------------------------------------|
    EXPECTED    |-----------------|-------------------|

## CASES TESTED ##
"""
Cases.SettlementReportsTests.MP_period_missing
Cases.SettlementReportsTests.Charge_link_period_missing
Cases.SettlementReportsTests.MP_period_and_charge_link_period_overlap
Cases.SettlementReportsTests.MP_period_and_charge_link_period_do_not_overlap
Cases.SettlementReportsTests.Charge_link_period_is_a_subset_of_the_MP_period
Cases.SettlementReportsTests.MP_period_subset_of_charge_link_period
Cases.SettlementReportsTests.MP_period_multiple
Cases.SettlementReportsTests.Charge_link_periods_have_a_gap
Cases.SettlementReportsTests.MP_period_and_charge_link_period_overlap
Cases.SettlementReportsTests.Charge_link_periods_multiple
Cases.SettlementReportsTests.Charge_link_period_between_MP_periods
Cases.SettlementReportsTests.Charge_price_information_periods_multiple

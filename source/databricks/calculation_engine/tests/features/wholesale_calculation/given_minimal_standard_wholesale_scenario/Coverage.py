from test_coverage.all_test_cases import Cases

"""
## PURPOSE
The purpose of this test is to check a minimal, but representative sample of all wholesale calculation results.

Fees
- Charge Masterdata
    - 3 fees, start date before calculation period, no end date.

- Charge Link Periods
    - Fees with quantity > 1.
    - Both a parent and a child MP has a fee.
    - A fee that ends before end of calculation period.
    - A fee that starts before end of calculation period, but not at the beginning of the calculation period.

- Charge Price
    - A fee charge in calculation period.
    - A fee charge link in calculation period, but no charge price for that period (price and amount = null).
    - A fee charge outside calculation period (no rows in results).

Subscriptions
- Charge Masterdata
    - 3 subscriptions, start date before calculation period, no end date.

- Charge Link Periods
    - Subscription with quantity > 1.
    - Both a parent and a child MP has a subscription.
    - A subscription that ends before end of calculation period.
    - A subscription that starts before end of calculation period, but not at beginning.

- Charge Price
    - Calculation period is covered by all subscription charge price periods.
    - Includes a price point to check correct rounding.

Hourly Tariffs
- Charge Masterdata
    - 4 tariffs, 1 of which has charge owner System Operator and is_tax = true.
    - All start dates before calculation period, no end date.

- Energy calculations
    - Energy time series for 1 hour.
    - That hour has positive grid loss for 3 quarters and negative grid loss for 1 quarter.

- Charge Link Periods
    - Both a parent and a child MP has a tariff.
    - Grid Loss MP has has a tariff.
    - System Correction MP has a tariff.
    - A tariff that ends before end of calculation period.
    - A tariff that starts before end of calculation period, but not at beginning.

- Charge Price
    - The charge price times match up with the energy time series (02.00 on the first day of the calculation period).

Daily Tariffs
- Charge Masterdata
    - 4 tariffs, 2 of which has charge owner System Operator, 1 of these is_tax = true and 1 of these is_tax = false.
    - All start dates before calculation period, no end date.

- Charge Link Periods
    - Both a parent and a child MP has a tariff.
    - Grid Loss MP has has a tariff.
    - System Correction MP has a tariff.
    - A tariff that ends before end of calculation period.
    - A tariff that starts before end of calculation period, but not at beginning.

## CASES TESTED
"""
Cases.CalculationTests.Typical_wholesale_scenario

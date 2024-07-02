# When Minimal Standard Scenario for wholesale calculations

The purpose of this test is to check a minimal, but representative sample of all wholesale calculation results.

## Coverage

- All types of wholesale calculations - fee, subscription, hourly and daily tariffs - are included.
- A small set of energy time series is included in order to test tariffs.

### Fees

- Charge Masterdata
    - 3 fees, from- and to fields cover whole calculation period.

- Charge Link Periods
    - Fees with quantity > 1
    - Both parent and child MP has a fee

charge price

- in calc period
- in calc period, but not charge price for that period (price and amount = null)
- outside calc period (no rows in results)

fee_per_ga_co_es

Aggregation
charge time (day)
charge-key
MP type

Check that identical aggregations are correctly summed and different aggregations have separate rows

monthly_fee_per_ga_co_es

Aggregation
charge time (month)
charge-key
Check that identical aggregations are correctly summed and different aggregations have separate rows

3 subscriptions, all start date before calculation period

charge masterdata

- from-to covers whole calc. period

charge link periods

- quantity = 1
- quantity > 1
- both a parent and a child MP has a subscription
- a subscription that ends before end of calculation period
- a subscription that starts before end of calculation period, but not at beginning
- a subscritption starting after end of calculation period

charge price

- in calc period
- A price point to check correct rounding

subscription_per_ga_co_es

Aggregation
charge time (day)
charge-key
MP type

Check that identical aggregations are correctly summed and different aggregations have separate rows

monthly_subscription_per_ga_co_es

Aggregation
charge time (month)
charge-key
Check that identical aggregations are correctly summed and different aggregations have separate rows

Hourly Tariffs

- 4 tariffs, one of which is SYO, tax
- Period start before calculation period

Energy calculations

- Energy time series for 1 hour
- Positive grid loss for 3 quarters and negative grid loss for 1 quarter

charge link periods

- Both a parent and a child MP has a tariff
- Grid Loss MP has has a tariff
- System Correction MP has a tariff
- a tariff that ends before end of calculation period
- a tariff that starts before end of calculation period, but not at beginning

Daily Tariffs

- 4 tariffs, 2 of which have SYO as co, on of which is tax

charge links
charge link periods

- Both a parent and a child MP has a tariff
- Grid Loss MP has has a tariff
- System Correction MP has a tariff
- a tariff that ends before end of calculation period
- a tariff that starts before end of calculation period, but not at beginning 
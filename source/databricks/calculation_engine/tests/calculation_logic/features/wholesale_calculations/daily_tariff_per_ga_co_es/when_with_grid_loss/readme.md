# Testing daily tariff per grid area, charge owner and energy supplier with grid loss

Calculation period: February 2023

Grid area: 804

| charge_code | charge_type | charge_owner_id | metering_point_id  | TYPE | from_date  | to_date | Energy supplier | MP type    |
|-------------|-------------|-----------------|--------------------|------|------------|---------|-----------------|------------|
| 41000       | tariff      | 5790001330552   | 571313180400100657 | E17  | 31-01-2023 | 23:00   | 8100000000115   | Grid_loss  |
| 41000       | tariff      | 5790001330552   | 571313180480500149 | E18  | 31-01-2023 | 23:00   | 8100000000108   | System_CMP |

    Positive_grid_los 1-20 February
    MP              kWh MP id
    Production E18  90  571313180400012004
    Consumption E17 75  571313180400140417

    Calculated grid loss 15 Per time
    360 Per dag Pris: 1.756998 Amount: 632.51928

    Negative_grid_loss 20-28 Februar
    MP              kWh MP id
    Production E18  80  571313180400012004
    Consumption E17 90  571313180400140417

    Calculated grid loss -10 Per time
    -240 Per dag Pris: 1.756998 Amount: 421.67952

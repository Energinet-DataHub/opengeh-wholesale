# Calculate flex consumption per grid area and energy supplier

The purpose of this test is to a slightly more complex set for flex_consumption_per_ga_and_es calculation.
Period is two days.
Between day 1 and 2, there is energy supplier change and also resolution change on the metering point 
in the time series.
Test data is completely synthetic.

Period:

            Period_Start                         Period_End
             2023-02-01                          2023-02-03
              23:00:00                            23:00:00
              ---|-----------------------------------|---
        
    MPP1 (E17)   |----------------------------------------------
    MPP2 (E18)   |----------------------------------------------
    MPP3 (E17) -------------------------------------------------

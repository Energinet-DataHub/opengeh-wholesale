# Calculate flex consumption per grid area and energy supplier

The purpose of this test is to create the simplest possible version of a test of flex_consumption_per_ga_and_es. 
Period is just one hour, even though we would always generate data for at least one day.
Test data is completely synthetic.

Period:

            Period_Start                         Period_End
             2023-02-01                          2023-02-01
              00:00:00                            01:00:00
              ---|-----------------------------------|---
        
    MPP1 (E17)   |----------------------------------------------
    MPP2 (E18)   |----------------------------------------------
    MPP3 (E17) -------------------------------------------------

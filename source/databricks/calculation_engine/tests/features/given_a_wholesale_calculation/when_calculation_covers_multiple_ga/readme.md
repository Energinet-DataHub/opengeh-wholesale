# When Calculation Covers Multiple Grid Areas

The purpose of this test is to check calculation results and basis data generation for a calculation that spans multiple grid areas. 

## Coverage

- Included in input are MP master data from three different grid areas.
- Included are time series, charge master data, charge links, charge prices referencing MPs from all three grid areas. 
- Included in calculation are only two of those grid areas. We check that:
  - Only energy and wholesale results for specified grid areas are generated.
  - Only basis data for the specified grid areas are generated. 

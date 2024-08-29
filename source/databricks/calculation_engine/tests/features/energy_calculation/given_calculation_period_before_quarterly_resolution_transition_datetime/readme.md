# Given calculation period is before quarterly transition date

The purpose is to test a calculation on a period before the transition to quarterly results.  

## Design considerations
 
- Period is before the calculation result resolution change
- Input data has two of each production MP, a consumption MP of both types of settlement methods and exchange MP - one with resolution 15M and one with resolution 1H  
- Time series for each MP

## Coverage

- Calculation results are hourly when calculation period is before result resolution change

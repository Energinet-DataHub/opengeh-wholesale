from coverage.all_test_cases import Cases

"""
## PURPOSE

## DESIGN CONSIDERATIONS
First run spans 3 days. Second run spans the same three days plus one more day.

For combination:
- Nonprofile
- ES 3000000000000
- BRP 3300000000000
- GA 800

1st run - Day 1 to 3
  Day 1   Day 2   Day 3
|-------|-------|-------|
 Results Results Results

Combination disappears on day 2 and after day 3:
   Day 1     Day 2     Day 3     Day 4
|---------|---------|---------|---------|
| Results |   QM    | Results | Results |

For combinations:
- Flex
- ES 3000000000000
- BRP 3300000000000
- GA 800
and
- Production
- ES 4000000000000
- BRP 4400000000000
- GA 800

1st run - Day 1 to 3
  Day 1   Day 2   Day 3
|-------|-------|-------|
 Results Results Results

Combinations disappears on day 2 and after day 3:
   Day 1     Day 2     Day 3     Day 4
|---------|---------|---------|---------|
|   QM    |   QM    |   QM    | Nothing |

## CASES TESTED
"""
Cases.CalculationTests.WithDrawalTests.WithDrawal_when_new_calculation_period_only_partially_covers_previous

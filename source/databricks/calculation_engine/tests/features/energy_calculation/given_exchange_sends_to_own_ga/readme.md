# Given exchange MP sends to own GA

The purpose is to test a scenario where there are exchange MPs where from and to grid area is the same.

## Design considerations

- Two variants are tested:
    - One is MP 200000000000000002 which is in another grid area (802) but sends from and to the grid area for this
      calculation (800).
    - One is MP 200000000000000004 which is in the grid area for this calculation (800) but sends to and from another
      grid area (804).
- Expected behaviour is that the measurements from the former are included in the calculation, but not from the latter.

## Coverage
 - Exchange MP where from and to is the same grid area

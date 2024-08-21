# Given period is two days with GLMP and SKMP changing ES

The purpose is to test that grid loss and system correction metering points can change energy supplier and balance
responsible.

## Design considerations

- Input period is post May 2023 so that results are quarterly
- Input period is two days with change of energy supplier/balance responsible on the grid loss and system correction
  metering points between the two days
- Quantities are set so that both grid loss and system correction metering point have values both days

## Coverage
 - Energy Supplier change on Grid Loss MP
 - Energy Supplier change on System Correction MP
 - Balance Responsible change on Grid Loss MP
 - Balance Responsible change on System Correction MP

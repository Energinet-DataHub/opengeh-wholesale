update Batch
set PeriodEnd = DateAdd(millisecond,1, PeriodEnd)
where DATEPART(millisecond, PeriodEnd) = 999

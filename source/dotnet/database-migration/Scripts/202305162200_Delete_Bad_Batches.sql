delete from batches.Batch
where executionstate = 2
    and (executiontimestart is null or executiontimeend is null)

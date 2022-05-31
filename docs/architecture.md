# Architecture

## Design Drivers

In general DataHub is build as a .NET Azure cloud based b2b system. The wholesale domain, however, crunches huge amounts of data and so the decision has been made to base those crunching parts on the [Databricks Lakehouse Platform](https://databricks.com/).

Data stored in parquet (possibly in Delta Tables) cannot be read synchronously. This implies an impedance mismatch between the requirements of the use cases of the interactive web-based users and the inner workings of the domain. This is key to understand some of the architectural design decisions.

## How does it Look?

Direction of arrows between components designate (1) flow of data like messages and/or (2) invocation in which case the arrow origins at the caller and points to the callee.

Systems, domains, and users interacting with the domain are colored green to easily identify the origin of the interactions with the domain.

![Architecture!](images/architecture.drawio.png)

## Selected Components

### `time-series-points`

Supports retrieval of historical state of metering points. This is a bi-temporal requirement that the data source must also be able to retrieve the state of time series points as they were at a certain point in time.

### `integration-event-listener`

Subscribes to integration events of interest from other domains, extracts the content from the Protobuf encoded messages. The purpose is the Protobuf decoding, which happens to be much simpler to do in .NET rather than in python.

### `job-manager`

Manages requested jobs.

Non-terminated jobs registered in the SQL database are monitored. Requested jobs are being started as Databricks jobs in the `calculator`. The manager continously monitors the Databricks jobs in order to (1) update the job status in the SQL table and (2) publish job completed events to `completed-jobs`.

Details: The manager provides the output path of the results to the `calculator` and adds the path to job in the SQL table.

### `result-sender`

Sends result to actors when aggregation job is complete.

As per integration design by the MessageHub domain the sender creates the necessary CIM XML RSM-014 documents and sends the path to the DataAvailable component of the MessageHub domain. This in turn enables the actors to peek and dequeue the messages.

### `e-sett-result-sender`

Subscribes to job completed events and sends results to [eSett](https://www.esett.com/) for nordic imbalance settlement.

## Selected Use Cases

TBW: View basis data in web

## Future Considerations

### Synchronous Access to Basis Data

TBW
Due to the 
FAS user needs to request basis data before download. We can later optimize with cache.

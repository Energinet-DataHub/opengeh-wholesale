# Databricks SQL Statement Execution API Client

This project contains a client for the Databricks SQL Statement Execution API. The client is a wrapper around the Databricks REST API. The client is used to execute SQL statements on a Databricks cluster.

The implementation uses solely `disposition=EXTERNAL_LINKS` despite that inlining is simpler. This is because inlining has a limit of 16MB, which isn't sufficient for all our use cases.

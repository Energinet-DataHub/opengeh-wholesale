# How to use secrets in notebooks

Secrets are stored as environment variables in the Databricks cluster, and read out in code.

## How to setup environment variables:

In the Databricks menu select "Compute"

Under the tab "All-purpose clusters" select the cluster you want to setup, if you do not see any cluster try to select the "All" filter.

In the "Configuration" tab, find "Advanced option"

Under the "Spark" tab you find the field "Environment variables" here you set the variables you need.

Example:

```bash
STORAGE_ACCOUNT_KEY="SOME-SECRET-KEY"
```

## How to read environment variables in code

Python example:

```python
import os
storage_account_key = os.environ["STORAGE_ACCOUNT_KEY"]
```

Scala example:

```scala
var storageKey = sys.env("STORAGE_ACCOUNT_KEY")
```

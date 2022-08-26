# How to use secrets in notebooks

Secrets are stored as environment variables in the databricks cluster, and read out in code.

**How to setup enviroment variables:**

In the databricks menu select "Compute"

Under the tab "All-purpose clusters" select the cluster you want to setup, if you do not see any cluster try to select the "All" filter.

In the "Configuration" tab, find "Advanced option"

Under the "Spark" tab you find the field "Environment variables" here you set the variables you need.

Example:
```
STORAGE_ACCOUNT_KEY="SOME-SECRET-KEY"
```

**How to read environment variables in code**

Python example:
```
import os
storage_account_key = os.environ["STORAGE_ACCOUNT_KEY"]
```

Scala example:
```
var storageKey = sys.env("STORAGE_ACCOUNT_KEY")
```
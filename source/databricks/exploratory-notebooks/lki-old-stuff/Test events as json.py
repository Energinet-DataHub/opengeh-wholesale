# Databricks notebook source
# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
import json
from pyspark.sql.types import *
from pyspark.sql.functions import *
from pyspark import SparkConf
from delta.tables import *
from datetime import datetime

storage_account_name = "timeseriesdatajouless" # this must be changed to your storage account name
storage_account_key = "<INSERT>"
containerName = "messagedata"
source = "abfss://messagedata@timeseriesdatajouless.dfs.core.windows.net/test-json-diff/"
print(source)

spark.conf.set("fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),storage_account_key)



# COMMAND ----------

df = spark.read.json(source)
df.show(100, False)

# COMMAND ----------

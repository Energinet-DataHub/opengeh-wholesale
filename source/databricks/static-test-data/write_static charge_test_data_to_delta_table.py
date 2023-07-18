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


# Databricks notebook source
from pyspark.sql import SparkSession

# Create SparkSession
spark = SparkSession.builder.getOrCreate()

# Define the source and destination table names
source_table = "default.charges_links_static"
destination_table = "wholesale.charge_link_periods"

# Read the destination table to retrieve the relevant columns
destination_df = spark.read.format("delta").table(destination_table)

# Get the column names from the destination table
relevant_columns = destination_df.columns

# Read the source table into a DataFrame
source_df = spark.read.format("delta").table(source_table)


# Select only the relevant columns from the source DataFrame
selected_df = source_df.select(relevant_columns)

# Write the selected DataFrame to the destination table
selected_df.write.format("delta").mode("append").insertInto(destination_table)

selected_df.show(1000)

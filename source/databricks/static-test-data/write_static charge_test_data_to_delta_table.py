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

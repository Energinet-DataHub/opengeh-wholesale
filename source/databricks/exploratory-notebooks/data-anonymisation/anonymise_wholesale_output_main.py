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

# COMMAND ----------

# MAGIC %md
# MAGIC ### Anoymise dataset for test environments
# MAGIC This notebook transforms the data from the p-001 to an anonymised version that could be transfered to t001 environment.
# MAGIC We use this method to ensure we can not recalculate the anonymised ids via any function and the data in the test environment is safe
# MAGIC We should keep this notebook to ensure we can anonymise data if it should be needed again.

# COMMAND ----------

import pyspark.sql.functions as F
from pyspark.sql.window import Window

# Source variables
source_database = "hive_metastore.wholesale_output"  # FILL IN
source_energy_results_table_name = "energy_results"
source_wholesale_results_table_name = "wholesale_results"
source_metering_points_database_and_table_name = (
    "wholesale_input.metering_point_periods"
)
source_charge_masterdata_database_and_table_name = (
    "wholesale_input.charge_masterdata_periods"
)
source_calculation_id_to_use = "51d60f89-bbc5-4f7a-be98-6139aab1c1b2"  # FILL IN

# Target variables
target_database = "hive_metastore.wholesale_output_anonymised"  # FILL IN
target_energy_results_table_name = "energy_results"
target_wholesale_results_table_name = "wholesale_results"
target_storage_account_name = "stdatalakeshresdwe002"  # FILL IN
target_delta_table_root_path = f"abfss://wholesale@{target_storage_account_name}.dfs.core.windows.net/wholesale_output_anonymised"  # FILL IN

# Source columns variables
grid_area_code_column_name = "grid_area_code"
metering_point_id_column_name = "metering_point_id"
balance_responsible_id_column_name = "balance_responsible_id"
energy_supplier_id_column_name = "energy_supplier_id"
calculation_id_column_name = "calculation_id"
charge_owner_id_column_name = "charge_owner_id"

# Anonymised columns variables
anonymised_grid_area_code_column_name = "anonymised_grid_area_code"
anonymised_balance_or_supplier_id_column_name = "anonymised_balance_or_supplier_id"
anonymised_charge_owner_or_supplier_id_column_name = (
    "anonymised_charge_owner_or_supplier_id"
)
anonymised_metering_point_id_column_name = "anonymised_metering_point_id"

# GLN generation variables
tmp_gln_column_name = "tmp_column"
gln_original_column_name = "gln"
gln_anonymised_column_name = "gln_anonymised"

# COMMAND ----------

# MAGIC %md
# MAGIC # Get All GLN Numbers To Later Anonymise

# COMMAND ----------

# Read all metering points
df_source_metering_points_table = (
    spark.read.table(f"{source_metering_points_database_and_table_name}")
    .select(
        metering_point_id_column_name,
        energy_supplier_id_column_name,
        balance_responsible_id_column_name,
    )
    .distinct()
)

# COMMAND ----------

# Read all charge masterdata periods
df_source_charge_masterdata_table = (
    spark.read.table(f"{source_charge_masterdata_database_and_table_name}")
    .select(charge_owner_id_column_name)
    .distinct()
)

# COMMAND ----------

# Read all data from energy results wholesale output
df_source_energy_results_table = (
    spark.read.table(f"{source_database}.{source_energy_results_table_name}")
    .select(
        metering_point_id_column_name,
        balance_responsible_id_column_name,
        energy_supplier_id_column_name,
    )
    .distinct()
)

# COMMAND ----------

# Read all data from wholesale results wholesale output
df_source_wholesale_results_table = (
    spark.read.table(f"{source_database}.{source_wholesale_results_table_name}")
    .select(charge_owner_id_column_name, energy_supplier_id_column_name)
    .distinct()
)

# COMMAND ----------

# MAGIC %md
# MAGIC All GLNs to be anonymised should be added to this dataframe below.
# MAGIC
# MAGIC df_all_gln_numbers is the dataframe that contains all GLNs that should have an anonymised GLN.
# MAGIC
# MAGIC Currently contains: Metering Point Id, Energy Supplier Id, Balance Responsible Id, and Charge Owner Id.

# COMMAND ----------

df_all_gln_numbers_for_mp = (
    df_source_metering_points_table.select(
        F.col(metering_point_id_column_name).alias(tmp_gln_column_name)
    )
    # These tables below shouldn't be needed for production, but for dev-002 we have some GLNs that only exists in the output tables
    .union(
        df_source_energy_results_table.select(
            F.col(metering_point_id_column_name).alias(tmp_gln_column_name)
        )
    )
    .distinct()
    .filter(F.col(tmp_gln_column_name).isNotNull())
)

df_all_gln_numbers_for_others = (
    df_source_metering_points_table.select(
        F.col(energy_supplier_id_column_name).alias(tmp_gln_column_name)
    )
    .union(
        df_source_metering_points_table.select(
            F.col(balance_responsible_id_column_name).alias(tmp_gln_column_name)
        )
    )
    .union(
        df_source_charge_masterdata_table.select(
            F.col(charge_owner_id_column_name).alias(tmp_gln_column_name)
        )
    )
    # These tables below shouldn't be needed for production, but for dev-002 we have some GLNs that only exists in the output tables
    .union(
        df_source_energy_results_table.select(
            F.col(balance_responsible_id_column_name).alias(tmp_gln_column_name)
        )
    )
    .union(
        df_source_energy_results_table.select(
            F.col(energy_supplier_id_column_name).alias(tmp_gln_column_name)
        )
    )
    .union(
        df_source_wholesale_results_table.select(
            F.col(charge_owner_id_column_name).alias(tmp_gln_column_name)
        )
    )
    .union(
        df_source_wholesale_results_table.select(
            F.col(energy_supplier_id_column_name).alias(tmp_gln_column_name)
        )
    )
    .distinct()
    .filter(F.col(tmp_gln_column_name).isNotNull())
)

list_of_gln_numbers_for_others = [
    row[tmp_gln_column_name] for row in df_all_gln_numbers_for_others.collect()
]

# COMMAND ----------

# MAGIC %md
# MAGIC ###Anonymisation algorithm for Metering Point IDs:
# MAGIC 1) Prefix anonymised ID with "5"
# MAGIC 2) Concat random row_number over each row of unique MP IDs, and left-pad with "0" with same length as unique MP IDs
# MAGIC 3) Concat "5"
# MAGIC 4) Right-pad with "0" until 18 characters in total
# MAGIC
# MAGIC #### Example
# MAGIC **1)**
# MAGIC Original (fake) MP ID: 514526978536898745 (1st after random order)
# MAGIC Anonymised MP ID: 500000015000000000
# MAGIC
# MAGIC **2)**
# MAGIC Original (fake) MP ID: 525865741589334125 (532435th after random order)
# MAGIC Anonymised MP ID: 505324355000000000

# COMMAND ----------

df_all_metering_point_ids = (
    df_all_gln_numbers_for_mp.select(
        F.col(tmp_gln_column_name).alias(gln_original_column_name)
    ).distinct()
).cache()

count_distinct_mpids = len(str(df_all_metering_point_ids.count()))
window_random_order = Window.orderBy(F.rand())

df_anonymised_metering_points = (
    df_all_metering_point_ids.withColumn(
        gln_anonymised_column_name,
        F.rpad(
            F.concat(
                F.lit("5"),
                F.lpad(
                    F.row_number().over(window_random_order), count_distinct_mpids, "0"
                ),
                F.lit("5"),
            ),
            18,
            "0",
        ),
    ).withColumn(
        gln_anonymised_column_name,
        F.when(
            F.col(gln_original_column_name).isNull(),
            F.lit(None),
        ).otherwise(F.col(gln_anonymised_column_name)),
    )
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC # Generate GLN Numbers To Be Used
# MAGIC ### Anonymisation algorithm all GLNs
# MAGIC Use randomized GLN Generator provided by Raccoons. It generates a valid GLN with the correct length of 12 characters, it makes sure that the checksum is valid and thus provides a valid GLN.
# MAGIC
# MAGIC The functions below are a direct translation from C# to Python, and is a translation of Raccoons GLN Generator

# COMMAND ----------

import random
import math
import typing
from typing import List, Tuple


def create_random_gln(length_of_gln=12):
    rng = random.Random()
    location = "".join(str(rng.randint(0, 9)) for _ in range(length_of_gln))

    for check_digit in range(10):
        gln = location + str(check_digit)
        if calculate_checksum(gln) == check_digit:
            return gln

    raise Exception("Should not happen")


def calculate_checksum(gln_number):
    sum_of_odd_numbers = 0
    sum_of_even_numbers = 0

    for i in range(1, len(gln_number)):
        current_number = int(gln_number[i - 1])

        if i % 2 == 0:
            sum_of_even_numbers += current_number
        else:
            sum_of_odd_numbers += current_number

    sum = sum_of_even_numbers * 3 + sum_of_odd_numbers

    return (math.ceil(sum / 10.0) * 10) - sum


# COMMAND ----------


def get_mapping_for_gln_numbers(
    list_of_gln: List, length_of_gln: int
) -> List[Tuple[int, int]]:
    list_of_anonymised_gln_numbers = []
    anonymised_gln_numbers_mapping = []
    for gln_number in list_of_gln:
        anonymised_gln_number = create_random_gln(length_of_gln)

        # Create a new GLN number until we get one we haven't seen yet or one that doesn't match a real GLN
        while (
            anonymised_gln_number in list_of_anonymised_gln_numbers
            or anonymised_gln_number in list_of_gln
        ):
            anonymised_gln_number = create_random_gln(length_of_gln)

        # Add to list of anonymised GLN numbers as well as the mapping
        list_of_anonymised_gln_numbers.append(anonymised_gln_number)
        anonymised_gln_numbers_mapping.append((gln_number, anonymised_gln_number))
    return anonymised_gln_numbers_mapping


# COMMAND ----------

anonymised_gln_numbers_mapping_for_others = get_mapping_for_gln_numbers(
    list_of_gln_numbers_for_others, length_of_gln=12
)

# COMMAND ----------

df_anonymised_gln_numbers = spark.createDataFrame(
    anonymised_gln_numbers_mapping_for_others + [(None, None)],
    [gln_original_column_name, gln_anonymised_column_name],
).cache()
df_anonymised_gln_numbers_with_mps = df_anonymised_gln_numbers.union(
    df_anonymised_metering_points
).cache()

# COMMAND ----------

# MAGIC %md
# MAGIC Assert that:
# MAGIC 1. We have no duplicates of GLNs in the original column of the anonymisation table
# MAGIC 2. We have no duplicates of GLNs in the anonymisation column of the anonymisation table
# MAGIC 3. We have the same amount of unique GLNs in the original column and the anonymisation column

# COMMAND ----------

assert (
    df_anonymised_gln_numbers_with_mps.groupBy(gln_original_column_name)
    .agg(F.sum(F.lit(1)).alias("id_count"))
    .filter("id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

assert (
    df_anonymised_gln_numbers_with_mps.groupBy(gln_anonymised_column_name)
    .agg(F.sum(F.lit(1)).alias("id_count"))
    .filter("id_count > 1")
    .count()
    == 0
)

# COMMAND ----------

assert (
    df_anonymised_gln_numbers_with_mps.select(gln_original_column_name)
    .distinct()
    .count()
    == df_anonymised_gln_numbers_with_mps.select(gln_anonymised_column_name)
    .distinct()
    .count()
)

# COMMAND ----------

# MAGIC %md
# MAGIC # Run Anonymise Wholesale Output Tables Individually

# COMMAND ----------

# MAGIC %run "./anonymise_wholesale_output_energy_results_data"

# COMMAND ----------

# MAGIC %run "./anonymise_wholesale_output_wholesale_results_data"

# COMMAND ----------

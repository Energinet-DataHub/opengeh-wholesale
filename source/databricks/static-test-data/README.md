# Static test data files fro charges

These files are used temporarily during development of charges in the wholesale domain. When the Migration domain starts to migrate charge data, these files can be removed.
The csv files (charges_links.csv, charges_master.csv and chages_prices.csv) are the raw files. These needs to be uploaded to Databricks. Import them using the Databricks UI and then copy the data to the delta tables using the notebook (charge_test_data_to_delta_table.py)

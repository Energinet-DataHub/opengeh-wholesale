# Settlement Report

[![Code style: black](https://img.shields.io/badge/code%20style-black-000000.svg)](https://github.com/psf/black)

## Overview

The `settlement_report` folder contains all the relevant code for creating and testing the python wheel, which is used in the Databricks Job(s) for creating settlement reports.

The wheel's responsibility is to:

- Create a csv files with results (and optionally basis data) from wholesale calculations or balance fixing calculations.
- Zip the csv files mentioned above

The wheel exposes a set of entry points that are used in the Databricks for creating the settlement reports.

## Project Structure

The `settlement_report_job` contains the required code for creating the python wheel. The code is structured as follows:

**entry_points/**:

Handles interactions with the outside world. It contains all the entry points and argument that the Databricks jobs/tasks use, and it is responsible for orchestrating the settlement report generation process.

**domain/**:

Contains the core business logic and rules. This folder is divided into subfolders: - one for each report type (e.g. metering point periods, time series points and energy results). Logic for preparing settlement reports data is implemented here. Here are some of the key responsibilities that is implemented for each type of settlement report data:

- filter input data based on the given parameters
- transform the input data into the desired format for csv: columns, data types, ordering etc.

**infrastructure/**:

Implementations for accessing and storing data for settlement reports. This means things like accessing delta tables/view (repository), writing csv and generating the zip file.

## Using the wheel in Databricks

The intention of the wheel is to be used with Databricks tasks. There is an entry point for each report type and one for the zip file generation. The orchestration of Databricks tasks is done in dh3-infrastructure.

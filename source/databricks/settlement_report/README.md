# Settlement Report

[![Code style: black](https://img.shields.io/badge/code%20style-black-000000.svg)](https://github.com/psf/black)


## Overview

The folder contains all the relevant code for creating and testing the python wheel used in the Databricks environment for creating settlement reports.

 The codebase is organized to facilitate development, testing, and deployment processes.

## Directory Structure


The 'The project directory is organized as follows:

**domain/**:

Contains the core business logic and rules. This folder is divided into subfolders - one for each type of settlement report data (e.g. metering point periods, time series points and energy results). Logic for generating and managing settlement reports is implemented here.

**entry_points/**:

Handles interactions with the outside world. It contains all the entry points that the Databricks jobs uses. And it is responsible for orchestrating the settlement report generation process.

**infrastructure/**: 
 
Implementations for accessing and storing settlement data. This means things like accessing delta tables/view (repository), writing csv and generating the zip file.
  
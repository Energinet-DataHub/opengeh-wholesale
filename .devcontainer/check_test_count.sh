#!/bin/bash -l

# Description
# This script checks if all the tests are included in the matrix in the test step in ci-databricks.yml.
# It is used in the pipeline to ensure that all the tests are included in the matrix.
# The script must be invoked with a filter matching the paths NOT included in the matrix

# $1: (Optional) Can be set to specify a filter for running python tests at the specified path.
echo "Filter (paths): '$@'"

# Exit immediately with failure status if any command fails
set -e

cd source/databricks/calculation_engine/tests/
# Enable extended globbing. E.g. see https://stackoverflow.com/questions/8525437/list-files-not-matching-a-pattern
shopt -s extglob

# This script runs pytest with the --collect-only flag to get the number of tests.
# 'grep' filters the output to get the line with the number of tests collected. Multiple lines can be returned.
# 'awk' is used to get the second column of the output which contains the number of tests.
# 'head' is used to get the first line of the output which contains the number of tests.
# Example output line returned by the grep filter: 'collected 10 items'
executed_test_count=$(pytest $@ --collect-only  | grep collected | awk '{print $2}' | head -n 1)

total_test_count=$(pytest --collect-only  | grep collected | awk '{print $2}' | head -n 1)

echo "Number of tests being executed: $executed_test_count"
echo "Total number of pytest tests: $total_test_count"


if [ "$total_test_count" == "$executed_test_count" ]; then
    echo "Not missing any tests."
else
    difference=$((total_test_count - executed_test_count))
    echo "Found $difference tests not executed. A folder is missing in the matrix."
    exit 1
fi

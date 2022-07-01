#!/bin/sh
clear
echo "Starting pytest with remote debugger attachment on port 3000"
echo "Please attach your debugger now"
python -m ptvsd --host 0.0.0.0 --port 3000 --wait -m pytest -v "$1"

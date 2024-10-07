#!/bin/bash

# Read arguments
grafanaName=${grafanaName}
resourceGroup=${resourceGroup}
subscription=${subscription}

# Only create service account if it doesn't exist, else only create token
if [ -z "$(az grafana service-account show --service-account spn -g "$resourceGroup" -n "$grafanaName" --subscription "$subscription")" ]; then
    echo "Creating Grafana service account"
    az grafana service-account create -g "$resourceGroup" -n "$grafanaName" --service-account spn --role admin --subscription "$subscription"
fi

key=$(az grafana service-account token create -g "$resourceGroup" -n "$grafanaName" --service-account spn --token "spntoken$RANDOM" --time-to-live "12M" --subscription "$subscription" --query "key" -o tsv)

echo $(jq -n --arg key "$key" '{"key": $key}')

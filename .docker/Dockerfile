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

# The spark version should follow the spark version in databricks.
# The databricks version of spark is controlled from dh3-infrastructure and uses latest LTS (ATTOW - Spark v3.3.0)
FROM jupyter/pyspark-notebook:spark-3.3.0

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

USER root
# This replaces the default spark configuration in the docker image with the ones defined in the sibling file
COPY spark-defaults.conf $SPARK_HOME/conf/spark-defaults.conf

# Install Azure CLI, see https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt
#   Install curl to be able to download Azure CLI install script
RUN apt-get update; \
    apt-get install -y curl
#   Download and execute install script
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Install Azurite (support wasb-protocol storage for Delta tables in integration tests)
RUN apt-get update; \
    apt-get install -y npm && npm install -g n && n lts && hash -r && npm install -g azurite

# Cleanup apt cache to reduce image size
RUN rm -rf /var/lib/apt/lists/*

# Install spark packages with mamba (packages has to be updated according to spark version)
RUN mamba install --quiet --yes --satisfied-skip-solve \
    'rope=1.6.*' 'pytest=7.2.*' 'configargparse=1.5.3'  \
    'coverage=7.0.*' 'azure-storage-blob=12.14.*' 'pytest-mock=3.10.*' \
    'virtualenv=20.*'

# Install python packages used in pyspark development (keep spark dependent packages alligned)
# delta-spark version has to have compatibility with spark version (https://docs.delta.io/latest/releases.html)
# example (delta 2.2.x = spark 3.3.x)
RUN pip --no-cache-dir install pyspelling azure-eventhub azure-identity==1.12.0 azure-storage-file-datalake==12.11.0 coverage-threshold types-python-dateutil \
    ptvsd azure-servicebus pytest-asyncio flake8 black dataclasses-json pytest-xdist databricks-cli==0.17.6 mypy delta-spark==2.2.0 \
    python-dateutil==2.8.2 dependency_injector==4.41.0 azure-monitor-query==1.2.0\
    azure-monitor-opentelemetry==1.0.0 azure-keyvault-secrets==4.7.0 \
    build

# Below will make everything in the directory owned by the group ${NB_GID}
RUN fix-permissions "${CONDA_DIR}"

# Set misc environment variables required for properly run spark
# note the amount of memory used on the driver is adjusted here
ENV PATH=$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH \
    PYTHONPATH="${SPARK_HOME}/python:${SPARK_HOME}/python/lib/py4j-0.10.9-src.zip" \
    SPARK_OPTS="--driver-java-options=-Xms1024M --driver-java-options=-Xmx4096M --driver-java-options=-Dlog4j.logLevel=info"

# Dynamically downloading spark dependencies from conf/spark-defaults.conf. This is done to save time in the build pipeline so that we don't need to download on every build.
RUN spark-shell

# Make $HOME owned by the root, which is the user used in the container
# This is needed for e.g. commands that create files or folders in $HOME
RUN sudo chown -R root:users $HOME

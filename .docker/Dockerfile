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
# The databricks version of spark is controlled from dh3-infrastructure and uses latest LTS (ATTOW - Spark v3.5.0)
FROM ghcr.io/energinet-datahub/pyspark-slim:3.5.1-3

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

USER root

RUN apt-get update; \
    # Install git as it is needed by spark
    apt-get install --no-install-recommends -y git; \
    # Curl is temporarily installed in order to download the Azure CLI (consider multi stage build instead)
    apt-get install --no-install-recommends -y curl; \
    # Install Azure CLI, see https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt
    # as it is needed by integration tests
    curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash; \
    # Cleanup apt cache to reduce image size
    apt-get remove -y curl; \
    rm -rf /var/lib/apt/lists/*

# This replaces the default spark configuration in the docker image with the ones defined in the sibling file
COPY spark-defaults.conf $SPARK_HOME/conf/

# Install spark packages with mamba (packages has to be updated according to spark version)
COPY mamba_requirements.txt mamba_requirements.txt
RUN mamba install --quiet --yes --satisfied-skip-solve --file mamba_requirements.txt

# Install python packages used in pyspark development (keep spark dependent packages alligned)
# delta-spark version has to have compatibility with spark version (https://docs.delta.io/latest/releases.html)
# example (delta 2.2.x = spark 3.3.x)
COPY requirements.txt requirements.txt
RUN pip --no-cache-dir install -r requirements.txt

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

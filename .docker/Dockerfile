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

FROM jupyter/pyspark-notebook:spark-3.1.2

SHELL ["/bin/bash", "-o", "pipefail", "-c"]

USER root
# This replaces the default spark configuration in the docker image with the ones defined in the sibling file
COPY spark-defaults.conf $SPARK_HOME/conf/spark-defaults.conf


# Install Azurite (support wasb-protocol storage for Delta tables in integration tests)
RUN apt-get update; \
    apt-get install -y npm && npm install -g n && n lts && hash -r && npm install -g azurite

# Install spark packages with mamba 
RUN mamba install --quiet --yes --satisfied-skip-solve \
    'pyarrow=2.0.*' 'rope=0.18.*' 'pytest=7.1.*' 'configargparse=1.2.3'  \
    'coverage=6.3.*' 'azure-storage-blob=12.8.*' 'pytest-mock=3.7.*' \
    'virtualenv=20.*'

# Install python packages used in pyspark development 
RUN pip --no-cache-dir install pyspelling azure-eventhub azure-identity azure-storage-file-datalake coverage-threshold \
    ptvsd azure-servicebus pytest-asyncio flake8 black dataclasses-json pytest-xdist databricks-cli \
    build

# Below will make everything in the directory owned by the group ${NB_GID}
RUN fix-permissions "${CONDA_DIR}"

# Set misc environment variables required for properly run spark 
# note the amount of memory used on the driver is adjusted here
ENV PATH=$SPARK_HOME/bin:$HADOOP_HOME/bin:$PATH \
    PYTHONPATH="${SPARK_HOME}/python:${SPARK_HOME}/python/lib/py4j-0.10.9-src.zip" \
    SPARK_OPTS="--driver-java-options=-Xms1024M --driver-java-options=-Xmx4096M --driver-java-options=-Dlog4j.logLevel=info"

# Dynamically downloading spark dependencies. This is done to safe time in the build pipeline so that we don't need to download on every build.
RUN spark-shell -conf spark-defaults.conf

# Make $HOME owned by the root, which is the user used in the container
# This is needed for e.g. commands that create files or folders in $HOME
RUN sudo chown -R root:users $HOME

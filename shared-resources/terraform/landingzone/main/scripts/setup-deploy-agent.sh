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

###################################################################
#
# Install GitHub runner and other software dependencies
# necessary for running our deployments
#
# Parameters:
#   $1: GitHub runner regitration token
#   $2: Name of deployment agent; also added as label
#
###################################################################


##################################
# Install Docker
##################################

#
# See https://docs.docker.com/engine/install/ubuntu/
#

# Setup repository

sudo apt-get update
sudo apt-get install -y \
  ca-certificates \
  curl \
  gnupg \
  lsb-release

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine

sudo apt-get update
sudo apt-get install -y \
  docker-ce \
  docker-ce-cli \
  containerd.io \
  docker-compose-plugin

#
# Avoid prefacing 'docker' command with 'sudo'
# See https://docs.docker.com/engine/install/linux-postinstall/#manage-docker-as-a-non-root-user
#

sudo groupadd docker
sudo usermod -aG docker $USER
# IMPORTANT: GitHub runner service must be started AFTER this step for it to work

##################################
# Install .NET SDK's
##################################

#
# Add the Microsoft package signing key to your list of trusted keys and add the package repository
# See https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#1804-
#

wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

#
# Install versions
# See https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#how-to-install-other-versions
#

# .NET SDK 5.0
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0

# .NET SDK 6.0
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-6.0

##################################
# Install Powershell
##################################

#
# See https://docs.microsoft.com/en-us/powershell/scripting/install/install-ubuntu?view=powershell-7.2
#

# Update the list of packages
sudo apt-get update
# Install pre-requisite packages.
sudo apt-get install -y wget apt-transport-https software-properties-common
# Download the Microsoft repository GPG keys
wget -q "https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb"
# Register the Microsoft repository GPG keys
sudo dpkg -i packages-microsoft-prod.deb
# Update the list of packages after we added packages.microsoft.com
sudo apt-get update
# Install PowerShell
sudo apt-get install -y powershell

##################################
# Install other dependencies
##################################

#
# Install Azure CLI
# See https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt
#

# First install latest version, to get all dependencies configured
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
# Then downgrade to the vesion we want
sudo apt-get install azure-cli=2.36.0-1~bionic --allow-downgrades -y

#
# Install unzip
#

sudo apt-get install unzip

#
# Install jq
#

sudo apt-get install -y jq

#
# Install pip
#

sudo apt install -y python-pip

#
# Install Databricks CLI on machine
#

pip install --upgrade databricks-cli


#
# Install Homebrew, reference: https://docs.brew.sh/Homebrew-on-Linux
#
NONINTERACTIVE=1 /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
echo 'eval "$(/home/linuxbrew/.linuxbrew/bin/brew shellenv)"' | sudo tee -a /root/.profile
eval "$(/home/linuxbrew/.linuxbrew/bin/brew shellenv)"

#
# Install fetch  (reference: https://github.com/gruntwork-io/fetch)
#
brew install fetch

##################################
# GitHub runner
##################################

#
# Download
#

# Create a folder
mkdir actions-runner && cd actions-runner

# Download the latest runner package
curl -o actions-runner-linux-x64-2.285.1.tar.gz -L https://github.com/actions/runner/releases/download/v2.285.1/actions-runner-linux-x64-2.285.1.tar.gz

# Validate the hash
echo '5fd98e1009ed13783d17cc73f13ea9a55f21b45ced915ed610d00668b165d3b2  actions-runner-linux-x64-2.285.1.tar.gz' | shasum -a 256 -c

# Extract the installer
tar xzf ./actions-runner-linux-x64-2.285.1.tar.gz

#
# Configure
#

# Create the runner and start the configuration experience
./config.sh --unattended --url https://github.com/Energinet-DataHub/dh3-environments --token $1 --name $2 --replace --labels azure,$3

#
# Run as a service
#

sudo ./svc.sh install
sudo ./svc.sh start
sudo ./svc.sh status

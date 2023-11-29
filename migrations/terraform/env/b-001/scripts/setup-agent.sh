###################################################################
#
# Install GitHub runner and other software dependencies necessary
# for running our deployments.
#
# Parameters:
#   $1: GitHub runner regitration token
#   $2: Name of deployment agent; also added as label
#
###################################################################

##################################
#
# Set shell options
# https://www.gnu.org/software/bash/manual/html_node/The-Set-Builtin.html
#
##################################

# Exit immediately if a pipeline (command, list of commands, compound command)
# returns a non-zero status.
set -e

##################################
#
# Setup Python, a Python virtual environment manager and
# python packages necessary for running Wholesale deployments.
#
# Preinstalled Python versions
# ----------------------------
# The current Ubuntu 18.04 image comes with:
#  - Python 2.7.17
#  - Python 3.6.9
#
# Most relevant tools
# -------------------
# 'python'
#   If not used with 'pyenv' it will use Python 2.x
# 'python3'
#   If not used with 'pyenv' it will use Python 3.x
# 'pip'
#   A package installer for Python.
# 'pyenv'
#   A virtual environment manager for Python.
#   See https://realpython.com/intro-to-pyenv/
#
# Concept
# -------
# To use Databricks CLI (commands) in deployments we need the
# package 'databricks-cli' which must be installed by 'pip' using
# Python 2.7.17 (the system version).
#
# To be able to use Python versions in isolation in deployments
# we install 'pyenv' so we can shift between 'system' and any
# specific Python version per deployment.
#
# To allow deployments to use other versions of Python we must
# install these versions.
#
# In deployment we then use 'pyenv' to create virtual environments
# using these Python versions.
#
# IMPORTANT: Ensure to delete any created environments after use
# in the deployment.
#
##################################

#
# Python 2.x ('python')
#

# Test 'python' version
python --version

# Install 'pip' for 'python'
sudo apt-get update
sudo apt-get install -y python-pip

# Install 'databricks-cli' for 'python' (the system version)
pip install --upgrade databricks-cli

#
# Python virtual environments
#

# Install 'pyenv' build dependencies
sudo apt-get update
sudo apt-get install -y make build-essential libssl-dev zlib1g-dev \
  libbz2-dev libreadline-dev libsqlite3-dev wget curl llvm libncurses5-dev \
  libncursesw5-dev xz-utils tk-dev libffi-dev liblzma-dev python-openssl

# Install 'pyenv'
# See https://realpython.com/intro-to-pyenv/#installing-pyenv
# or https://kolibri-dev.readthedocs.io/en/develop/howtos/installing_pyenv.html

curl https://pyenv.run | bash

# The output of the command tells us to add certain lines to our startup files for terminal sessions.
# This will add 'pyenv' to path and initialize 'pyenv/pyenv-virtualenv' auto completion:
#  - ~/.bashrc for interactive shells and ~./profile for login shells
echo 'export PYENV_ROOT="$HOME/.pyenv"' | tee -a ~/.bashrc ~/.profile
echo 'command -v pyenv >/dev/null || export PATH="$PYENV_ROOT/bin:$PATH"' | tee -a ~/.bashrc ~/.profile
echo 'eval "$(pyenv init -)"' | tee -a ~/.bashrc ~/.profile
echo 'eval "$(pyenv virtualenv-init -)"' | tee -a ~/.bashrc ~/.profile
#  - For current shell session
export PYENV_ROOT="$HOME/.pyenv"
command -v pyenv >/dev/null || export PATH="$PYENV_ROOT/bin:$PATH"
eval "$(pyenv init -)"
eval "$(pyenv virtualenv-init -)"

pyenv install 3.9.7 # The version used by Wholesale wheel

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

# COMMENTED because we get error: "groupadd: group 'docker' already exists"
### sudo groupadd docker
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

# .NET SDK 7.0
sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-7.0

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
# Install Powershell Modules
##################################

# Notice use of call operator `&`. See https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_pwsh?view=powershell-7.2#-command---c
sudo pwsh -Command "& {Install-Module sqlserver -Force}"

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
# Install Homebrew, reference: https://docs.brew.sh/Homebrew-on-Linux
#
NONINTERACTIVE=1 /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
echo 'eval "$(/home/linuxbrew/.linuxbrew/bin/brew shellenv)"' | sudo tee -a /root/.profile
eval "$(/home/linuxbrew/.linuxbrew/bin/brew shellenv)"

#
# Install fetch  (reference: https://github.com/gruntwork-io/fetch)
#
brew install fetch

# DO NOT INSTALL OR CONFIGURE GITHUB RUNNER
# ##################################
# # GitHub runner
# ##################################

# #
# # Download
# #

# # Create a folder
# mkdir actions-runner && cd actions-runner

# # Download the latest runner package
# curl -o actions-runner-linux-x64-2.299.1.tar.gz -L https://github.com/actions/runner/releases/download/v2.299.1/actions-runner-linux-x64-2.299.1.tar.gz

# # Validate the hash
# echo '147c14700c6cb997421b9a239c012197f11ea9854cd901ee88ead6fe73a72c74  actions-runner-linux-x64-2.299.1.tar.gz' | shasum -a 256 -c

# # Extract the installer
# tar xzf ./actions-runner-linux-x64-2.299.1.tar.gz

# #
# # Configure
# #

# # Create the runner and start the configuration experience
# ./config.sh --unattended --url https://github.com/Energinet-DataHub/dh3-environments --token $1 --name $2 --replace --labels azure,$3

# #
# # Run as a service
# #

# sudo ./svc.sh install
# sudo ./svc.sh start
# sudo ./svc.sh status
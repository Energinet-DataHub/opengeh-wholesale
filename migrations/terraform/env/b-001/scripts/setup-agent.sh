###################################################################
#
# Install GitHub runner.
#
# Parameters:
#   $1: GitHub runner regitration token
#   $2: Name of the agent; also added as label
#
###################################################################

##################################
# GitHub runner
##################################

#
# Download
#

# Create a folder
mkdir actions-runner && cd actions-runner

# Download the latest runner package
curl -o actions-runner-linux-x64-2.299.1.tar.gz -L https://github.com/actions/runner/releases/download/v2.299.1/actions-runner-linux-x64-2.299.1.tar.gz

# Validate the hash
echo '147c14700c6cb997421b9a239c012197f11ea9854cd901ee88ead6fe73a72c74  actions-runner-linux-x64-2.299.1.tar.gz' | shasum -a 256 -c

# Extract the installer
tar xzf ./actions-runner-linux-x64-2.299.1.tar.gz

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

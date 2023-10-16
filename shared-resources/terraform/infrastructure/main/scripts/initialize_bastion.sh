#!/bin/bash

# This script will initialize the Bastion Ubuntu VM with needed software. It should only be run once.
# It uses aad-auth to allow for authentication against azure ad. See https://github.com/ubuntu/aad-auth

sudo apt update && sudo apt upgrade -y

# Install lightweight GUI
sudo apt-get install -y xfce4 --no-install-recommends

# Install dependencies such as xRPD (to allow RDP) and Edge dependencies, and AAD
sudo apt-get -y install libpam-aad libnss-aad xrdp fonts-liberation libu2f-udev xdg-utils dbus-x11 xfce4-terminal alsa-topology-conf alsa-ucm-conf libasound2 libasound2-data

# Create home directory for each user to ensure separate sessions
sudo pam-auth-update --enable mkhomedir

# Install Edge
wget https://packages.microsoft.com/repos/edge/pool/main/m/microsoft-edge-stable/microsoft-edge-stable_117.0.2045.55-1_amd64.deb

sudo dpkg -i microsoft-edge-stable_117.0.2045.55-1_amd64.deb

# Enable multiple sessions in RDP
sudo sed -i '8 a unset DBUS_SESSION_BUS_ADDRESS' /etc/xrdp/startwm.sh
sudo sed -i '8 a unset XDG_RUNTIME_DIR' /etc/xrdp/startwm.sh

sudo systemctl restart xrdp

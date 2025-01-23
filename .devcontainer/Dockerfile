FROM mcr.microsoft.com/vscode/devcontainers/base:ubuntu-22.04

# Install terrascan
RUN curl -L "$(curl -s https://api.github.com/repos/tenable/terrascan/releases/latest | grep -o -E "https://.+?_Linux_x86_64.tar.gz")" > terrascan.tar.gz
RUN tar -xf terrascan.tar.gz terrascan && rm terrascan.tar.gz
RUN install terrascan /usr/local/bin && rm terrascan

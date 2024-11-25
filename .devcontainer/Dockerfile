FROM mcr.microsoft.com/dotnet/sdk:8.0

# Create vscode user (if not exists)
RUN groupadd -g 1000 vscode || true && \
    useradd -m -s /bin/bash -u 1000 -g 1000 vscode

# Install all required packages
RUN apt-get update && \
    apt-get install -y \
    nodejs \
    p7zip-full \
    git \
    procps \
    ssh-client \
    sudo \
    podman && \
    wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Configure sudo access for vscode user
RUN echo "vscode ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/vscode && \
    chmod 0440 /etc/sudoers.d/vscode

# Set up Podman configuration for remote access
RUN mkdir -p /home/vscode/.config/containers && \
    echo '[engine]\nremote = true\nconnections = ["podman-machine-default"]\n[containers]\nuserns = "host"\nutsns = "host"\nipcns = "host"\nnetns = "host"\ncgroupns = "host"\ncgroups = "enabled"' > /home/vscode/.config/containers/containers.conf && \
    chown -R vscode:vscode /home/vscode/.config

ENV PATH="/root/.dotnet/tools:/home/vscode/.dotnet/tools:${PATH}"

# Switch to vscode user
USER vscode
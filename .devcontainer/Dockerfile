FROM mcr.microsoft.com/vscode/devcontainers/dotnet:5.0

# Install .NET Core 3.1
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y apt-transport-https \
    && apt-get update \
    && apt-get install -y aspnetcore-runtime-3.1

# Install Mono (soon won't be necessary (.NET 6?))
RUN apt install gnupg ca-certificates \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" >> /etc/apt/sources.list.d/mono-official-stable.list \
    && apt update \
    && export DEBIAN_FRONTEND=noninteractive \
    && apt install -y mono-devel

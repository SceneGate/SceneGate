FROM mcr.microsoft.com/vscode/devcontainers/dotnet:6.0-jammy

# Install Mono (for DocFX)
RUN apt install -y apt-transport-https dirmngr gnupg ca-certificates \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" >> /etc/apt/sources.list.d/mono-official-stable.list \
    && apt update \
    && apt install -y mono-devel

# Bug in the installation of .NET 6 in ubuntu https://github.com/dotnet/runtime/issues/79237
ENV DOTNET_ROOT=/usr/share/dotnet

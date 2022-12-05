FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# Add draw.io linux
ADD https://github.com/jgraph/drawio-desktop/releases/download/v17.4.2/drawio-x86_64-17.4.2.AppImage /drawio/drawio-x86_64-17.4.2.AppImage
RUN chmod 777 /drawio/drawio-x86_64-17.4.2.AppImage

# Copy everything
COPY . ./

RUN chmod 777 /scripts/entrypoint.sh

# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app

RUN dotnet tool install --global AzureDiagramGenerator --version $NUGET_VERSION

RUN apt-get update && apt-get install libglib2.0-0 libnss3 libatk1.0-0 libatk-bridge2.0-0 libcups2 libdrm2 libgtk-3-0 libgbm-dev libasound2 xvfb -y

ENV ELECTRON_DISABLE_SECURITY_WARNINGS "true"
ENV DRAWIO_DISABLE_UPDATE "true"
ENV XVFB_DISPLAY ":42"
ENV XVFB_OPTIONS ""

ENTRYPOINT ["/entrypoint.sh"]


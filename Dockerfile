FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# Add draw.io linux
RUN apt-get update && apt-get install libglib2.0-0 libnss3 libatk1.0-0 libatk-bridge2.0-0 libcups2 libdrm2 libgtk-3-0 libgbm-dev libasound2 xvfb -y
ADD https://github.com/jgraph/drawio-desktop/releases/download/v17.4.2/drawio-x86_64-17.4.2.AppImage /drawio/drawio-x86_64-17.4.2.AppImage
RUN chmod 777 /drawio/drawio-x86_64-17.4.2.AppImage

# Copy everything
COPY ./entrypoint.sh ./

RUN chmod 777 /entrypoint.sh

RUN dotnet tool install --global AzureDiagramGenerator --version 0.6.30

ENV ELECTRON_DISABLE_SECURITY_WARNINGS "true"
ENV DRAWIO_DISABLE_UPDATE "true"
ENV XVFB_DISPLAY ":42"
ENV XVFB_OPTIONS ""

ENTRYPOINT ["/entrypoint.sh"]


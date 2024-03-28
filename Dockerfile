FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /
COPY entrypoint.sh .
RUN chmod +x ./entrypoint.sh && \
    apt-get update && apt-get install -y python3-pip && \
    apt-get install -y awscli
WORKDIR /app
ENTRYPOINT ["/entrypoint.sh"]

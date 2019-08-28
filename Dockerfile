FROM mcr.microsoft.com/dotnet/core-nightly/sdk:3.0.100-preview9-alpine3.10 AS build

WORKDIR /app
EXPOSE 80

# copy csproj and restore as distinct layers
COPY podnoms-api/*.csproj ./podnoms-api/
COPY podnoms-common/*.csproj ./podnoms-common/
COPY podnoms-data/*.csproj ./podnoms-data/
WORKDIR /app/podnoms-api
RUN dotnet restore \
    --source https://api.nuget.org/v3/index.json \
    --source https://dotnet.myget.org/F/aspnetcore-dev/api/v3/index.json \
    --source https://dotnet.myget.org/F/aspnetcore-ci-dev/api/v3/index.json \
    --source https://www.myget.org/F/sixlabors/api/v3/index.json

# copy and publish app and libraries
WORKDIR /app/
COPY podnoms-api/. ./podnoms-api/
COPY podnoms-common/. ./podnoms-common/
COPY podnoms-data/. ./podnoms-data/

# build the api project
WORKDIR /app/podnoms-api
RUN dotnet publish -c Release -o out

# spin up the runtime
FROM mcr.microsoft.com/dotnet/core-nightly/aspnet:3.0.0-preview9-alpine3.9 AS runtime
RUN apk add --no-cache --update \
    python \
    ffmpeg \
    libuv \
    curl \
    curl-dev && \
    curl -L https://yt-dl.org/downloads/latest/youtube-dl -o /usr/local/bin/youtube-dl && \
    chmod a+rx /usr/local/bin/youtube-dl && \
    youtube-dl -U

EXPOSE 80
COPY --from=build /app/podnoms-api/out ./
RUN youtube-dl -U

ENTRYPOINT ["dotnet", "podnoms-api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

WORKDIR /app
EXPOSE 80

RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-sos
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters

# copy csproj and restore as distinct layers
COPY podnoms-api/*.csproj ./podnoms-api/
COPY podnoms-common/*.csproj ./podnoms-common/
COPY podnoms-audio-parser/*.csproj ./podnoms-audio-parser/
COPY podnoms-data/*.csproj ./podnoms-data/

WORKDIR /app/podnoms-api

RUN dotnet restore 

# copy and publish app and libraries
WORKDIR /app/

COPY podnoms-api/. ./podnoms-api/
COPY podnoms-common/. ./podnoms-common/
COPY podnoms-audio-parser/. ./podnoms-audio-parser/
COPY podnoms-data/. ./podnoms-data/

# build the api project
WORKDIR /app/podnoms-api
RUN dotnet publish -c Release -o out

# spin up the runtime
FROM fergalmoran/podnoms-alpine-dotnet AS runtime

EXPOSE 80
COPY --from=build /app/podnoms-api/out ./

#install diagnostic tools
COPY --from=build /dotnetcore-tools /opt/dotnetcore-tools
ENV PATH="/opt/dotnetcore-tools:${PATH}"
#RUN /opt/dotnetcore-tools/dotnet-sos install

RUN mkdir ./data
RUN youtube-dl -U

ENTRYPOINT ["dotnet", "podnoms-api.dll"]

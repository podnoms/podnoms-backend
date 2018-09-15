FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /app

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
WORKDIR /app/podnoms-api
RUN dotnet publish -c Release -o out

FROM fergalmoran/podnoms.alpine.base AS runtime
COPY --from=build /app/podnoms-api/out ./
ENTRYPOINT ["dotnet", "podnoms-api.dll"]


#FROM microsoft/dotnet:2.1-sdk-alpine AS build
#WORKDIR /app
#
## copy csproj and restore as distinct layers
#COPY *.sln .
#COPY podnoms-data/*.csproj ./podnoms-data/
#COPY podnoms-common/*.csproj ./podnoms-common/
#COPY podnoms-api/*.csproj ./podnoms-api/
#
#RUN dotnet restore \
#    --source https://api.nuget.org/v3/index.json \
#    --source https://dotnet.myget.org/F/aspnetcore-dev/api/v3/index.json \
#    --source https://dotnet.myget.org/F/aspnetcore-ci-dev/api/v3/index.json \
#    --source https://www.myget.org/F/sixlabors/api/v3/index.json
#
## copy everything else and build app
#COPY . .
#WORKDIR /podnoms-data/
#RUN dotnet build -c Release -o /app
#
#WORKDIR /podnoms-common/
#RUN dotnet build -c Release -o /app
#
#WORKDIR /podnoms-api/
#RUN dotnet build -c Release -o /app
#
#FROM build AS publish
#WORKDIR /app
#EXPOSE 80
#
#RUN dotnet publish -c Release -o out
#
#FROM fergalmoran/podnoms.alpine.base AS runtime
#COPY --from=publish /app/out ./
#
#ENTRYPOINT ["dotnet", "PodNoms.Api.dll"]

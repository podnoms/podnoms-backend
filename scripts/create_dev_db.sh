#!/usr/bin/env bash
source $HOME/.prv/env

PASSWORD=$MSSQLPASSWORD

echo Creating SQL Server

docker run \
    --name podnoms-mssql \
    --restart always \
    -e "ACCEPT_EULA=Y" \
    -e "SA_PASSWORD=$PASSWORD" \
    -v /opt/mssql:/var/opt/mssql \
    -p 1433:1433 \
    -d mcr.microsoft.com/mssql/server
exit

echo Waiting for docker
until [ "$(/usr/bin/docker inspect -f {{.State.Health.Status}} podnoms-mssql)"=="healthy" ]; do
    sleep 0.1
done

echo Creating databases
mssql-cli -S localhost -d master -U sa -P $PASSWORD \ 
    --input_file ./scripts/create_dev_db.sql

echo Migrating
export ASPNETCORE_ENVIRONMENT=Development &&
    dotnet ef database update \
        --project podnoms-common/podnoms-common.csproj \
        --context PodNomsDbContext

echo Initial run
ASPNETCORE_ENVIRONMENT=Development && dotnet watch --project ./podnoms-api/podnoms-api.csproj run

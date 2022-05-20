#!/usr/bin/env bash
HOST=localhost
USER=sa
PASSWORD=H8ckMyB88lz

echo "Closing db connections"
/opt/mssql-tools/bin/sqlcmd \
    -S $HOST \
    -U $USER \
    -P $PASSWORD \
    -d master \
    -i ./scripts/sql/drop.sql

echo "Creating dev db"
/opt/mssql-tools/bin/sqlcmd \
    -S $HOST \
    -d master \
    -U $USER \
    -P $PASSWORD \
    -i ./scripts/sql/create_dev_db.sql

export ASPNETCORE_ENVIRONMENT=Development &&
    dotnet ef database update --context PodNomsDbContext --project ./podnoms-common/podnoms-common.csproj

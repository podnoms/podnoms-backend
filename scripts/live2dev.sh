#!/usr/bin/env bash
source scripts/.env

echo Scripting live db
mssql-scripter \
    -S podnoms.database.windows.net \
    -d PodNoms \
    -U podnoms \
    -P $sqladminpassword \
    --schema-and-data \
    > scripts/live.sql

echo Dropping dev db
/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P H8ckMyB88lz \
    -d master \
    -q "DROP DATABASE PodNoms" \
    < /dev/null

echo Importing live db
/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P H8ckMyB88lz \
    -d master \
    -i scripts/live.sql

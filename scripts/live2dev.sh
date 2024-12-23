#!/usr/bin/env bash
source scripts/.env
source ~/.bash_functions
source ~/.prv/env

az account set --subscription PodNoms

login="podnoms"
password=$sqladminpassword
database="PodNoms"
key=$azurestoragekey
filename=$(uuidgen).bacpac
server=podnoms

if false; then
    az sql db export \
        --admin-password $password \
        --admin-user $login \
        --storage-key $key \
        --storage-key-type StorageAccessKey \
        --storage-uri "https://podnoms.blob.core.windows.net/backups/$filename" \
        --resource-group rg-podnoms-sql \
        --name $database \
        --server $server
fi
filename=f4ea4e92-30e9-4d35-972a-d113c91d2b51.bacpac

echo Downloading blob 
az storage blob download \
    --file /tmp/$filename \
    --account-name podnoms \
    --account-key $key \
    --container-name backups \
    --name $filename

echo Dropping db
sqlcmd -S localhost \
    -U sa \
    -P $MSSQLPASSWORD \
    -d master \
    -Q "DROP DATABASE IF EXISTS $database"

echo Restoring package
sqlpackage \
    /a:Import \
    /tcs:"Data Source=localhost,1433;Initial Catalog=PodNoms;User Id=sa;Password=$MSSQLPASSWORD;TrustServerCertificate=True" \
    /sf:/tmp/f4ea4e92-30e9-4d35-972a-d113c91d2b51.bacpac \
    /p:DatabaseEdition=Premium \
    /p:DatabaseServiceObjective=P6

echo Done

exit

echo Scripting live db
mssql_scripter \
    -S podnoms.database.windows.net \
    -d PodNoms \
    -U podnoms \
    -P $sqladminpassword \
    --schema-and-data \
    >scripts/live.sql

echo Dropping dev db
/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P $MSSQLPASSWORD \
    -d master \
    -q "DROP DATABASE PodNoms" \
    </dev/null

echo Importing live db
/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P $MSSQLPASSWORD \
    -d master \
    -i scripts/live.sql

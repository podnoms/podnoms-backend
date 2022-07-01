#!/usr/bin/env bash
PROJECT_ROOT=/srv/dev/podnoms/podnoms-backend/podnoms-identity
PROJECT_FILE=$PROJECT_ROOT/podnoms-identity.csproj
DATABASE=podnoms_auth

export ASPNETCORE_ENVIRONMENT=Development

echo Dropping existing db
dropdb -f --if-exists -h cluster-master -U postgres $DATABASE

echo Creating new db
createdb -h cluster-master -U postgres $DATABASE

echo Removing migrations
rm -rf $PROJECT_ROOT/Migrations/*

echo Creating new migration set
dotnet ef migrations add "Initial" --project $PROJECT_FILE

echo Migrating new db to latest

dotnet ef database update --project $PROJECT_FILE

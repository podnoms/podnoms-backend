#!/usr/bin/env bash

(trap 'kill 0' SIGINT; dotnet watch --project ./podnoms-api/podnoms-api.csproj run & dotnet watch --project ./podnoms-jobs/podnoms-jobs.csproj run)

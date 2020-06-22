#!/usr/bin/env bash
docker run \
    --name podnoms-mssql \
    --restart always \
    -e "ACCEPT_EULA=Y" \
    -e "SA_PASSWORD=H8ckMyB88lz" \
    -v /opt/mssql:/var/opt/mssql \
    -p 1433:1433 \
    -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu

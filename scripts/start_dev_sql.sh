#!/usr/bin/env bash
docker run \
  --name mssql \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=H8ckMyB88lz" \
  -v /opt/mssql:/var/opt/mssql \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2019-CTP3.0-ubuntu

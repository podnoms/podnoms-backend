#!/usr/bin/env bash
docker run \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=H8ckMyB88lz" \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2019-CTP3.0-ubuntu

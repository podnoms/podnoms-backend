#!/usr/bin/env bash
docker build -t podnoms.azurecr.io/podnoms.api .
docker push podnoms.azurecr.io/podnoms.api

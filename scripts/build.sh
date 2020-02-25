#!/usr/bin/env bash
docker build -t podnoms.azurecr.io/podnoms.api .
az acr login --name podnoms
docker push podnoms.azurecr.io/podnoms.api

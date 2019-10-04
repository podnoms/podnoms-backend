#!/usr/bin/env bash
docker build -t podnoms.azurecr.io/podnoms.jobs . -f Dockerfile.jobs
az acr login --name podnoms
docker push podnoms.azurecr.io/podnoms.jobs

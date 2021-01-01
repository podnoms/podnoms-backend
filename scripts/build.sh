#!/usr/bin/env bash
docker build -t docker.pkg.github.com/podnoms/podnoms-backend/podnoms-api .
az acr login --name podnoms
docker push docker.pkg.github.com/podnoms/podnoms-backend/podnoms-api

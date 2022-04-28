#!/usr/bin/env bash
docker build -t ghcr.io/podnoms/podnoms-api .
docker push ghcr.io/podnoms/podnoms-api

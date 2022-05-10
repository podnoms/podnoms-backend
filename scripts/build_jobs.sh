#!/usr/bin/env bash
docker build -t ghcr.io/podnoms/podnoms-jobs . -f Dockerfile.jobs
docker push ghcr.io/podnoms/podnoms-jobs

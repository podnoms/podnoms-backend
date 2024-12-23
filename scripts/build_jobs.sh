#!/usr/bin/env bash
docker --context default build --push -t ghcr.io/podnoms/podnoms-jobs . -f hosting/Dockerfile.jobs

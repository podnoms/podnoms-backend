#!/usr/bin/env bash

docker run -d \
    -p 6379:6379 \
    --restart always \
    --hostname podnoms-redis \
    --name podnoms-redis \
    redis

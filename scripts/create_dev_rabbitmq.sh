#!/usr/bin/env bash

docker run -d \
    -p 15671:15671 \
    -p 15672:15672 \
    -p 5672:5672 \
    --restart always \
    --hostname podnoms-rabbit \
    --name podnoms-rabbit \
    rabbitmq

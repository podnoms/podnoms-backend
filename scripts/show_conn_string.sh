#!/usr/bin/env bash

az keyvault secret show \
    --name ConnectionStrings--DefaultConnection \
    --vault-name podnomskeys \
    --query value \
    -o tsv

az keyvault secret show \
    --name ConnectionStrings--JobSchedulerConnection \
    --vault-name podnomskeys \
    --query value \
    -o tsv

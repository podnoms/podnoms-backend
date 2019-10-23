#!/bin/bash
#This is the "recommended" way of doing it but it doesn't work.

REGISTRY=podnoms
# REPOSITORIES=(podnoms.jobs podnoms.api podnoms.pages podnoms.web)

REPOSITORIES=(podnoms.api)
for repository in "${REPOSITORIES[@]}"
do
    PURGE_CMD="mcr.microsoft.com/acr/acr-cli:0.1 purge \
    --registry {{.Run.Registry}} --filter '$repository:.*' --untagged --ago 1d"

    echo $PURGE_CMD

    az acr run \
        --cmd "$PURGE_CMD" \
        --registry $REGISTRY \
        /dev/null

    # az acr task create --name purgeTask \
    # --cmd "$PURGE_CMD" \
    # --schedule "0 0 * * *" \
    # --registry podnoms \
    # --context /dev/null

done
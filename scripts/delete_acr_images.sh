#!/bin/bash

# WARNING! This script deletes data!
# Run only if you do not have systems
# that pull images via manifest digest.

# Change to 'true' to enable image delete
ENABLE_DELETE=true

# Modify for your environment
REGISTRY=podnoms
REPOSITORIES=(podnoms.jobs podnoms.api podnoms.pages podnoms.web)

for repository in "${REPOSITORIES[@]}"
do
    echo repository
    # Delete all untagged (orphaned) images
    if [ "$ENABLE_DELETE" = true ]
    then
        az acr repository show-manifests --name $REGISTRY --repository $repository  --query "[?tags[0]==null].digest" -o tsv \
        | xargs -I% az acr repository delete --name $REGISTRY --image $repository@% --yes
    else
        echo "No data deleted."
        echo "Set ENABLE_DELETE=true to enable image deletion of these images in $repository:"
        az acr repository show-manifests --name $REGISTRY --repository $repository --query "[?tags[0]==null]" -o tsv
    fi
done

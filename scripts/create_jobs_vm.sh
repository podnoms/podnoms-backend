docker-machine create \
    --driver generic \
    --generic-ip-address=podnoms-jobs.northeurope.cloudapp.azure.com \
    --generic-ssh-user=fergalm --generic-ssh-key=/home/fergalm/.ssh/id_rsa \
    podnoms-jobs-vm
#!/usr/bin/env bash
source $HOME/.prv/env

/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P $MSSQLPASSWORD \
    -d master \
    -q "DROP DATABASE PodNoms"

/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P $MSSQLPASSWORD \
    -d master \
    -i scripts/live.sql

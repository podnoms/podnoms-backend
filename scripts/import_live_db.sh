#!/usr/bin/env bash

/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P H8ckMyB88lz \
    -d master \
    -q "DROP DATABASE PodNoms"

/opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P H8ckMyB88lz \
    -d master \
    -i live.sql

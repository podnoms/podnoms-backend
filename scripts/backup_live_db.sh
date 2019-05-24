#!/usr/bin/env bash
echo "Locating connection string"
CONNSTR=`./show_conn_string.sh`

echo "Starting backup"
mssql-scripter \
  --connection-string "$CONNSTR" \
  --schema-and-data \
  --script-drop-create
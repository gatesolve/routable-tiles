#!/bin/sh

DB_DIR="${PWD}/helsinki-buildings"
LOG_DIR="${PWD}/helsinki-buildings-logs"

mkdir -p ${LOG_DIR}
sudo docker run \
  -d \
  -v "${DB_DIR}:/var/app/db/" \
  -v "${LOG_DIR}:/var/app/logs/" \
  -p 5000:5000 \
  --name routeable-tiles-api \
  tuukkah/routeable-tiles-api

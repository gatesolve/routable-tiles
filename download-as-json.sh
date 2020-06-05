#!/bin/sh

DB_DIR="${PWD}/helsinki-buildings"

JSON_DIR="${DB_DIR}.json"

rm -Rf "${JSON_DIR}"
mkdir -p "${JSON_DIR}"
cd "${JSON_DIR}"

find "${DB_DIR}/14" -type f -printf '%P\n'|sed -e 's#^\([^.]*\).*#http://localhost:5000/14/\1#'|sort|uniq|xargs wget -x -nH

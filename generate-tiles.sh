#!/bin/sh

# https://download.bbbike.org/osm/bbbike/Helsinkirouting/
# Helsinki.osm.pbf
# https://wiki.openstreetmap.org/wiki/Osmosis/Detailed_Usage_0.48#--log-progress_.28--lp.29

SOURCE_PBF="${HOME}/Downloads/Helsinki.osm.pbf"
FILTERED="${PWD}/helsinki-buildings.osm.pbf"
DB_DIR="${PWD}/helsinki-buildings"

osmosis \
  --read-pbf "${SOURCE_PBF}" \
  --log-progress \
  --tag-filter reject-relations \
  --tag-filter reject-ways \
  --tag-filter accept-nodes entrance=* amenity=loading_dock addr:housenumber=* \
  \
  --read-pbf "${SOURCE_PBF}" \
  --log-progress \
  --tag-filter reject-relations \
  --tag-filter accept-ways building=* building:part=* \
  --used-node \
  \
  --read-pbf "${SOURCE_PBF}" \
  --log-progress \
  --tag-filter accept-relations type=associated_entrance building=* building:part=* \
  --used-way \
  --used-node \
  \
  --merge --merge \
  --write-pbf "${FILTERED}"

sudo rm -rf "${DB_DIR}"
mkdir -p "${DB_DIR}"
sudo docker run \
  --rm \
  -v "${DB_DIR}:/var/app/db/" \
  -v "${FILTERED}:/var/app/source/input.osm.pbf" \
  tuukkah/routeable-tiles

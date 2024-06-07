#!/bin/sh
apk add curl
echo "Waiting for Elasticsearch availability"
until curl -s -u elastic:${ELASTIC__INITIAL_PASSWORD} http://elasticsearch:9200/_cluster/health | grep -vq '"status":"red"'; do sleep 30; done
echo "Setting kibana_system password"
until curl -s -X POST -u elastic:${ELASTIC__INITIAL_PASSWORD} -H "Content-Type: application/json" http://elasticsearch:9200/_security/user/kibana_system/_password -d '{"password":"${ELASTIC__INITIAL_PASSWORD}"}' | grep -q "^{}"; do sleep 10; done
echo "Storing cluster_uuid"
until curl -s -u elastic:${ELASTIC__INITIAL_PASSWORD} http://elasticsearch:9200/ | grep cluster_uuid | awk '{split($0,a,":"); print a[2]}' | sed 's/[\", ]//g' > /usr/share/setup/cluster_uuid; do sleep 10; done
echo "All done!"

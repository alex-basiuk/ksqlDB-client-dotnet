#!/bin/bash
echo "Waiting for ksqlDB to be up and running..."
timeout 2m sh -c 'while [[ "$(curl -s -o /dev/null -w ''%{http_code}'' http://primary-ksqldb-server:8088/info)" != "200" ]]; do sleep 5; done' || false
echo "Creating orders_stream..."
curl -X POST http://primary-ksqldb-server:8088/ksql -H "Accept: application/vnd.ksql.v1+json" -d "{ \"ksql\": \"CREATE STREAM orders_stream (ordertime BIGINT, orderid INT, itemid VARCHAR, orderunits DOUBLE, address STRUCT<city VARCHAR, state VARCHAR, zipcode BIGINT>) WITH (KAFKA_TOPIC='orders_topic', VALUE_FORMAT='JSON', PARTITIONS=1, REPLICAS=1);\",\"streamsProperties\": {}}"
echo "Creating users_table..."
curl -X POST http://primary-ksqldb-server:8088/ksql -H "Accept: application/vnd.ksql.v1+json" -d "{ \"ksql\": \"CREATE TABLE users_table (registertime BIGINT, gender VARCHAR, regionid VARCHAR, userid VARCHAR PRIMARY KEY, interests ARRAY<STRING>, contactInfo MAP<STRING, STRING>) WITH (kafka_topic='users_topic', value_format='JSON', PARTITIONS=1, REPLICAS=1);\",\"streamsProperties\": {}}"
echo "All done"
tail -f /dev/null
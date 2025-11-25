#!/bin/bash
set -o errexit
set -o pipefail
set -o nounset

echo "[kafka] Topics setup"

declare -a TOPICS=()
TOPICS[0]=demo.topic

echo "[kafka] Waiting for Kafka port to open..."
timeout 20 bash -c 'until printf "" 2>>/dev/null >>/dev/tcp/$0/$1; do sleep 1; done' kafka 9092

for topic in "${TOPICS[@]}"
do
  echo "[kafka] Adding topic - $topic"
  sh /opt/bitnami/kafka/bin/kafka-topics.sh \
    --bootstrap-server=kafka:9092 \
    --create \
    --partitions=8 \
    --replication-factor=1 \
    --if-not-exists \
    --topic=$topic
done


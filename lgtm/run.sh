#!/bin/sh

docker ps 2>&1 | grep -q zipkin

if [ $? = 0 ]; then
    echo "Please stop the running Zipkin Docker instance and try again"
    exit 1
fi

exec docker run -p 3000:3000 -p 4317:4317 -p 4318:4318 -p 9411:9411 --rm --name otel-lgtm -ti otel-lgtm:local
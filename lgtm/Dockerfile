FROM grafana/otel-lgtm

COPY otelcol-config.yaml /otel-lgtm/
COPY grafana-dashboard-*.json /otel-lgtm/
COPY grafana-dashboards.yaml /otel-lgtm/grafana/conf/provisioning/dashboards/grafana-dashboards.yaml

RUN sed -i '/4318/a echo " - 4911: Zipkin HTTP endpoint"' /otel-lgtm/run-all.sh
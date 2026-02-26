import http from 'k6/http';
import { sleep } from 'k6';

// hex ID 생성
function randomHex(bytes) {
  const chars = 'abcdef0123456789';
  let result = '';
  for (let i = 0; i < bytes * 2; i++) {
    result += chars[Math.floor(Math.random() * chars.length)];
  }
  return result;
}

export default function () {

  const traceId = randomHex(16);
  const spanId = randomHex(8);

  const start = Date.now() * 1e6;

  // API 호출 (trace 연결)
  http.get("http://localhost:5000/work", {
    headers: {
      "traceparent": `00-${traceId}-${spanId}-01`
    }
  });

  sleep(1);

  const end = Date.now() * 1e6;

  // OTLP HTTP 직접 전송
  const body = JSON.stringify({
    resourceSpans: [{
      resource: {
        attributes: [{
          key: "service.name",
          value: { stringValue: "k6-manual" }
        }]
      },
      scopeSpans: [{
        spans: [{
          traceId: traceId,
          spanId: spanId,
          name: "k6-parent-span",
          kind: 1,
          startTimeUnixNano: String(start),
          endTimeUnixNano: String(end)
        }]
      }]
    }]
  });

  http.post(
    "http://localhost:4318/v1/traces",
    body,
    { headers: { "Content-Type": "application/json" } }
  );
}

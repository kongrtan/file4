import http from 'k6/http';
import { sleep } from 'k6';
// 공식 k6 jslib에서 제공하는 트레이싱 도우미
import { HttpInstrumentation } from 'https://jslib.k6.io/k6-jslib-http-instrumentation/1.0.0/index.js';

// 1. Instrumentation 설정
const instrumentation = new HttpInstrumentation({
    exporter: {
        endpoint: 'localhost:4317', // OTEL Collector 주소
    },
    propagator: 'w3c', // traceparent 헤더를 사용하여 서버와 TraceID 연동
});

export default function () {
    const url = 'https://api.example.com/test';

    // 2. 계측된(Instrumented) 호출
    // 이 방식은 내부적으로 TraceID를 생성하여 서버에 전파하고,
    // k6측 스팬도 생성하여 Collector로 보냅니다.
    instrumentation.instrumentHTTP(() => {
        return http.get(url);
    }, {
        name: 'My_Manual_Span', // 의도한 스팬 이름
        attributes: {
            'custom.data': 'my-value', // 의도한 속성만 추가
        },
    });

    sleep(1);
}

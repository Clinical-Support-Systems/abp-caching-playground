import http from "k6/http";
import { sleep } from "k6";

// Custom metrics
const responseTrend = new Trend("response_time");
const errorRate = new Rate("error_rate");
const throughputRate = new Rate("throughput_rate");

export let options = {
    scenarios: {
        burst: {
            executor: "ramping-arrival-rate",
            startRate: 10,
            timeUnit: "1s",
            preAllocatedVUs: 50,
            maxVUs: 500,
            stages: [
                { duration: "1m", target: 10 },     // Normal load, 10 requests per second
                { duration: "30s", target: 300 },   // Sudden burst of traffic, quickly ramp up to 300 RPS
                { duration: "1m", target: 300 },    // Sustain the burst for a short period
                { duration: "30s", target: 10 },    // Return to normal load
                { duration: "2m", target: 10 }      // Maintain normal load to verify recovery
            ]
        }
    },
    thresholds: {
        // Define SLA thresholds
        'http_req_duration': ["p95<500"],   // 95% of requests must complete below 500ms
        'error_rate': ["rate<0.05"],        // Error rate must be less than 5%
        'http_req_failed': ["rate<0.05"]    // HTTP errors must be less than 5%
    }
};

export default function () {
    const baseUrl = __ENV.APP_HOST || "https://host.docker.internal:44319";
    console.log(`Testing against URL: ${baseUrl}`);

    const response = http.get(baseUrl);

    // Check that the response is valid
    const checkSuccess = check(response, {
        'status is 200': (r) => r.status === 200,
        'response time < 200ms': (r) => r.timings.duration < 200,
        'response body contains expected data': (r) => r.body.includes("products")
    });

    // Record custom metrics
    responseTrend.add(response.timings.duration);
    errorRate.add(!checkSuccess);
    throughputRate.add(1);

    sleep(1);
}
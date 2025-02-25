import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
    scenarios: {
        cached_reads: {
            executor: "ramping-vus",
            startVUs: 0,
            stages: [
                { duration: "20s", target: 100 },
                { duration: "30s", target: 100 },
                { duration: "20s", target: 0 }
            ],
            gracefulRampDown: "0s"
        }
    }
};

export default function () {

    // Use a configurable host URL via environment variable
    const baseUrl = __ENV.APP_HOST || "https://host.docker.internal:44319";

    console.log(`Testing against URL: ${baseUrl}`);

    const response = http.get(baseUrl);
    check(response, {
        'status is 200': (r) => r.status === 200
    });

    sleep(1);
}
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
    const params = {
        headers: {
            'Cache-Implementation': __ENV.CACHE_IMPL // 'redis' or 'fusion'
        }
    };

    const response = http.get("http://localhost:5000/api/cache-test/item/1", params);
    check(response, {
        'status is 200': (r) => r.status === 200
    });

    sleep(1);
}
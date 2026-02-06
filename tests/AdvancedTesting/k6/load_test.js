import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '10s', target: 10 }, // Ramp up to 10 users
        { duration: '20s', target: 10 }, // Stay at 10 users
        { duration: '10s', target: 0 },  // Ramp down
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
        http_req_failed: ['rate<0.01'],   // Error rate < 1%
    },
};

export default function () {
    const payload = JSON.stringify({
        fullName: 'Load Test User',
        email: 'load@test.com',
        age: 25
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
            'X-Api-Version': '1.0'
        },
    };

    // Ensure application is running on port 5000 (standard for dotnet run)
    const res = http.post('http://localhost:5000/api/v1/users', payload, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'id returned': (r) => JSON.parse(r.body).id !== undefined,
    });

    sleep(1);
}

import http from 'k6/http';
import { sleep } from 'k6';

export let options = {
    vus: 1,  // 1 virtual user
    duration: '1s',  // Run for 1 second
};

export default function () {
    console.log('K6 verification script executed successfully.');
    sleep(1);
}

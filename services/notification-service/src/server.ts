import {createApp} from './app';
import {env} from './config/env';
import {startConsumers} from './consumers';

// import { startEmailDeliveryWorker } from './workers/emailDelivery.worker';

async function main(): Promise<void> {
  const app = createApp();

  app.listen(env.http.port, () => {
    console.log(`[http] Notification Service REST API listening on :${env.http.port}`);
  });

  await startConsumers();

  // Bật khi có event dùng cơ chế worker (không phải OTP — xem
  // src/workers/emailDelivery.worker.ts):
  // startEmailDeliveryWorker();
}

main().catch((err) => {
  console.error('[bootstrap] Failed to start Notification Service:', err);
  process.exit(1);
});

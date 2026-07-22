import {kafka} from '../kafka/client';
import {createOtpRequestedConsumer, runOtpRequestedConsumer} from './otpRequested.consumer';

export async function startConsumers(): Promise<void> {
  const otpConsumer = createOtpRequestedConsumer(kafka);
  await otpConsumer.connect();
  await runOtpRequestedConsumer(otpConsumer);

  console.log('[consumers] Notification Service consumers started');

  const shutdown = async () => {
    await otpConsumer.disconnect();
    process.exit(0);
  };
  process.once('SIGINT', shutdown);
  process.once('SIGTERM', shutdown);
}

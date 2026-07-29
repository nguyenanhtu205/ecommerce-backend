import {EachMessagePayload} from 'kafkajs';
import {kafka} from '../kafka/client';
import {env} from '../config/env';
import {handleOtpRequestedMessage, OTP_REQUESTED_TOPIC} from './otpRequested.consumer';
import {handleShopActivatedMessage, SHOP_ACTIVATED_TOPIC} from './shopActivated.consumer';

type MessageHandler = (payload: EachMessagePayload) => Promise<void>;

const TOPIC_HANDLERS: Record<string, MessageHandler> = {
  [OTP_REQUESTED_TOPIC]: handleOtpRequestedMessage,
  [SHOP_ACTIVATED_TOPIC]: handleShopActivatedMessage,
};

export async function startConsumers(): Promise<void> {
  const consumer = kafka.consumer({groupId: env.kafka.groupId});

  await consumer.connect();
  await consumer.subscribe({topics: Object.keys(TOPIC_HANDLERS), fromBeginning: false});

  await consumer.run({
    eachMessage: async (payload) => {
      const handler = TOPIC_HANDLERS[payload.topic];
      if (!handler) {
        console.warn(`[consumers] Không có handler đăng ký cho topic: ${payload.topic}`);
        return;
      }
      await handler(payload);
    },
  });

  console.log('[consumers] Notification Service consumer started, topics:', Object.keys(TOPIC_HANDLERS));

  const shutdown = async () => {
    console.log('[consumers] Shutting down...');
    await consumer.disconnect();
    process.exit(0);
  };
  process.once('SIGINT', shutdown);
  process.once('SIGTERM', shutdown);
}

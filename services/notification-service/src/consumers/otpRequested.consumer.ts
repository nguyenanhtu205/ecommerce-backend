import {Consumer, EachMessagePayload, Kafka} from 'kafkajs';
import {env} from '../config/env';
import {otpRequestedSchema} from '../types/events/otp-requested.event';
import {otpNotificationService} from '../services/otpNotification.service';
import {NOTIFICATION_EVENTS} from '../config/notificationEvents.config';

const TOPIC = NOTIFICATION_EVENTS.OTP_REQUESTED.eventType;

export function createOtpRequestedConsumer(kafka: Kafka): Consumer {
  return kafka.consumer({groupId: env.kafka.groupId});
}

export async function runOtpRequestedConsumer(consumer: Consumer): Promise<void> {
  await consumer.subscribe({topic: TOPIC, fromBeginning: false});

  await consumer.run({
    eachMessage: async ({topic, partition, message}: EachMessagePayload) => {
      await handleMessage(topic, partition, message.value?.toString('utf-8'), message.offset);
    },
  });
}

async function handleMessage(
  topic: string,
  partition: number,
  raw: string | undefined,
  offset: string,
): Promise<void> {
  if (!raw) return;

  let payload: unknown;
  try {
    payload = JSON.parse(raw);
  } catch {
    console.error('[otp-requested] The message is not valid JSON. Skipping');
    return;
  }

  const parsed = otpRequestedSchema.safeParse(payload);
  if (!parsed.success) {
    console.error('[otp-requested] Payload does not match the schema:', parsed.error.flatten());
    return;
  }

  const eventId = `${topic}-${partition}-${offset}`;

  await otpNotificationService.handleOtpRequested(eventId, parsed.data);
}

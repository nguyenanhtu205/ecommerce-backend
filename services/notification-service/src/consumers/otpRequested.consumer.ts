import {EachMessagePayload} from 'kafkajs';
import {otpRequestedSchema} from '../types/events/otp-requested.event';
import {otpNotificationService} from '../services/otpNotification.service';
import {NOTIFICATION_EVENTS} from '../config/notificationEvents.config';

export const OTP_REQUESTED_TOPIC = NOTIFICATION_EVENTS.OTP_REQUESTED.eventType;

export async function handleOtpRequestedMessage({topic, partition, message}: EachMessagePayload): Promise<void> {
  const raw = message.value?.toString('utf-8');
  if (!raw) return;

  let payload: unknown;
  try {
    payload = JSON.parse(raw);
  } catch {
    console.error('[otp-requested] Message không phải JSON hợp lệ, bỏ qua');
    return;
  }

  const parsed = otpRequestedSchema.safeParse(payload);
  if (!parsed.success) {
    console.error('[otp-requested] Payload không khớp schema:', parsed.error.flatten());
    return;
  }

  const eventId = `${topic}-${partition}-${message.offset}`;
  await otpNotificationService.handleOtpRequested(eventId, parsed.data);
}

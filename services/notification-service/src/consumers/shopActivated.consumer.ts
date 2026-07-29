import {EachMessagePayload} from 'kafkajs';
import {shopActivatedSchema} from '../types/events/shop-activated.event';
import {shopActivatedNotificationService} from '../services/shopActivatedNotification.service';
import {NOTIFICATION_EVENTS} from '../config/notificationEvents.config';

export const SHOP_ACTIVATED_TOPIC = NOTIFICATION_EVENTS.SHOP_ACTIVATED.eventType;

export async function handleShopActivatedMessage({topic, partition, message}: EachMessagePayload): Promise<void> {
  const raw = message.value?.toString('utf-8');
  if (!raw) return;

  let payload: unknown;
  try {
    payload = JSON.parse(raw);
  } catch {
    console.error('[shop-activated] The message is not valid JSON. Skipping');
    return;
  }

  const parsed = shopActivatedSchema.safeParse(payload);
  if (!parsed.success) {
    console.error('[shop-activated] Payload does not match the schema:', parsed.error.flatten());
    return;
  }

  const eventId = `${topic}-${partition}-${message.offset}`;
  await shopActivatedNotificationService.handleShopActivated(eventId, parsed.data);
}

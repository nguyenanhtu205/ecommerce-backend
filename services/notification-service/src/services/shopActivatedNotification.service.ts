import {withTransaction} from '../db/pool';
import {ShopActivatedPayload} from '../types/events/shop-activated.event';
import {NOTIFICATION_EVENTS} from "../config/notificationEvents.config";
import {DuplicateEventError, processedEventRepository} from "../repositories/processedEvent.repository";
import {messageDeliveryRepository} from "../repositories/messageDelivery.repository";
import {renderShopActivatedEmail} from "../templates/shopActivated.template";
import {emailService} from "./email.service";

const EVENT_DEF = NOTIFICATION_EVENTS.SHOP_ACTIVATED;
const TEMPLATE_CODE = 'shop_activated';

export const shopActivatedNotificationService = {
  async handleShopActivated(eventId: string, payload: ShopActivatedPayload): Promise<void> {
    console.log(`[shop-activated] Receive event ${eventId}, purpose=${payload.purpose}`);
    const idempotencyKey = `otp:${payload.purpose}:${eventId}`;

    let deliveryId: string;
    try {
      deliveryId = await withTransaction(async (client) => {
        await processedEventRepository.markProcessed(client, eventId, EVENT_DEF.eventType);

        return messageDeliveryRepository.create(client, {
          idempotencyKey,
          sourceEventId: eventId,
          channel: 'EMAIL',
          recipient: payload.email,
          templateCode: TEMPLATE_CODE,
          payload: {purpose: payload.purpose, activatedAt: payload.activatedAt}
        });
      });
    } catch (err) {
      if (err instanceof DuplicateEventError) {
        console.warn(`[shop-activated] Skipping duplicate event: ${eventId}`);
        return;
      }
      throw err;
    }

    const email = renderShopActivatedEmail(payload);

    try {
      await emailService.send({to: payload.email, subject: email.subject, html: email.html});
      await messageDeliveryRepository.markSent(deliveryId);
      console.log(`[shop-activated] Sent email to ${payload.email} (delivery=${deliveryId})`);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unknown error while sending email';
      console.error(`[shop-activated] Failed to send email for delivery ${deliveryId}:`, message);
      await messageDeliveryRepository.markFailed(deliveryId, message);
    }
  }
}

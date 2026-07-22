import {withTransaction} from '../db/pool';
import {DuplicateEventError, processedEventRepository} from '../repositories/processedEvent.repository';
import {messageDeliveryRepository} from '../repositories/messageDelivery.repository';
import {renderOtpRequestedEmail} from '../templates/otpRequested.template';
import {emailService} from './email.service';
import {OtpRequestedPayload} from '../types/events/otp-requested.event';
import {NOTIFICATION_EVENTS} from '../config/notificationEvents.config';

const EVENT_DEF = NOTIFICATION_EVENTS.OTP_REQUESTED;
const TEMPLATE_CODE = 'otp_requested';

export const otpNotificationService = {
  async handleOtpRequested(eventId: string, payload: OtpRequestedPayload): Promise<void> {
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
          payload: {purpose: payload.purpose, requestedAt: payload.requestedAt},
        });
      });
    } catch (err) {
      if (err instanceof DuplicateEventError) {
        console.warn(`[otp-requested] Skipping duplicate event: ${eventId}`);
        return;
      }
      throw err;
    }

    const email = renderOtpRequestedEmail(payload);

    try {
      await emailService.send({to: payload.email, subject: email.subject, html: email.html});
      await messageDeliveryRepository.markSent(deliveryId);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unknown error while sending email';
      console.error(`[otp-requested] Failed to send email for delivery ${deliveryId}:`, message);
      await messageDeliveryRepository.markFailed(deliveryId, message);
    }
  },
};

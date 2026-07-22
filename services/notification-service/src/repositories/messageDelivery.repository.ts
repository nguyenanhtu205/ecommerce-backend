import {PoolClient} from 'pg';
import {pool} from '../db/pool';

export type DeliveryChannel = 'EMAIL';

export interface CreateMessageDeliveryInput {
  idempotencyKey: string;
  sourceEventId: string;
  channel: DeliveryChannel;
  recipient: string;
  templateCode: string;
  payload: Record<string, unknown>;
}

export const messageDeliveryRepository = {
  async create(client: PoolClient, input: CreateMessageDeliveryInput): Promise<string> {
    const result = await client.query<{ id: string }>(
      `INSERT INTO message_deliveries
       (id, idempotency_key, source_event_id, channel, recipient, template_code, payload, status, created_at)
       VALUES (gen_random_uuid(), $1, $2, $3, $4, $5, $6, 'PENDING', now()) RETURNING id`,
      [
        input.idempotencyKey,
        input.sourceEventId,
        input.channel,
        input.recipient,
        input.templateCode,
        JSON.stringify(input.payload),
      ],
    );

    return result.rows[0].id;
  },

  async markSent(deliveryId: string): Promise<void> {
    await pool.query(
      `UPDATE message_deliveries
       SET status  = 'SENT',
           sent_at = now()
       WHERE id = $1`,
      [deliveryId],
    );
  },

  async markFailed(deliveryId: string, errorMessage: string): Promise<void> {
    await pool.query(
      `UPDATE message_deliveries
       SET status        = 'FAILED',
           error_message = $2
       WHERE id = $1`,
      [deliveryId, errorMessage],
    );
  },
};

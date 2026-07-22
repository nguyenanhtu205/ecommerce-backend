import {pool} from '../db/pool';
import {emailService} from '../services/email.service';
import {TEMPLATE_REGISTRY} from '../templates/registry';

const POLL_INTERVAL_MS = 5000;
const BATCH_SIZE = 20;
const MAX_ATTEMPTS = 5;

export function startEmailDeliveryWorker(): NodeJS.Timeout {
  return setInterval(() => {
    processPendingDeliveries().catch((err) => {
      console.error('[email-delivery-worker] Error processing batch:', err);
    });
  }, POLL_INTERVAL_MS);
}

async function processPendingDeliveries(): Promise<void> {
  const client = await pool.connect();
  let rows: Array<{ id: string; recipient: string; template_code: string; payload: unknown }>;

  try {
    await client.query('BEGIN');

    const result = await client.query(
      `SELECT id, recipient, template_code, payload
       FROM message_deliveries
       WHERE channel = 'EMAIL'
         AND status IN ('PENDING', 'FAILED')
         AND attempts < $1
       ORDER BY created_at
         LIMIT $2
         FOR
      UPDATE SKIP LOCKED`,
      [MAX_ATTEMPTS, BATCH_SIZE],
    );
    rows = result.rows;

    await client.query('COMMIT');
  } catch (err) {
    await client.query('ROLLBACK');
    client.release();
    throw err;
  }
  client.release();

  for (const row of rows) {
    await sendOne(row);
  }
}

async function sendOne(row: {
  id: string;
  recipient: string;
  template_code: string;
  payload: unknown;
}): Promise<void> {
  const render = TEMPLATE_REGISTRY[row.template_code];
  if (!render) {
    console.error(`[email-delivery-worker] Template not found: ${row.template_code}`);
    await pool.query(
      `UPDATE message_deliveries
       SET status        = 'FAILED',
           error_message = $2,
           attempts      = attempts + 1
       WHERE id = $1`,
      [row.id, `Unknown template_code: ${row.template_code}`],
    );
    return;
  }

  try {
    const email = render(row.payload);
    await emailService.send({to: row.recipient, subject: email.subject, html: email.html});
    await pool.query(
      `UPDATE message_deliveries
       SET status   = 'SENT',
           sent_at  = now(),
           attempts = attempts + 1
       WHERE id = $1`,
      [row.id],
    );
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Unknown error';
    await pool.query(
      `UPDATE message_deliveries
       SET status        = 'FAILED',
           error_message = $2,
           attempts      = attempts + 1
       WHERE id = $1`,
      [row.id, message],
    );
  }
}

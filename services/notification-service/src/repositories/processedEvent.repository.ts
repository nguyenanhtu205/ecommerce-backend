import {PoolClient} from 'pg';

export class DuplicateEventError extends Error {
  constructor(public readonly eventId: string) {
    super(`Event ${eventId} has already been processed`);
    this.name = 'DuplicateEventError';
  }
}

export const processedEventRepository = {
  async markProcessed(client: PoolClient, eventId: string, eventType: string): Promise<void> {
    const result = await client.query(
      `INSERT INTO processed_events (id, event_id, event_type, processed_at)
       VALUES (gen_random_uuid(), $1, $2, now()) ON CONFLICT (event_id) DO NOTHING`,
      [eventId, eventType],
    );

    if (result.rowCount === 0) {
      throw new DuplicateEventError(eventId);
    }
  },
};

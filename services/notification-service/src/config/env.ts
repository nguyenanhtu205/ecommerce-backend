import 'dotenv/config';

function required(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required env var: ${name}`);
  }
  return value;
}

export const env = {
  http: {
    port: Number(process.env.PORT ?? 3000),
  },
  kafka: {
    brokers: required('KAFKA_BROKERS').split(','),
    clientId: process.env.KAFKA_CLIENT_ID ?? 'notification-service',
    groupId: process.env.KAFKA_GROUP_ID ?? 'notification-service-group',
  },
  postgres: {
    connectionString: required('DATABASE_URL'),
  },
  smtp: {
    host: required('SMTP_HOST'),
    port: Number(process.env.SMTP_PORT ?? 587),
    user: required('SMTP_USER'),
    pass: required('SMTP_PASS'),
    from: process.env.SMTP_FROM ?? 'no-reply@example.com',
  },
};

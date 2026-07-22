import {z} from 'zod';

export const otpRequestedSchema = z.object({
  email: z.string().email(),
  code: z.string().min(1),
  purpose: z.enum(['register', 'reset-password']),
  requestedAt: z.string(),
});

export type OtpRequestedPayload = z.infer<typeof otpRequestedSchema>;

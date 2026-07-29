import {z} from 'zod';

export const shopActivatedSchema = z.object({
  email: z.string().email(),
  purpose: z.enum(['activate-shop']),
  activatedAt: z.string(),
});

export type ShopActivatedPayload = z.infer<typeof shopActivatedSchema>;

interface RenderedEmail {
  subject: string;
  html: string;
}

type TemplateRenderer = (payload: unknown) => RenderedEmail;

export const TEMPLATE_REGISTRY: Record<string, TemplateRenderer> = {
  // order_confirmed: renderOrderConfirmedEmail,
};

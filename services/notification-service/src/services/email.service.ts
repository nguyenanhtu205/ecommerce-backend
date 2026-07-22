import nodemailer, {Transporter} from 'nodemailer';
import {env} from '../config/env';

let transporter: Transporter | null = null;

function getTransporter(): Transporter {
  if (!transporter) {
    transporter = nodemailer.createTransport({
      host: env.smtp.host,
      port: env.smtp.port,
      secure: env.smtp.port === 465,
      auth: {user: env.smtp.user, pass: env.smtp.pass},
    });
  }
  return transporter;
}

export interface SendEmailInput {
  to: string;
  subject: string;
  html: string;
}

export const emailService = {
  async send(input: SendEmailInput): Promise<void> {
    await getTransporter().sendMail({
      from: env.smtp.from,
      to: input.to,
      subject: input.subject,
      html: input.html,
    });
  },
};

import {OtpRequestedPayload} from '../types/events/otp-requested.event';

interface RenderedEmail {
  subject: string;
  html: string;
}

const COPY: Record<OtpRequestedPayload['purpose'], { subject: string; heading: string }> = {
  register: {
    subject: 'Xác nhận địa chỉ email của bạn',
    heading: 'Hoàn tất đăng ký tài khoản',
  },
  'reset-password': {
    subject: 'Mã xác nhận đặt lại mật khẩu',
    heading: 'Đặt lại mật khẩu',
  },
};

export function renderOtpRequestedEmail(payload: OtpRequestedPayload): RenderedEmail {
  const copy = COPY[payload.purpose];

  return {
    subject: copy.subject,
    html: `
      <div style="font-family: sans-serif; max-width: 480px; margin: 0 auto;">
        <h2>${copy.heading}</h2>
        <p>Mã xác thực của bạn là:</p>
        <p style="font-size: 32px; font-weight: bold; letter-spacing: 4px;">${payload.code}</p>
        <p>Mã có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
      </div>
    `,
  };
}

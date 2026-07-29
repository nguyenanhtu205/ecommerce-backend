import {ShopActivatedPayload} from '../types/events/shop-activated.event';
import {formatVietnamDateTime} from '../utils/formatVietnamDateTime';

interface RenderedEmail {
  subject: string;
  html: string;
}

const COPY: Record<ShopActivatedPayload['purpose'], { subject: string; heading: string }> = {
  'activate-shop': {
    subject: 'Shop của bạn đã được kích hoạt',
    heading: 'Chúc mừng! Shop của bạn đã sẵn sàng hoạt động',
  },
};

export function renderShopActivatedEmail(payload: ShopActivatedPayload): RenderedEmail {
  const copy = COPY[payload.purpose];

  return {
    subject: copy.subject,
    html: `
      <div style="font-family: sans-serif; max-width: 480px; margin: 0 auto;">
        <h2>${copy.heading}</h2>
        <p>Xin chào,</p>
        <p>
          Chúng tôi xin thông báo rằng shop của bạn đã được kích hoạt thành công.
        </p>
        <p>
          Từ bây giờ, bạn có thể:
        </p>
        <ul>
          <li>Đăng sản phẩm để bán.</li>
          <li>Quản lý thông tin shop và sản phẩm.</li>
          <li>Tiếp nhận và xử lý đơn hàng từ khách hàng.</li>
        </ul>
        <p>
          Thời điểm kích hoạt:
          <strong>${formatVietnamDateTime(payload.activatedAt)}</strong>
        </p>
        <p>
          Chúc bạn kinh doanh thuận lợi và đạt được nhiều đơn hàng!
        </p>
        <p>
          Trân trọng,<br />
          Đội ngũ hỗ trợ
        </p>
      </div>
    `,
  };
}

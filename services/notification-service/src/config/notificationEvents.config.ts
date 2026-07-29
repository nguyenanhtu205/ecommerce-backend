import {NotificationCategory} from '../types/notification-category';

export interface NotificationEventDefinition {
  eventType: string;
  category: NotificationCategory | null;
  createInApp: boolean;
  createEmail: boolean;
  checkPreference: boolean;
}

export const NOTIFICATION_EVENTS = {
  OTP_REQUESTED: {
    eventType: 'notification.otp-requested.v1',
    category: null,
    createInApp: false,
    createEmail: true,
    checkPreference: false,
  },

  SHOP_ACTIVATED: {
    eventType: 'notification.shop-activated.v1',
    category: null,
    createInApp: false,
    createEmail: false,
    checkPreference: false,
  }

  // ORDER_CONFIRMED: {
  //   eventType: 'order.confirmed.v1',
  //   category: 'ORDER',
  //   createInApp: true,
  //   createEmail: true,
  //   checkPreference: true,
  // },
} as const satisfies Record<string, NotificationEventDefinition>;

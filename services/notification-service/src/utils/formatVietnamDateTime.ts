export function formatVietnamDateTime(date: string): string {
  const formatter = new Intl.DateTimeFormat('vi-VN', {
    timeZone: 'Asia/Ho_Chi_Minh',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
    day: 'numeric',
    month: 'numeric',
    year: 'numeric',
  });

  const parts = formatter.formatToParts(new Date(date));

  const get = (type: string) =>
    parts.find((p) => p.type === type)?.value ?? '';

  const hour = get('hour');
  const minute = get('minute');
  const dayPeriod = get('dayPeriod').toLowerCase();

  return `${hour}:${minute} ${dayPeriod}, ngày ${get('day')}/${get('month')}/${get('year')}`;
}

const LOCALE = 'sr-Latn';

const dinFormatter = new Intl.NumberFormat(LOCALE, { maximumFractionDigits: 0 });

/** 30000 → "30.000 din" */
export function formatDin(amount: number): string {
  return `${dinFormatter.format(amount)} din`;
}

/** "2026-11-14" → a Date at midnight UTC. Calendar dates are always handled in UTC. */
export function parseIsoDate(isoDate: string): Date {
  const [year, month, day] = isoDate.split('-').map(Number);
  return new Date(Date.UTC(year, month - 1, day)); // 💬 months are 0-based in JavaScript: November is 10
}

const longDateFormatter = new Intl.DateTimeFormat(LOCALE, {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
});

/** "2026-11-14" → "subota, 14. novembar 2026." */
export function formatLongDate(isoDate: string): string {
  return longDateFormatter.format(parseIsoDate(isoDate));
}

const shortDateFormatter = new Intl.DateTimeFormat(LOCALE, {
  weekday: 'short',
  day: 'numeric',
  month: 'numeric',
  timeZone: 'UTC',
});

/** "2026-11-14" → "sub, 14.11." */
export function formatShortDate(isoDate: string): string {
  const parts = shortDateFormatter.formatToParts(parseIsoDate(isoDate));
  const part = (type: Intl.DateTimeFormatPartTypes) =>
    parts.find((p) => p.type === type)?.value ?? '';
  return `${part('weekday')}, ${part('day')}.${part('month')}.`;
}

/** "17:00:00" → "17:00" */
export function formatTime(time: string): string {
  return time.slice(0, 5);
}

/** ("17:00:00", "19:00:00") → "17:00–19:00" */
export function formatTimeRange(start: string, end: string): string {
  return `${formatTime(start)}–${formatTime(end)}`; // 💬 an en dash (–), not a hyphen, as in the design
}

export interface PluralForms {
  one: string;
  few: string;
  other: string;
}

const pluralRules = new Intl.PluralRules(LOCALE);

/** pluralSr(2, { one: 'slobodan termin', few: 'slobodna termina', other: 'slobodnih termina' }) → "2 slobodna termina" */
export function pluralSr(count: number, forms: PluralForms): string {
  const category = pluralRules.select(count); // 💬 'one' | 'few' | 'other' for Serbian
  const form = category === 'one' ? forms.one : category === 'few' ? forms.few : forms.other;
  return `${count} ${form}`;
}

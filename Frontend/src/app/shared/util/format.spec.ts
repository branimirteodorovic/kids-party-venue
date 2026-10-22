import { describe, expect, it } from 'vitest';
import { formatDin, formatLongDate, formatShortDate, formatTimeRange, pluralSr } from './format';

describe('formatDin', () => {
  it('groups thousands with a dot and adds "din"', () => {
    expect(formatDin(30000)).toBe('30.000 din');
  });

  it('rounds to whole dinars', () => {
    expect(formatDin(2499.6)).toBe('2.500 din');
  });
});

describe('formatLongDate', () => {
  it('writes the weekday, day, month and year in Serbian', () => {
    expect(formatLongDate('2026-11-14')).toBe('subota, 14. novembar 2026.');
  });
});

describe('formatShortDate', () => {
  it('uses the short weekday and no spaces inside the date', () => {
    expect(formatShortDate('2026-11-14')).toBe('sub, 14.11.');
  });
});

describe('formatTimeRange', () => {
  it('drops the seconds and joins with an en dash', () => {
    expect(formatTimeRange('17:00:00', '19:00:00')).toBe('17:00–19:00');
  });
});

describe('pluralSr', () => {
  const forms = { one: 'slobodan termin', few: 'slobodna termina', other: 'slobodnih termina' };

  it.each([
    [1, '1 slobodan termin'],
    [2, '2 slobodna termina'],
    [5, '5 slobodnih termina'],
    [11, '11 slobodnih termina'],
    [21, '21 slobodan termin'],
    [22, '22 slobodna termina'],
  ])('%i → %s', (count, expected) => {
    expect(pluralSr(count, forms)).toBe(expected);
  });
});

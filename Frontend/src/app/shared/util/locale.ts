export type AppLocale = 'sr-Latn' | 'en' | 'ru';

/** LOCALE_ID → AppLocale. Anything unexpected (a dev build without localize says 'en-US') is Serbian. */
export function toAppLocale(localeId: string): AppLocale {
  return localeId === 'en' || localeId === 'ru' ? localeId : 'sr-Latn';
}

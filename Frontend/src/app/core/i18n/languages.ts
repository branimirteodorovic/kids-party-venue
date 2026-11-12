import { AppLocale } from '../../shared/util/locale';

export interface Language {
  locale: AppLocale;
  /** URL prefix of this language's build. Serbian has none. */
  prefix: '' | '/en' | '/ru';
  /** Always in the language itself, never translated. */
  name: string;
  /** What the API expects in Accept-Language (used in M3). */
  apiCode: 'sr' | 'en' | 'ru';
}

export const LANGUAGES: readonly Language[] = [
  { locale: 'sr-Latn', prefix: '', name: 'Srpski', apiCode: 'sr' },
  { locale: 'en', prefix: '/en', name: 'English', apiCode: 'en' },
  { locale: 'ru', prefix: '/ru', name: 'Русский', apiCode: 'ru' },
];

import { Component } from '@angular/core';
import { Logo } from '../../../shared/ui/logo/logo';
import { Icon } from '../../../shared/ui/icon/icon';
import { Button, ButtonSize, ButtonVariant } from '../../../shared/ui/button/button';
import { formatLongDate, formatShortDate, pluralSr } from '../../../shared/util/format';
import { DinPipe } from '../../../shared/util/din-pipe';
import { Badge } from '../../../shared/ui/badge/badge';
import { INQUIRY_STATUS_META, InquiryStatus } from '../../../shared/domain/inquiry-status';
import { InquiryStatusBadge } from '../../../shared/domain/inquiry-status-badge/inquiry-status-badge';

interface Swatch {
  name: string;
  hex: string;
  className: string; // 💬 a COMPLETE class string, so Tailwind finds it (0.2)
}

@Component({
  imports: [Logo, Icon, Button, DinPipe, Badge, InquiryStatusBadge],
  selector: 'app-styleguide',
  styleUrl: './styleguide.css',
  templateUrl: './styleguide.html',
})
export class Styleguide {
  protected readonly swatches: Swatch[] = [
    { name: 'page', hex: '#FFFCF8', className: 'bg-page' },
    { name: 'surface', hex: '#FFFFFF', className: 'bg-surface' },
    { name: 'primary-600', hex: '#5B3CC4', className: 'bg-primary-600' },
    // TODO: the rest
  ];

  protected readonly typeStyles = [
    {
      token: 'display',
      size: '32/40 → 44/52',
      className: 'font-heading text-display font-extrabold desktop:text-display-lg',
    },
    {
      token: 'h1',
      size: '28/36 → 34/42',
      className: 'font-heading text-h1 font-extrabold desktop:text-h1-lg',
    },
    {
      token: 'h2',
      size: '24/32 → 26/34',
      className: 'font-heading text-h2 font-extrabold desktop:text-h2-lg',
    },
    { token: 'h3', size: '20/28', className: 'font-heading text-h3 font-bold' },
    { token: 'body-lg', size: '18/28', className: 'text-body-lg' },
    { token: 'body', size: '16/24', className: 'text-body' },
    { token: 'label', size: '16/20', className: 'text-label font-semibold' },
    {
      token: 'price',
      size: '22/28',
      className: 'font-heading text-price font-extrabold tabular-nums',
    },
    { token: 'caption', size: '14/20', className: 'text-caption' },
  ];

  protected readonly buttonVariants: ButtonVariant[] = [
    'primary',
    'secondary',
    'text',
    'danger',
    'danger-outline',
  ];
  protected readonly buttonSizes: ButtonSize[] = ['md', 'lg'];

  /** Must match the names registered in core/icons.ts (Lucide kebab-case). */
  protected readonly iconNames = [
    'archive',
    'badge-check',
    'ban',
    'calendar-days',
    'check',
    'chevron-down',
    'chevron-left',
    'chevron-right',
    'circle-alert',
    'circle-check',
    'circle-x',
    'flag',
    'heart',
    'hourglass',
    'house',
    'inbox',
    'info',
    'loader-circle',
    'map-pin',
    'menu',
    'minus',
    'plus',
    'search',
    'timer-off',
    'undo-2',
    'users',
    'x',
  ];

  protected readonly statuses = Object.keys(INQUIRY_STATUS_META) as InquiryStatus[];

  // 💬 The template can't call imported functions, so each result is stored in a field.
  protected readonly longDate = formatLongDate('2026-11-14'); // "subota, 14. novembar 2026."
  protected readonly shortDate = formatShortDate('2026-11-14'); // "sub, 14.11."

  private readonly slotForms = {
    one: 'slobodan termin',
    few: 'slobodna termina',
    other: 'slobodnih termina',
  };
  // 💬 Serbian has 3 forms: 1 → one, 2 → few, 5 → other
  protected readonly plurals = [1, 2, 5].map((count) => pluralSr(count, this.slotForms));
}

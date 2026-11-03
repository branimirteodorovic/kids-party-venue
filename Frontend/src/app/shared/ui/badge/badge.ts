import { Component, computed, input } from '@angular/core';
import { Icon } from '../icon/icon';

export type BadgeTone =
  | 'info'
  | 'warning'
  | 'success'
  | 'paid'
  | 'completed'
  | 'refunded'
  | 'danger'
  | 'muted'
  | 'accent';

const BASE =
  'inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-caption font-semibold';

const TONE_CLASSES: Record<BadgeTone, string> = {
  info: 'border-info-border bg-info-bg text-info-fg whitespace-nowrap',
  warning: 'border-warning-border bg-warning-bg text-warning-fg whitespace-nowrap',
  success: 'border-success-border bg-success-bg text-success-fg whitespace-nowrap',
  paid: 'border-paid-border bg-paid-bg text-paid-fg whitespace-nowrap',
  completed: 'border-completed-border bg-completed-bg text-completed-fg whitespace-nowrap',
  refunded: 'border-refunded-border bg-refunded-bg text-refunded-fg whitespace-nowrap',
  danger: 'border-danger-border bg-danger-bg text-danger-fg whitespace-nowrap',
  muted: 'border-dashed border-border-input bg-surface text-text-muted whitespace-nowrap',
  accent: 'border-accent-200 bg-accent-50 text-accent-800 whitespace-nowrap',
};

@Component({
  selector: 'app-badge',
  imports: [Icon],
  template: `
    @if (icon()) {
      <app-icon [name]="icon()!" [size]="16" />
    }
    <ng-content />
  `,
  host: { '[class]': 'classes()' },
})
export class Badge {
  readonly tone = input<BadgeTone>('info');
  readonly icon = input<string>();
  protected readonly classes = computed(() => `${BASE} ${TONE_CLASSES[this.tone()]}`);
}

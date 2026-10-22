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
  info: 'border-info-border bg-info-bg text-info-fg',
  warning: 'border-warning-border bg-warning-bg text-warning-fg',
  success: 'border-success-border bg-success-bg text-success-fg',
  paid: 'border-paid-border bg-paid-bg text-paid-fg',
  completed: 'border-completed-border bg-completed-bg text-completed-fg',
  refunded: 'border-refunded-border bg-refunded-bg text-refunded-fg',
  danger: 'border-danger-border bg-danger-bg text-danger-fg',
  muted: 'border-dashed border-border-input bg-surface text-text-muted',
  accent: 'border-accent-200 bg-accent-50 text-accent-800',
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

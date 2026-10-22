import { booleanAttribute, Component, computed, input } from '@angular/core';
import { Icon } from '../icon/icon';

export type ButtonVariant = 'primary' | 'secondary' | 'text' | 'danger' | 'danger-outline';
export type ButtonSize = 'md' | 'lg';

const BASE =
  'inline-flex items-center justify-center gap-2 rounded-md font-semibold transition-colors ' +
  'disabled:cursor-not-allowed disabled:border-transparent disabled:bg-surface-sunken disabled:text-disabled-fg';

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary: 'bg-primary-600 text-white hover:bg-primary-700 active:bg-primary-800',
  secondary:
    'border-2 border-primary-600 bg-surface text-primary-700 hover:bg-primary-50 active:bg-primary-100',
  text: 'text-primary-700 underline-offset-4 hover:underline',
  danger: 'bg-danger-fg text-white hover:bg-danger-hover',
  'danger-outline': 'border-2 border-danger-border bg-surface text-danger-fg hover:bg-danger-bg',
};

const SIZE_CLASSES: Record<ButtonSize, string> = {
  md: 'min-h-12 px-5 text-label',
  lg: 'min-h-14 px-6 text-button',
};

/** Exported so links that look like buttons can reuse it. */
export function buttonClasses(
  variant: ButtonVariant,
  size: ButtonSize,
  fullWidth: boolean,
): string {
  return `${BASE} ${VARIANT_CLASSES[variant]} ${SIZE_CLASSES[size]} ${fullWidth ? 'w-full' : ''}`;
}

@Component({
  selector: 'button[appButton]',
  imports: [Icon],
  template: `
    @if (loading()) {
      <app-icon name="loader-circle" [size]="20" class="animate-spin" />
    }
    <ng-content />
  `,
  host: {
    '[class]': 'classes()',
    '[disabled]': 'disabled() || loading()',
    '[attr.aria-busy]': 'loading() || null',
  },
})
export class Button {
  readonly variant = input<ButtonVariant>('primary');

  readonly size = input<ButtonSize>('md');

  readonly disabled = input(false, { transform: booleanAttribute });
  readonly loading = input(false, { transform: booleanAttribute });
  readonly fullWidth = input(false, { transform: booleanAttribute });

  protected readonly classes = computed(() =>
    buttonClasses(this.variant(), this.size(), this.fullWidth()),
  );
}

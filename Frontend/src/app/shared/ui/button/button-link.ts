import { computed, Directive, input } from '@angular/core';
import { buttonClasses, ButtonSize, ButtonVariant } from './button';

@Directive({
  selector: 'a[appButtonLink]',
  host: {
    '[class]': 'classes()',
  },
})
export class ButtonLink {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');

  protected readonly classes = computed(() => buttonClasses(this.variant(), this.size(), false));
}

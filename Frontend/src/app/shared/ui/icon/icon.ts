import { Component, input } from '@angular/core';
import { LucideDynamicIcon } from '@lucide/angular';

@Component({
  selector: 'app-icon',
  imports: [LucideDynamicIcon],
  template: `
    <svg
      [lucideIcon]="name()"
      [size]="size()"
      [attr.aria-hidden]="label() ? null : 'true'"
      [attr.aria-label]="label() ?? null"
      [attr.role]="label() ? 'img' : null"
    ></svg>
  `,
  host: { class: 'inline-flex shrink-0' },
})
export class Icon {
  /** A registered Lucide icon name, e.g. 'calendar-days'. */
  readonly name = input.required<string>();
  readonly size = input(24);
  /** Only for an icon that stands alone and means something. Icons next to text get no label. */
  readonly label = input<string>();
}

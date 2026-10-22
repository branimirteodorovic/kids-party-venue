import { Component, computed, input } from '@angular/core';
import { Badge } from '../../ui/badge/badge';
import { INQUIRY_STATUS_META, InquiryStatus } from '../inquiry-status';

@Component({
  selector: 'app-inquiry-status-badge',
  imports: [Badge],
  template: `<app-badge [tone]="meta().tone" [icon]="meta().icon">{{ label() }}</app-badge>`,
})
export class InquiryStatusBadge {
  readonly status = input.required<InquiryStatus>();
  readonly audience = input<'provider' | 'parent'>('provider');

  protected readonly meta = computed(() => INQUIRY_STATUS_META[this.status()]);
  protected readonly label = computed(() =>
    this.audience() === 'parent' ? this.meta().parentLabel : this.meta().label,
  );
}

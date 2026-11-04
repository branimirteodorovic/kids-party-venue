import { Component, input } from '@angular/core';
import { Icon } from '../icon/icon';

export type HeadingLevel = 'h2' | 'h3';

@Component({
  imports: [Icon],
  selector: 'app-empty-state',
  templateUrl: './empty-state.html',
})
export class EmptyState {
  readonly heading = input.required<string>();
  readonly message = input<string>();
  readonly icon = input<string>();
  readonly headingLevel = input<HeadingLevel>('h3');
}

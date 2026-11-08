import { Component, input } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-tracking-page',
  styleUrl: './tracking-page.css',
  templateUrl: './tracking-page.html',
})
export class TrackingPage {
  readonly token = input<string>();
}

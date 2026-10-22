import { Component } from '@angular/core';

@Component({
  selector: 'app-logo',
  template: `
    <svg viewBox="0 0 32 40" class="h-9 w-auto" aria-hidden="true">
      <path
        class="text-primary-600"
        fill="currentColor"
        d="M16 2C9.4 2 4 7.4 4 14c0 7.6 6 13.6 12 15.4C22 27.6 28 21.6 28 14 28 7.4 22.6 2 16 2Z"
      />
      <ellipse cx="11.5" cy="10.5" rx="2.4" ry="4" fill="#fff" opacity=".35" transform="rotate(-20 11.5 10.5)" />
      <path class="text-primary-700" fill="currentColor" d="M14 29h4l-1 2.6h-2z" />
      <path
        class="text-accent-500"
        d="M16 31.6c-2 2.4 2 3.6 0 6.4"
        fill="none"
        stroke="currentColor"
        stroke-width="1.6"
        stroke-linecap="round"
      />
    </svg>
    <span class="font-heading text-h3 font-extrabold tracking-tight text-text">
      Slavljenik<span class="text-primary-600">.rs</span>
    </span>
  `,
  host: { class: 'inline-flex items-center gap-2' },
})
export class Logo {}

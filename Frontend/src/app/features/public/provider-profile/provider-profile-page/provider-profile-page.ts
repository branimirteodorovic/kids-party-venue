import { Component, input } from '@angular/core';
import { ProviderCategory } from '../../../../shared/domain/provider-category';

@Component({
  imports: [],
  selector: 'app-provider-profile-page',
  styleUrl: './provider-profile-page.css',
  templateUrl: './provider-profile-page.html',
})
export class ProviderProfilePage {
  // From the route's data (categoryRoutes)
  readonly category = input.required<ProviderCategory>();
  // From the path: /rodjendaonice/:city (undefined on /rodjendaonice)
  readonly city = input<string>();
  // From the path: /rodjendaonice/:city/:slug (undefined everywhere else)
  readonly slug = input<string>();
  // From the query string ?termin=9f2c1a7e-4b3d-4e8a-9c51-0d6f2a8b7e34
  // eslint-disable-next-line @angular-eslint/no-input-rename
  readonly slotId = input<string | undefined>(undefined, { alias: 'termin' });
}

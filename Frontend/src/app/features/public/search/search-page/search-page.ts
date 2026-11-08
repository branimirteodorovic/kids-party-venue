import { Component, input } from '@angular/core';
import { ProviderCategory } from '../../../../shared/domain/provider-category';
import { toOptionalInt } from '../../../../shared/util/parse';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  selector: 'app-search-page',
  templateUrl: './search-page.html',
  styleUrl: './search-page.css',
})
export class SearchPage {
  // From the route's data (categoryRoutes)
  readonly category = input.required<ProviderCategory>();
  // From the path: /rodjendaonice/:city (undefined on /rodjendaonice)
  readonly city = input<string>();
  // From the path: /rodjendaonice/:city/kvart/:district (undefined everywhere else)
  readonly district = input<string>();
  // From the query string ?datum=2026-11-14
  // eslint-disable-next-line @angular-eslint/no-input-rename
  readonly date = input<string | undefined>(undefined, { alias: 'datum' });
  // PartOfDay from ?vreme= (a string for now)
  // eslint-disable-next-line @angular-eslint/no-input-rename
  readonly partOfDay = input<string | undefined>(undefined, { alias: 'vreme' });
  // Kids from ?deca=, as a number:
  readonly kids = input<number | undefined, string | undefined>(undefined, {
    // eslint-disable-next-line @angular-eslint/no-input-rename
    alias: 'deca',
    transform: toOptionalInt,
  });
  // Adults from ?odrasli=, the same way
  readonly adults = input<number | undefined, string | undefined>(undefined, {
    // eslint-disable-next-line @angular-eslint/no-input-rename
    alias: 'odrasli',
    transform: toOptionalInt,
  });
}

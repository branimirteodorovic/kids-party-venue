import { Component, computed, input, output } from '@angular/core';
import { Icon } from '../../../../shared/ui/icon/icon';
import { Badge } from '../../../../shared/ui/badge/badge';
import { formatShortDate, formatTimeRange } from '../../../../shared/util/format';
import { DinPipe } from '../../../../shared/util/din-pipe';

export interface ProviderCardData {
  slug: string;
  name: string;
  district: string;
  photoUrl: string;
  fromPrice: number; // per child
  kidsCapacity: number;
  nextFreeSlots: { date: string; start: string; end: string }[]; // "2026-11-14", "17:00:00", "19:00:00"
  isSponsored: boolean;
}

@Component({
  imports: [Icon, Badge, DinPipe],
  selector: 'app-provider-card',
  styleUrl: './provider-card.css',
  templateUrl: './provider-card.html',
})
export class ProviderCard {
  readonly data = input.required<ProviderCardData>();
  readonly isFavorite = input<boolean>(false);
  readonly favoriteToggle = output<string>();

  protected onFavoriteToggle() {
    this.favoriteToggle.emit(this.data().slug);
  }

  protected readonly freeSlots = computed<string[]>(() => {
    const result = this.data()
      .nextFreeSlots.slice(0, 3)
      .map((s) => this.getFormattedTimeRange(s));
    if (this.data().nextFreeSlots.length > 3) {
      return [...result, `+ još ${this.data().nextFreeSlots.length - 3}`];
    }

    return result;
  });

  protected getFormattedTimeRange(data: { date: string; start: string; end: string }) {
    return `${formatShortDate(data.date)} · ${formatTimeRange(data.start, data.end)}`;
  }
}

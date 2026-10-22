import { Pipe, PipeTransform } from '@angular/core';
import { formatDin } from './format';

@Pipe({ name: 'din' })
export class DinPipe implements PipeTransform {
  transform(amount: number | null | undefined): string {
    return amount == null ? '' : formatDin(amount); // 💬 == null catches both null and undefined
  }
}

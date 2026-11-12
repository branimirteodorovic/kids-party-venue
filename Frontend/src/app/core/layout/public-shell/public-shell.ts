import { Component, signal } from '@angular/core';
import { Logo } from '../../../shared/ui/logo/logo';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Icon } from '../../../shared/ui/icon/icon';
import { ButtonLink } from '../../../shared/ui/button/button-link';
import { BRAND_NAME } from '../../brand';

@Component({
  imports: [Logo, RouterLink, RouterOutlet, ButtonLink, Icon, RouterLinkActive],
  selector: 'app-public-shell',
  templateUrl: './public-shell.html',
})
export class PublicShell {
  protected readonly menuOpen = signal(false);

  protected readonly links = [
    { path: '/rodjendaonice', label: $localize`:@@route.partyVenues:Rođendaonice` },
    { path: '/moje-proslave', label: $localize`:@@route.myParties:Moje proslave` },
  ];

  protected readonly brandName = BRAND_NAME;
}

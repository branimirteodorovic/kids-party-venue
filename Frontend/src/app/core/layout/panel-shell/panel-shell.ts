import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Logo } from '../../../shared/ui/logo/logo';
import { Icon } from '../../../shared/ui/icon/icon';

@Component({
  imports: [RouterOutlet, Logo, RouterLink, RouterLinkActive, Icon],
  selector: 'app-panel-shell',
  styles: ``,
  templateUrl: './panel-shell.html',
})
export class PanelShell {
  protected readonly navItems = [
    { path: '/panel', label: $localize`:@@route.home:Početna`, icon: 'house', exact: true },
    { path: '/panel/upiti', label: $localize`:@@route.inquiries:Upiti`, icon: 'inbox', exact: false },
    { path: '/panel/kalendar', label: $localize`:@@route.calendar:Kalendar`, icon: 'calendar-days', exact: false },
    { path: '/panel/profil', label: $localize`:@@route.profile:Profil`, icon: 'store', exact: false },
    { path: '/panel/vise', label: $localize`:@@route.more:Više`, icon: 'ellipsis', exact: false },
  ];
}

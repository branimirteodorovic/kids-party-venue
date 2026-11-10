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
    { path: '/panel', label: 'Početna', icon: 'house', exact: true },
    { path: '/panel/upiti', label: 'Upiti', icon: 'inbox', exact: false },
    { path: '/panel/kalendar', label: 'Kalendar', icon: 'calendar-days', exact: false },
    { path: '/panel/profil', label: 'Profil', icon: 'store', exact: false },
    { path: '/panel/vise', label: 'Više', icon: 'ellipsis', exact: false },
  ];
}

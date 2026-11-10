import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Logo } from '../../../shared/ui/logo/logo';

@Component({
  imports: [RouterOutlet, Logo, RouterLink, RouterLinkActive],
  selector: 'app-admin-shell',
  templateUrl: './admin-shell.html',
})
export class AdminShell {
  protected readonly navItems = [
    { path: '/admin', label: 'Pregled', exact: true },
    { path: '/admin/provera-profila', label: 'Provera profila', exact: false },
    { path: '/admin/ponudjaci', label: 'Ponuđači', exact: false },
    { path: '/admin/gradovi', label: 'Gradovi', exact: false },
    { path: '/admin/pretplate', label: 'Pretplate', exact: false },
    { path: '/admin/sifarnici', label: 'Šifarnici', exact: false },
  ];
}

import { Routes } from '@angular/router';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    title: 'Pregled',
    loadComponent: () =>
      import('./../../features/admin/dashboard/admin-dashboard-page/admin-dashboard-page').then(
        (m) => m.AdminDashboardPage,
      ),
  },
];

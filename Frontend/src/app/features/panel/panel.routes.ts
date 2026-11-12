import { Routes } from '@angular/router';

export const PANEL_ROUTES: Routes = [
  {
    path: '',
    title: $localize`:@@panel.dashboardTitle:Početna`,
    loadComponent: () =>
      import('./../../features/panel/dashboard/dashboard-page/dashboard-page').then(
        (m) => m.DashboardPage,
      ),
  },
  {
    path: 'upiti',
    title: $localize`:@@panel.inquiriesTitle:Upiti`,
    loadComponent: () =>
      import('./../../features/panel/inquiries/inquiries-page/inquiries-page').then(
        (m) => m.InquiriesPage,
      ),
  },
  {
    path: 'upiti/:inquiryId',
    title: $localize`:@@panel.inquiryTitle:Upit`,
    loadComponent: () =>
      import('./../../features/panel/inquiries/inquiry-detail-page/inquiry-detail-page').then(
        (m) => m.InquiryDetailPage,
      ),
  },
  {
    path: 'kalendar',
    title: $localize`:@@panel.calendarTitle:Kalendar`,
    loadComponent: () =>
      import('./../../features/panel/calendar/calendar-page/calendar-page').then(
        (m) => m.CalendarPage,
      ),
  },
  {
    path: 'profil',
    title: $localize`:@@panel.profileTitle:Profil`,
    loadComponent: () =>
      import('./../../features/panel/profile/profile-page/profile-page').then((m) => m.ProfilePage),
  },
  {
    path: 'vise',
    title: $localize`:@@panel.moreTitle:Više`,
    loadComponent: () =>
      import('./../../features/panel/more/more-page/more-page').then((m) => m.MorePage),
  },
];

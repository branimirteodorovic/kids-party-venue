import { Route, Routes } from '@angular/router';
import { ProviderCategory } from './shared/domain/provider-category';
import { isDevMode } from '@angular/core';
import { PublicShell } from './core/layout/public-shell/public-shell';

const searchPage = () =>
  import('./features/public/search/search-page/search-page').then((m) => m.SearchPage);
const providerProfilePage = () =>
  import('./features/public/provider-profile/provider-profile-page/provider-profile-page').then(
    (m) => m.ProviderProfilePage,
  );
const inquiryPage = () =>
  import('./features/public/inquiry/inquiry-page/inquiry-page').then((m) => m.InquiryPage);

/** City, district and profile pages for one category, e.g. /rodjendaonice/novi-sad/igraonica-zvezdica */
function categoryRoutes(path: string, category: ProviderCategory, title: string): Route {
  return {
    path,
    data: { category },
    children: [
      { path: '', title, loadComponent: searchPage },
      { path: ':city', title, loadComponent: searchPage },
      { path: ':city/kvart/:district', title, loadComponent: searchPage },
      { path: ':city/:slug', title, loadComponent: providerProfilePage },
      { path: ':city/:slug/upit', title: 'Pošaljite upit', loadComponent: inquiryPage },
    ],
  };
}

const devRoutes: Routes = isDevMode()
  ? [
      {
        path: 'styleguide',
        title: 'Styleguide',
        loadComponent: () =>
          import('./features/dev/styleguide/styleguide').then((m) => m.Styleguide),
      },
    ]
  : [];

export const routes: Routes = [
  {
    path: 'prijava',
    title: 'Prijava',
    loadComponent: () => import('./features/auth/login-page/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'preuzmi-profil',
    title: 'Preuzmite profil',
    loadComponent: () => import('./features/auth/claim-page/claim-page').then((m) => m.ClaimPage),
  },
  ...devRoutes,
  {
    path: '',
    component: PublicShell,
    children: [
      {
        path: '',
        pathMatch: 'full',
        title: 'Dečiji rođendani',
        loadComponent: () =>
          import('./features/public/home/home-page/home-page').then((m) => m.HomePage),
      },
      categoryRoutes('rodjendaonice', 'Venue', 'Rođendaonice'),
      {
        path: 'upit/:token',
        title: 'Vaš upit',
        loadComponent: () =>
          import('./features/public/tracking/tracking-page/tracking-page').then(
            (m) => m.TrackingPage,
          ),
      },
      {
        path: 'moje-proslave',
        title: 'Moje proslave',
        loadComponent: () =>
          import('./features/public/my-parties/my-parties-page/my-parties-page').then(
            (m) => m.MyPartiesPage,
          ),
      },
      {
        path: 'za-ponudjace',
        title: 'Za ponuđače',
        loadComponent: () =>
          import('./features/public/for-providers/for-providers-page/for-providers-page').then(
            (m) => m.ForProvidersPage,
          ),
      },
      {
        path: '**',
        title: 'Stranica nije pronađena',
        loadComponent: () =>
          import('./features/not-found-page/not-found-page').then((m) => m.NotFoundPage),
      },
    ],
  },
];

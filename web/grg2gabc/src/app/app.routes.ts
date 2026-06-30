import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'convert',
    loadChildren: () => import('./features/convert/convert.routes').then(m => m.CONVERT_ROUTES),
  },
  {
    path: 'editor',
    loadChildren: () => import('./features/editor/editor.routes').then(m => m.EDITOR_ROUTES),
  },
  { path: '', redirectTo: 'convert', pathMatch: 'full' },
];

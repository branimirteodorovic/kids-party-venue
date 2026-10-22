import { Component } from '@angular/core';
import { Styleguide } from './features/dev/styleguide/styleguide';

@Component({
  imports: [Styleguide],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {}

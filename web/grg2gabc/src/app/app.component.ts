import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, TranslateModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit {
  constructor(private translate: TranslateService) {}

  ngOnInit(): void {
    const saved = localStorage.getItem('lang');
    const lang = saved ?? 'pl';
    this.translate.use(lang);
  }
}

import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-editor',
  standalone: true,
  imports: [TranslateModule],
  template: `<p>{{ 'editor.comingSoon' | translate }}</p>`,
})
export class EditorComponent {}

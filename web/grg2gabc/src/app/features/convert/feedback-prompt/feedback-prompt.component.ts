import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-feedback-prompt',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './feedback-prompt.component.html',
  styleUrl: './feedback-prompt.component.scss',
})
export class FeedbackPromptComponent {
  @Input() gabcContent = '';
  @Output() dismissed = new EventEmitter<void>();

  private translate = inject(TranslateService);

  showFallback = false;
  copied = false;

  get mailtoHref(): string {
    const subject = encodeURIComponent(this.translate.instant('feedback.emailSubject'));
    const body = encodeURIComponent(this.translate.instant('feedback.emailBody'));
    return `mailto:${environment.contactEmail}?subject=${subject}&body=${body}`;
  }

  get emailTemplate(): string {
    return this.translate.instant('feedback.emailBody');
  }

  openMailto(): void {
    window.location.href = this.mailtoHref;
    // Show fallback after a short delay in case no client opened
    setTimeout(() => { this.showFallback = true; }, 1500);
  }

  async copyTemplate(): Promise<void> {
    await navigator.clipboard.writeText(this.emailTemplate);
    this.copied = true;
    setTimeout(() => { this.copied = false; }, 2000);
  }

  dismiss(): void {
    this.dismissed.emit();
  }
}

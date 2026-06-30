import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ConversionService } from '../../core/conversion.service';
import { UnmappedNeume } from '../../core/gabc/grg2-neume-for-gabc';
import { FeedbackPromptComponent } from './feedback-prompt/feedback-prompt.component';
import { environment } from '../../../environments/environment';

const MAX_FILE_BYTES = 10 * 1024 * 1024;
const FEEDBACK_PROBABILITY = 0.2; // 1-in-5

@Component({
  selector: 'app-convert',
  standalone: true,
  imports: [CommonModule, TranslateModule, FeedbackPromptComponent],
  templateUrl: './convert.component.html',
  styleUrl: './convert.component.scss',
})
export class ConvertComponent {
  private converter = inject(ConversionService);

  gabcViewerUrl = environment.gabcViewerUrl;

  selectedFile: File | null = null;
  gabcOutput: string | null = null;
  warnings: UnmappedNeume[] = [];
  errorKey: string | null = null;
  showFeedback = false;

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.selectedFile = file;
    this.gabcOutput = null;
    this.warnings = [];
    this.errorKey = null;
  }

  convert(): void {
    if (!this.selectedFile) return;

    if (this.selectedFile.size > MAX_FILE_BYTES) {
      this.errorKey = 'convert.errorFileTooLarge';
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const buffer = e.target!.result as ArrayBuffer;
        const result = this.converter.convert(buffer, this.selectedFile!.name);
        this.gabcOutput = result.gabc;
        this.warnings = result.warnings;
        this.errorKey = null;
        this.maybeShowFeedback();
      } catch (err) {
        const msg = err instanceof Error ? err.message : '';
        this.errorKey = msg.startsWith('Invalid file') ? 'convert.errorInvalidFile' : 'convert.errorUnexpected';
        this.gabcOutput = null;
        this.warnings = [];
      }
    };
    reader.readAsArrayBuffer(this.selectedFile);
  }

  download(): void {
    if (!this.gabcOutput || !this.selectedFile) return;
    const baseName = this.selectedFile.name.replace(/\.grg$/i, '');
    const blob = new Blob([this.gabcOutput], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = baseName + '.gabc';
    a.click();
    URL.revokeObjectURL(url);
  }

  private maybeShowFeedback(): void {
    if (Math.random() < FEEDBACK_PROBABILITY) {
      this.showFeedback = true;
    }
  }

  dismissFeedback(): void {
    this.showFeedback = false;
  }
}

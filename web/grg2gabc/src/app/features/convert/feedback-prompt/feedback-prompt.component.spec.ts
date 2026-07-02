import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { FeedbackPromptComponent } from './feedback-prompt.component';
import { environment } from '../../../../environments/environment';

class FakeTranslateService {
  instant(key: string): string {
    return key;
  }
}

describe('FeedbackPromptComponent', () => {
  let component: FeedbackPromptComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [FeedbackPromptComponent],
      providers: [{ provide: TranslateService, useClass: FakeTranslateService }],
    });
    component = TestBed.createComponent(FeedbackPromptComponent).componentInstance;
  });

  it('starts with the fallback hidden and nothing copied', () => {
    expect(component.showFallback).toBe(false);
    expect(component.copied).toBe(false);
  });

  it('emits dismissed when dismiss() is called', () => {
    const spy = jasmine.createSpy('dismissed');
    component.dismissed.subscribe(spy);

    component.dismiss();

    expect(spy).toHaveBeenCalled();
  });

  it('builds a mailto link addressed to the configured contact email', () => {
    expect(component.mailtoHref).toContain(`mailto:${environment.contactEmail}`);
    expect(component.mailtoHref).toContain('subject=feedback.emailSubject');
    expect(component.mailtoHref).toContain('body=feedback.emailBody');
  });

  it('copies the email template to the clipboard and resets after 2 seconds', fakeAsync(() => {
    spyOn(navigator.clipboard, 'writeText').and.resolveTo();

    component.copyTemplate();
    tick();
    expect(navigator.clipboard.writeText).toHaveBeenCalledWith(component.emailTemplate);
    expect(component.copied).toBe(true);

    tick(2000);
    expect(component.copied).toBe(false);
  }));
});

import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { AppComponent } from './app.component';

class FakeTranslateService {
  use = jasmine.createSpy('use');
}

describe('AppComponent', () => {
  let translate: FakeTranslateService;

  beforeEach(() => {
    localStorage.removeItem('lang');
    TestBed.configureTestingModule({
      providers: [{ provide: TranslateService, useClass: FakeTranslateService }],
    });
    translate = TestBed.inject(TranslateService) as unknown as FakeTranslateService;
  });

  afterEach(() => localStorage.removeItem('lang'));

  it('defaults to Polish when no language preference is stored', () => {
    const component = TestBed.createComponent(AppComponent).componentInstance;
    component.ngOnInit();
    expect(translate.use).toHaveBeenCalledWith('pl');
  });

  it('uses the language stored in localStorage when present', () => {
    localStorage.setItem('lang', 'en');
    const component = TestBed.createComponent(AppComponent).componentInstance;
    component.ngOnInit();
    expect(translate.use).toHaveBeenCalledWith('en');
  });
});

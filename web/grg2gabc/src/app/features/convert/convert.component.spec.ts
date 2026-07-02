import { TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { ConvertComponent } from './convert.component';
import { ConversionService } from '../../core/conversion.service';

function selectEvent(file: File): Event {
  return { target: { files: [file] } } as unknown as Event;
}

// Stand-in for the real FileReader: the browser's native async file I/O isn't
// tracked by NgZone, so tests awaiting its completion are flaky. This carries
// the buffer alongside the File and delivers it to onload synchronously.
class FakeFileReader {
  onload: ((ev: { target: { result: ArrayBuffer } }) => void) | null = null;

  readAsArrayBuffer(file: File & { _buffer?: ArrayBuffer }): void {
    this.onload?.({ target: { result: file._buffer ?? new ArrayBuffer(0) } });
  }
}

function fileWithContent(name: string, text = 'hello'): File & { _buffer: ArrayBuffer } {
  const file = new File([text], name) as File & { _buffer: ArrayBuffer };
  file._buffer = new TextEncoder().encode(text).buffer;
  return file;
}

const originalFileReader = window.FileReader;

describe('ConvertComponent', () => {
  let component: ConvertComponent;
  let converterSpy: jasmine.SpyObj<ConversionService>;

  beforeEach(() => {
    converterSpy = jasmine.createSpyObj('ConversionService', ['convert']);
    TestBed.configureTestingModule({
      imports: [TranslateModule.forRoot()],
      providers: [{ provide: ConversionService, useValue: converterSpy }],
    });
    component = TestBed.createComponent(ConvertComponent).componentInstance;
    (window as unknown as { FileReader: unknown }).FileReader = FakeFileReader;
  });

  afterEach(() => {
    (window as unknown as { FileReader: unknown }).FileReader = originalFileReader;
  });

  it('does nothing when convert() is called with no file selected', () => {
    component.convert();
    expect(converterSpy.convert).not.toHaveBeenCalled();
  });

  it('resets prior results when a new file is selected', () => {
    component.gabcOutput = 'stale';
    component.warnings = [{ id: 1, positionX: 0, positionY: 0, caption: '' }];
    component.errorKey = 'convert.errorUnexpected';

    component.onFileSelected(selectEvent(new File(['x'], 'a.grg')));

    expect(component.gabcOutput).toBeNull();
    expect(component.warnings).toEqual([]);
    expect(component.errorKey).toBeNull();
  });

  it('rejects files above the 10 MB limit without reading them', () => {
    const bigFile = new File([new Uint8Array(10 * 1024 * 1024 + 1)], 'big.grg');
    component.onFileSelected(selectEvent(bigFile));

    component.convert();

    expect(component.errorKey).toBe('convert.errorFileTooLarge');
    expect(converterSpy.convert).not.toHaveBeenCalled();
  });

  it('populates gabcOutput and warnings on a successful conversion', () => {
    converterSpy.convert.and.returnValue({ gabc: 'GABC!', warnings: [] });
    component.onFileSelected(selectEvent(fileWithContent('chant.grg')));

    component.convert();

    expect(component.gabcOutput).toBe('GABC!');
    expect(component.errorKey).toBeNull();
  });

  it('reports an invalid-file error when the converter rejects the format', () => {
    converterSpy.convert.and.throwError('Invalid file format: expected GRG2 header, got "XXXX"');
    component.onFileSelected(selectEvent(fileWithContent('chant.grg')));

    component.convert();

    expect(component.errorKey).toBe('convert.errorInvalidFile');
    expect(component.gabcOutput).toBeNull();
  });

  it('reports an unexpected error for any other failure', () => {
    converterSpy.convert.and.throwError('boom');
    component.onFileSelected(selectEvent(fileWithContent('chant.grg')));

    component.convert();

    expect(component.errorKey).toBe('convert.errorUnexpected');
    expect(component.gabcOutput).toBeNull();
  });

  it('does nothing when download() is called with no output', () => {
    const clickSpy = spyOn(HTMLAnchorElement.prototype, 'click');
    component.download();
    expect(clickSpy).not.toHaveBeenCalled();
  });

  it('downloads the result with the source filename\'s extension swapped to .gabc', () => {
    let capturedName = '';
    spyOn(HTMLAnchorElement.prototype, 'click').and.callFake(function (this: HTMLAnchorElement) {
      capturedName = this.download;
    });
    spyOn(URL, 'createObjectURL').and.returnValue('blob:fake-url');
    const revokeSpy = spyOn(URL, 'revokeObjectURL');

    component.selectedFile = new File([''], 'chant.grg');
    component.gabcOutput = 'gabc content';
    component.download();

    expect(capturedName).toBe('chant.gabc');
    expect(revokeSpy).toHaveBeenCalledWith('blob:fake-url');
  });

  it('dismissFeedback hides the feedback prompt', () => {
    component.showFeedback = true;
    component.dismissFeedback();
    expect(component.showFeedback).toBe(false);
  });
});

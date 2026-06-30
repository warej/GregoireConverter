import { Injectable } from '@angular/core';
import { Grg2 } from './grg2/grg2.model';
import { GabcExporter, ConversionResult } from './gabc/gabc-exporter';

@Injectable({ providedIn: 'root' })
export class ConversionService {
  convert(buffer: ArrayBuffer, fileName: string): ConversionResult {
    const baseName = fileName.replace(/\.grg$/i, '');
    const grg = Grg2.parse(buffer);
    return new GabcExporter().convert(grg, baseName);
  }
}

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'filterLaneByWard',
  pure: false
})
export class FilterLaneByWardPipe implements PipeTransform {
  transform(items: any[], pdw: number[]): any {
    if (!items) {
      return items;
    }

    if (!pdw) return undefined;
    if (pdw.length != 3) return undefined;

    return items.filter(x => x.Ward == pdw[2]);
  }
}

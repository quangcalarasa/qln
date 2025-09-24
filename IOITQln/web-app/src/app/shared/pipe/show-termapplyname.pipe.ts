import { Pipe, PipeTransform } from '@angular/core';
import { TermApply } from 'src/app/shared/utils/consts';

@Pipe({
    name: 'showTermApplyName',
    pure: false
})
export class ShowTermApplyNamePipe implements PipeTransform {
    transform(key: number): any {
        return TermApply[key as unknown as keyof typeof TermApply];
    }

}

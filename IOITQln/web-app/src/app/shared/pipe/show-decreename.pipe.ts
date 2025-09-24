import { Pipe, PipeTransform } from '@angular/core';
import { Decree } from 'src/app/shared/utils/consts';

@Pipe({
    name: 'showDecreeName',
    pure: false
})
export class ShowDecreeNamePipe implements PipeTransform {
    transform(key: number): any {
        return Decree[key as unknown as keyof typeof Decree];
    }

}

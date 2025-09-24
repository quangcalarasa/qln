import { Pipe, PipeTransform } from '@angular/core';
import { Decree } from 'src/app/shared/utils/consts';

@Pipe({
    name: 'showNameItem',
    pure: false
})
export class ShowNameItemPipe implements PipeTransform {
    transform(id: number, items: any[], propertyName: string): any {
        if (!items) {
            return "";
        }

        let item = items.find(x => x.Id == id);
        return item ? item[propertyName] : "";
    }
}

import { Pipe, PipeTransform } from '@angular/core';
import { ModuleSystem } from 'src/app/shared/utils/consts';

@Pipe({
    name: 'showModuleSystem',
    pure: false
})
export class ShowModuleSystemPipe implements PipeTransform {
    transform(key: number): any {
        return ModuleSystem[key as unknown as keyof typeof ModuleSystem];
    }

}

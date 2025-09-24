import { Pipe, PipeTransform } from '@angular/core';
import { LevelBlock } from 'src/app/shared/utils/consts';

@Pipe({
    name: 'showLevelBlock',
    pure: false
})
export class ShowLevelBlockNamePipe implements PipeTransform {
    transform(key: number): any {
        return LevelBlock[key as unknown as keyof typeof LevelBlock];
    }

}

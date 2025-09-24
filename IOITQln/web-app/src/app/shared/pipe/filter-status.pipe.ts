import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
	name: 'filterCtMainTexture',
	pure: false
})
export class FilterCtMainTexturePipe implements PipeTransform {

	transform(items: any[], type_maintexture: number, levelblock: number): any {
		if (!items) {
			return items;
		}

		return items.filter(x => x.TypeMainTexTure == type_maintexture && x.LevelBlock == levelblock);
	}

}

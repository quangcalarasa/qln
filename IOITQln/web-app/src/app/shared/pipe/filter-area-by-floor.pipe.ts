import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
	name: 'filterAreaByFloor',
	pure: false
})
export class FilterAreaByFloorPipe implements PipeTransform {

	transform(items: any[], floorId: number): any {
		if (!items) {
			return items;
		}

		return items.filter(x => x.FloorId == floorId);
	}

}

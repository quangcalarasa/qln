import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
	name: 'filterConstByList',
	pure: false
})
export class FilterConstByListPipe implements PipeTransform {

	transform(items: any, list: any): any {
		if (!items) {
			return items;
		}

		return items.filter((x: any) => {
			return list.find((y: any) => y.key == x.key || y.LevelId?.toString() == x.key);
		});
	}

}

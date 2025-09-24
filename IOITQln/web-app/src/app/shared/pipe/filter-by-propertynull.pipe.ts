import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
	name: 'filterByPropertyNull',
	pure: false
})
export class FilterByPropertyNull implements PipeTransform {

	transform(items: any[], property: string): any {
		if (!items) {
			return items;
		}

		return items.filter(x => x[property] != undefined);
	}

}

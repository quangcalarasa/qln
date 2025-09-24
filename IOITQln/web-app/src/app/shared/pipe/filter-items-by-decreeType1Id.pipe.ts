import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
	name: 'filterItemsByDecreeType1Id',
	pure: false
})
export class FilterItemsByDecreeType1IdPipe implements PipeTransform {

	transform(items: any[], decreeType1Id: number): any {
		if (!items) {
			return items;
		}

		return items.filter(x => x.DecreeType1Id == decreeType1Id);
	}

}

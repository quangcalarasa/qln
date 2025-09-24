import { Pipe, PipeTransform } from '@angular/core';
import { DecreeEnum } from '../utils/enums';

@Pipe({
	name: 'filterDecreeWithoutAnyItem',
	pure: false
})
export class FilterDecreeWithoutAnyItemPipe implements PipeTransform {
	transform(items: any[], decreeEnum: DecreeEnum): any {
		if (!items) {
			return items;
		}

		return items.filter(x => x.key != decreeEnum.toString() && x.key != DecreeEnum.SPECIAL.toString());
	}

}

import { Pipe, PipeTransform } from '@angular/core';
import { TypeReportApplyEnum } from '../utils/enums';

@Pipe({
	name: 'filterTypeReportApplyWithoutAnyItem',
	pure: false
})
export class FilterTypeReportApplyWithoutAnyItemPipe implements PipeTransform {
	transform(items: any[], typeReportApplyEnum?: TypeReportApplyEnum): any {
		if (!items) {
			return items;
		}

		if (!typeReportApplyEnum)
			return items.filter(x => x.key != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG.toString() && x.key != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE.toString());

		return items.filter(x => x.key != typeReportApplyEnum?.toString());
	}
}

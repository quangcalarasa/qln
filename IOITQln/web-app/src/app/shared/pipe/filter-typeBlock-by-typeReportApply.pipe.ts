import { Pipe, PipeTransform } from '@angular/core';
import { TypeReportApplyEnum } from '../utils/enums';

@Pipe({
	name: 'filterTypeBlockByTypeReportApply',
	pure: false
})
export class FilterTypeBlockByTypeReportApplyPipe implements PipeTransform {

	transform(items: any[], typeReportApply: number): any {
		if (!items || !typeReportApply) {
			return items;
		}

		if (typeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG || typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE || typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU)
			return items.filter(x => x.typeBlockMaps.find((x: any) => x.TypeReportApply == typeReportApply));
		else if (typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
			return items.filter(x => x.typeBlockMaps.find((x: any) => x.TypeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG || x.TypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU));
		else if (typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE)
			return items.filter(x => x.typeBlockMaps.find((x: any) => x.TypeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG || x.TypeReportApply == TypeReportApplyEnum.NHA_RIENG_LE));
	}

}

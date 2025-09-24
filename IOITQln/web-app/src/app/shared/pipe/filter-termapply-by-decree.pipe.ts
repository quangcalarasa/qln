import { Pipe, PipeTransform } from '@angular/core';
import { DecreeEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';

@Pipe({
	name: 'filterTermApplyByDecree',
	pure: false
})
export class FilterTermApplyByDecreePipe implements PipeTransform {

	transform(items: any[], decreeId?: number, typeReportApply?: number): any {
		if (!items) {
			return items;
		}

		if (typeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG || typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU || typeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
			if (decreeId == DecreeEnum.ND_99) {
				return items.filter((x: any) => parseInt(x.key) == 65 || parseInt(x.key) == 70);
			}
			else if (decreeId == DecreeEnum.ND_34) {
				return items.filter((x: any) => parseInt(x.key) == 134 || parseInt(x.key) == 234);
			}
			else if (decreeId == DecreeEnum.ND_61) {
				return items.filter((x: any) => parseInt(x.key) == 61);
			}
			else return undefined;
		}
		else {
			if (decreeId == DecreeEnum.ND_99) {
				return items.filter((x: any) => parseInt(x.key) == 71);
			}
			else if (decreeId == DecreeEnum.ND_34) {
				return items.filter((x: any) => parseInt(x.key) == 35);
			}
			else return undefined;
		}
	}

}

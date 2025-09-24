import { Pipe, PipeTransform } from '@angular/core';
import { TypeReportApplyEnum, DecreeEnum } from '../utils/enums';

@Pipe({
	name: 'filterApartmentDetailDataPricing',
	pure: false
})
export class FilterApartmentDetailDataPricingPipe implements PipeTransform {

	transform(items: any[], typeReportApply: TypeReportApplyEnum, applyInvestmentRate: boolean, parentTypeReportApply?: TypeReportApplyEnum): any {
		if (!items) {
			return items;
		}

		if (typeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU || (typeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && parentTypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU)) {
			if (applyInvestmentRate)
				return items.filter(x => x.ApplyInvestmentRate && x.DecreeType1Id == DecreeEnum.ND_99 && !x.IsMezzanine);
			else
				return items.filter(x => !x.ApplyInvestmentRate || x.DecreeType1Id != DecreeEnum.ND_99 || x.IsMezzanine);
		}
		else {
			return items;
		}
	}

}

import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { DecreeEnum, TypeCaseApply_34Enum, TermApplyEnum, TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { DeductionLandMoneyRepository } from 'src/app/infrastructure/repositories/deduction-land-money.repository';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { LandscapeLimitRepository } from 'src/app/infrastructure/repositories/landscape-limit.repository';

@Component({
    selector: 'app-pricing-land-price-item',
    templateUrl: './land-price-item.component.html'
})

export class PricingLandPriceItemComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() block: any;
    @Input() apartment: any;
    @Input() pricingApartmentLandDetails: any[] = [];
    @Input() landPricingTbl: any[] = [];

    deduction_land_money_data: any[] = [];
    TermApplyEnum = TermApplyEnum;
    TypeReportApplyEnum = TypeReportApplyEnum;
    DecreeEnum = DecreeEnum;

    constructor(
        private deductionLandMoneyRepository: DeductionLandMoneyRepository,
        private landscapeLimitRepository: LandscapeLimitRepository
    ) { }

    async ngOnInit() {
        this.getDeductionLandMoneyData();

        if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG && this.validateForm.value.TypeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE) {
            this.pricingApartmentLandDetails = [...this.validateForm.value.pricingApartmentLandDetails];
            if (this.pricingApartmentLandDetails.length == 0) {
                this.pricingApartmentLandDetails = this.apartment.apartmentLandDetails.map((item: any, index: number) => {
                    delete item["Id"];
                    item.ConversionArea = this.calcConversionArea(item.DecreeType1Id, item.TermApply);
                    if (item.DecreeType1Id == DecreeEnum.ND_99)
                        item.LandUnitPrice = this.block.LandScapePrice_99;
                    else if (item.DecreeType1Id == DecreeEnum.ND_34)
                        item.LandUnitPrice = this.block.CaseApply_34 = TypeCaseApply_34Enum.KHOAN_2 ? this.block.LandScapePrice_34 : this.block.AlleyLandScapePrice_34;
                    else
                        item.LandUnitPrice = this.block.LandScapePrice_61;

                    item.LandPrice = this.calcLandPrice(item);

                    return item;
                });

                this.calcTotalLandPrice();
            }
        }
        else if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_RIENG_LE && this.validateForm.value.TypeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE) {
            this.pricingApartmentLandDetails = [...this.validateForm.value.pricingApartmentLandDetails];
            if (this.pricingApartmentLandDetails.length == 0) {
                let sumLimitArea = 0;
                let sumLimitAreaStr = "";

                //Call api lấy dữ liệu hạn mức đất ở trước
                let landscapeLimitItems: any[] = [];
                await Promise.all(
                    this.block.apartmentLandDetails.map(async (item: any, index: number) => {
                        const resp = await this.landscapeLimitRepository.getItemByDecreeTypeReportApplyDistrict(item.DecreeType1Id, this.block.TypeReportApply, this.block.District);
                        if (resp.data != undefined) {
                            let landscapeLimitItem = resp.data;
                            landscapeLimitItem.DecreeType1Id = item.DecreeType1Id;
                            landscapeLimitItem.TypeReportApply = this.block.TypeReportApply;
                            landscapeLimitItems.push(landscapeLimitItem);
                        }
                    })
                );

                this.block.apartmentLandDetails.map(async (item: any, index: number) => {
                    delete item["Id"];
                    if (item.DecreeType1Id == DecreeEnum.ND_99)
                        item.LandUnitPrice = this.block.LandScapePrice_99;
                    else if (item.DecreeType1Id == DecreeEnum.ND_34)
                        item.LandUnitPrice = this.block.CaseApply_34 = TypeCaseApply_34Enum.KHOAN_2 ? this.block.LandScapePrice_34 : this.block.AlleyLandScapePrice_34;
                    else
                        item.LandUnitPrice = this.block.LandScapePrice_61;


                    //Tính InLimitArea, InLimitPercent, OutLimitArea, OutLimitPercent, SumLimitArea
                    sumLimitArea += item.PrivateArea ?? 0;
                    sumLimitAreaStr = sumLimitAreaStr == "" ? item.PrivateArea + "" : sumLimitAreaStr + " + " + item.PrivateArea;
                    item.SumLimitAreaStr = sumLimitAreaStr;
                    item.SumLimitArea = sumLimitArea;

                    let landscapeLimitItem = landscapeLimitItems.find(x => x.DecreeType1Id == item.DecreeType1Id && x.TypeReportApply == this.block.TypeReportApply && x.DistrictId == this.block.District)

                    if (landscapeLimitItem != undefined) {
                        item.InLimitPercent = landscapeLimitItem.InLimitPercent;       //Cần tính
                        item.OutLimitPercent = landscapeLimitItem.OutLimitPercent;     //Cần tính
                        item.LandscapeAreaLimit = landscapeLimitItem.LimitAreaNormal;    //Cần tính

                        if (sumLimitArea <= item.LandscapeAreaLimit) {
                            item.InLimitArea = item.PrivateArea;
                            item.OutLimitArea = 0;
                        }
                        else {
                            let area = sumLimitArea - item.PrivateArea;
                            if (area >= item.LandscapeAreaLimit) {
                                item.InLimitArea = 0;
                                item.OutLimitArea = item.PrivateArea;
                            }
                            else {
                                item.InLimitArea = item.LandscapeAreaLimit - area;
                                item.OutLimitArea = item.PrivateArea - item.InLimitArea;
                            }
                        }

                        item.LandPrice = this.calcLandPriceNrl(item);

                    }
                    else {
                        item.InLimitPercent = 100;       //Cần tính
                        item.OutLimitPercent = 100;     //Cần tính
                        item.LandscapeAreaLimit = undefined;    //Cần tính

                        item.InLimitArea = 0;
                        item.OutLimitArea = item.PrivateArea;

                        item.LandPrice = this.calcLandPriceNrl(item);

                    }

                    this.pricingApartmentLandDetails.push(item);
                    this.calcTotalLandPrice();

                });
            }
        }
        else if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU && this.validateForm.value.TypeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE) {
            let flatCoefficient_99 = this.validateForm.value.FlatCoefficient_99;
            let flatCoefficient_34 = this.validateForm.value.FlatCoefficient_34;
            let flatCoefficient_61 = this.validateForm.value.FlatCoefficient_61;

            this.pricingApartmentLandDetails = [...this.validateForm.value.pricingApartmentLandDetails];
            if (this.pricingApartmentLandDetails.length == 0) {
                this.pricingApartmentLandDetails = this.apartment.apartmentLandDetails.map((item: any, index: number) => {
                    delete item["Id"];
                    if (item.DecreeType1Id == DecreeEnum.ND_99)
                        item.LandUnitPrice = this.block.LandScapePrice_99;
                    else if (item.DecreeType1Id == DecreeEnum.ND_34)
                        item.LandUnitPrice = this.block.CaseApply_34 = TypeCaseApply_34Enum.KHOAN_2 ? this.block.LandScapePrice_34 : this.block.AlleyLandScapePrice_34;
                    else
                        item.LandUnitPrice = this.block.LandScapePrice_61;

                    item.LandPrice = this.calcLandPriceNcc(item, flatCoefficient_99, flatCoefficient_34, flatCoefficient_61);

                    return item;
                });

                this.calcTotalLandPrice();
            }
        }
        else if (this.block.TypeReportApply == TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && this.validateForm.value.TypeReportApply != TypeReportApplyEnum.BAN_PHAN_DIEN_TICH_LIEN_KE) {
            this.pricingApartmentLandDetails = [...this.validateForm.value.pricingApartmentLandDetails];
            if (this.pricingApartmentLandDetails.length == 0) {
                this.pricingApartmentLandDetails = this.apartment.apartmentLandDetails.map((item: any, index: number) => {
                    delete item["Id"];
                    if (item.DecreeType1Id == DecreeEnum.ND_99)
                        item.LandUnitPrice = this.block.LandScapePrice_99;
                    else if (item.DecreeType1Id == DecreeEnum.ND_34)
                        item.LandUnitPrice = this.block.CaseApply_34 = TypeCaseApply_34Enum.KHOAN_2 ? this.block.LandScapePrice_34 : this.block.AlleyLandScapePrice_34;
                    else
                        item.LandUnitPrice = this.block.LandScapePrice_61;

                    item.LandPrice = this.calcLandPriceNhcBbl4(item);

                    return item;
                });

                this.calcTotalLandPrice();
            }
        }
    }

    //Hàm khởi tạo giá trị nhà riêng lẻ, có gọi tới api lấy trị nên cần theo thứ tư
    async initData(item: any) {

    }

    //Tính Giá đất Nhà hộ chung
    calcLandPrice(item: any) {
        let landprice: any;
        let deductionLandMoneyValue = item.DeductionLandMoneyValue ? (100 - item.DeductionLandMoneyValue) / 100 : 1;
        switch (item.TermApply) {
            case TermApplyEnum.DIEU_65:
                landprice = item.LandUnitPrice * ((item.PrivateArea ?? 0) * 0.4 + item.ConversionArea * 0.1 + (item.GeneralArea ?? 0) * 0.4) * deductionLandMoneyValue;
                break;
            case TermApplyEnum.DIEU_70:
                landprice = item.LandUnitPrice * ((item.PrivateArea ?? 0) + item.ConversionArea + (item.GeneralArea ?? 0));
                break;
            case TermApplyEnum.KHOAN_1_DIEU_34:
                landprice = item.LandUnitPrice * ((item.PrivateArea ?? 0) * 0.4 + item.ConversionArea * 0.1 + (item.GeneralArea ?? 0) * 0.4) * deductionLandMoneyValue;
                break;
            case TermApplyEnum.KHOAN_2_DIEU_34:
                landprice = item.LandUnitPrice * ((item.PrivateArea ?? 0) + item.ConversionArea + (item.GeneralArea ?? 0));
                break;
            case TermApplyEnum.DIEU_7:
                landprice = item.LandUnitPrice * ((item.PrivateArea ?? 0) * 0.4 + item.ConversionArea * 0.1 + (item.GeneralArea ?? 0) * 0.4) * deductionLandMoneyValue;
                break;
            default:
                break;
        }

        return Math.round(landprice);
    }

    //Tính diện tích quy đổi Nhà hộ chung
    calcConversionArea(DecreeType1Id: number, TermApply: number) {
        let blockDetails = this.block.blockDetails;
        let landscapeAreaValue1 = this.apartment.LandscapeAreaValue1 ?? 0;
        let landPricingTbl = this.validateForm.value.landPricingTbl;

        let numerator = landPricingTbl.filter((x: any) => x.DecreeType1Id == DecreeType1Id && x.TermApply == TermApply).reduce((total: number, cur: any) => {

            let coefficient = cur.CoefficientDistribution ?? 0;
            return total + cur.GeneralArea * coefficient;
        }, 0);

        let denominator = blockDetails.reduce((total: number, cur: any) => {
            let coefficient = DecreeType1Id == DecreeEnum.ND_99 ? cur.Coefficient_99 : (DecreeType1Id == DecreeEnum.ND_34 ? cur.Coefficient_34 : cur.Coefficient_61)
            return total + cur.GeneralArea * coefficient;
        }, 0);

        let conversionArea = (numerator / denominator) * (landscapeAreaValue1 ?? 0);
        return Math.round(conversionArea * 100) / 100;
    }

    //Tính lại giá đất khi Thay đổi Tiền đất miễn giảm hoặc Hệ số nhà chung cư
    changeDeductionLandMoney(deductionLandMoneyId: number, index: number) {
        if (deductionLandMoneyId) {
            let deductionLandMoney = this.deduction_land_money_data.find((x: any) => x.Id == deductionLandMoneyId);

            this.pricingApartmentLandDetails[index].DeductionLandMoneyValue = deductionLandMoney.Value;
        } else this.pricingApartmentLandDetails[index].DeductionLandMoneyValue = undefined;

        if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
            this.pricingApartmentLandDetails[index].LandPrice = this.calcLandPriceNrl(this.pricingApartmentLandDetails[index])
        }
        else if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG) {
            this.pricingApartmentLandDetails[index].LandPrice = this.calcLandPrice(this.pricingApartmentLandDetails[index])
        }
        else if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) {
            let flatCoefficient_99 = this.validateForm.value.FlatCoefficient_99;
            let flatCoefficient_34 = this.validateForm.value.FlatCoefficient_34;
            let flatCoefficient_61 = this.validateForm.value.FlatCoefficient_61;
            this.pricingApartmentLandDetails[index].LandPrice = this.calcLandPriceNcc(this.pricingApartmentLandDetails[index], flatCoefficient_99, flatCoefficient_34, flatCoefficient_61);
        }
        this.pricingApartmentLandDetails = [...this.pricingApartmentLandDetails];

        this.calcTotalLandPrice();
        // Call emit tới function calcConversionArea
        this.eventEmitter.emit();
        // this.calcConversionArea();
    }

    //Tính tổng số tiền đất
    calcTotalLandPrice() {
        let landPrice = this.pricingApartmentLandDetails.reduce((total: number, curItem) => {
            return total + (curItem.LandPrice ?? 0);
        }, 0);

        this.validateForm.get('LandPrice')?.setValue(landPrice);
        this.validateForm.get('pricingApartmentLandDetails')?.setValue(this.pricingApartmentLandDetails);
    }

    //Lấy ds tiền đất miễn giảm
    async getDeductionLandMoneyData() {
        let paging: GetByPageModel = new GetByPageModel();
        paging.page_size = 0;
        paging.select = 'Id,Condition,Value,DecreeType1Id';

        const resp = await this.deductionLandMoneyRepository.getByPage(paging);

        if (resp.meta?.error_code == 200) {
            this.deduction_land_money_data = resp.data;
        }
    }

    convertLandPricingTbl(DecreeType1Id: number, TermApply: number) {
        let landPricingTbl = this.validateForm.value.landPricingTbl;

        let rs = landPricingTbl.filter((x: any) => x.DecreeType1Id == DecreeType1Id && x.TermApply == TermApply).reduce((res: any, cur: any) => {
            let index_Exist = res.findIndex((x: any) => x.FloorId == cur.FloorId);
            if (index_Exist == -1) {
                let obj = {
                    FloorId: cur.FloorId,
                    CoefficientDistribution: cur.CoefficientDistribution ?? 0,
                    items: [cur]
                };

                res.push(obj);
            } else {
                res[index_Exist].items.push(cur);
            }

            return res;
        }, []);

        return rs;
    }

    convertBlockDetailData(DecreeType1Id: number, TermApply: number) {
        let blockDetails = this.block.blockDetails;

        let rs = blockDetails.reduce((res: any, cur: any) => {
            let index_Exist = res.findIndex((x: any) => x.FloorId == cur.FloorId);
            if (index_Exist == -1) {
                let obj = {
                    FloorId: cur.FloorId,
                    Coefficient: DecreeType1Id == DecreeEnum.ND_99 ? cur.Coefficient_99 : (DecreeType1Id == DecreeEnum.ND_34 ? cur.Coefficient_34 : cur.Coefficient_61),
                    items: [cur]
                };

                res.push(obj);
            } else {
                res[index_Exist].items.push(cur);
            }

            return res;
        }, []);

        return rs;
    }

    //Tính giá đất Nhà riêng lẻ
    calcLandPriceNrl(item: any) {
        let landprice: any;
        let deductionLandMoneyValue = item.DeductionLandMoneyValue ? (100 - item.DeductionLandMoneyValue) / 100 : 1;
        switch (item.TermApply) {
            case TermApplyEnum.DIEU_65:
                landprice = item.LandUnitPrice * (item.InLimitArea * item.InLimitPercent / 100 + item.OutLimitArea * item.OutLimitPercent / 100) * deductionLandMoneyValue;
                break;
            case TermApplyEnum.DIEU_70:
                landprice = item.LandUnitPrice * (item.InLimitArea + item.OutLimitArea);
                break;
            case TermApplyEnum.KHOAN_1_DIEU_34:
                landprice = item.LandUnitPrice * (item.InLimitArea * item.InLimitPercent / 100 + item.OutLimitArea * item.OutLimitPercent / 100) * deductionLandMoneyValue;
                break;
            case TermApplyEnum.KHOAN_2_DIEU_34:
                landprice = item.LandUnitPrice * (item.InLimitArea + item.OutLimitArea);
                break;
            case TermApplyEnum.DIEU_7:
                landprice = item.LandUnitPrice * (item.InLimitArea * item.InLimitPercent / 100 + item.OutLimitArea * item.OutLimitPercent / 100) * deductionLandMoneyValue;
                break;
            default:
                break;
        }

        return Math.round(landprice);
    }

    //Tính giá đất Nhà chung cư
    calcLandPriceNcc(item: any, flatCoefficient_99?: number, flatCoefficient_34?: number, flatCoefficient_61?: number) {
        let landprice: any;
        let deductionLandMoneyValue = item.DeductionLandMoneyValue ? (100 - item.DeductionLandMoneyValue) / 100 : 1;

        switch (item.TermApply) {
            case TermApplyEnum.DIEU_65:
                landprice = item.LandUnitPrice * (item.PrivateArea ?? 0) * (item.CoefficientDistribution ?? 1) * 0.1 * (flatCoefficient_99 ?? 1) * deductionLandMoneyValue;
                break;
            case TermApplyEnum.DIEU_70:
                landprice = item.LandUnitPrice * (item.PrivateArea ?? 0) * (item.CoefficientDistribution ?? 1) * (flatCoefficient_99 ?? 1);
                break;
            case TermApplyEnum.KHOAN_1_DIEU_34:
                landprice = item.LandUnitPrice * (item.PrivateArea ?? 0) * (item.CoefficientDistribution ?? 1) * 0.1 * (flatCoefficient_34 ?? 1) * deductionLandMoneyValue;
                break;
            case TermApplyEnum.KHOAN_2_DIEU_34:
                landprice = item.LandUnitPrice * (item.PrivateArea ?? 0) * (item.CoefficientDistribution ?? 1) * (flatCoefficient_34 ?? 1);
                break;
            case TermApplyEnum.DIEU_7:
                landprice = item.LandUnitPrice * (item.PrivateArea ?? 0) * (item.CoefficientDistribution ?? 1) * 0.1 * (flatCoefficient_61 ?? 1) * deductionLandMoneyValue;
                break;
            default:
                break;
        }

        return Math.round(landprice);
    }

    //
    getStrPrivateArea(idx: number): string {
        let str = this.pricingApartmentLandDetails.splice(0, idx + 1).reduce((str, cur) => {
            return str + " + " + cur.PrivateArea;
        }, "");

        return str;
    }

    //Tính lại giá đất khi Hệ số nhà chung cư thay đổi
    changeFlatCoefficient() {
        let flatCoefficient_99 = this.validateForm.value.FlatCoefficient_99;
        let flatCoefficient_34 = this.validateForm.value.FlatCoefficient_34;
        let flatCoefficient_61 = this.validateForm.value.FlatCoefficient_61;
        this.pricingApartmentLandDetails.forEach(item => {
            if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_RIENG_LE) {
                item.LandPrice = this.calcLandPriceNrl(item)
            }
            else if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_HO_CHUNG) {
                item.LandPrice = this.calcLandPrice(item)
            }
            else if (this.block.TypeReportApply == TypeReportApplyEnum.NHA_CHUNG_CU) {
                item.LandPrice = this.calcLandPriceNcc(item, flatCoefficient_99, flatCoefficient_34, flatCoefficient_61);
            }
        });

        this.pricingApartmentLandDetails = [...this.pricingApartmentLandDetails];

        this.calcTotalLandPrice();
        this.eventEmitter.emit();
    }

    //Tính giá đất Bán phần diện tích sử dụng chung của nhà hộ chung, nhà chung cư
    calcLandPriceNhcBbl4(item: any) {
        return Math.round(item.LandUnitPrice * (item.PrivateArea ?? 0));
    }

    //Tính giá đất Bán phần diện tích đất liền kề
    async calcLandPriceBbl5(pricingApartmentLandDetails: any) {
        if (!pricingApartmentLandDetails) return;

        let sumLimitArea = this.validateForm.value.SellLandArea ?? 0;
        let sumLimitAreaStr = sumLimitArea + "";
        this.pricingApartmentLandDetails = [];

        let landscapeLimitItems: any[] = [];

        await Promise.all(pricingApartmentLandDetails.map(async (item: any, index: number) => {
            const resp = await this.landscapeLimitRepository.getItemByDecreeTypeReportApplyDistrict(item.DecreeType1Id, this.block.TypeReportApply, this.block.District);
            if (resp.data != undefined) {
                let landscapeLimitItem = resp.data;
                landscapeLimitItem.DecreeType1Id = item.DecreeType1Id;
                landscapeLimitItem.TypeReportApply = this.block.TypeReportApply;
                landscapeLimitItems.push(landscapeLimitItem);
            }
        }));

        pricingApartmentLandDetails.map(async (item: any, index: number) => {
            if (item.DecreeType1Id == DecreeEnum.ND_99)
                item.LandUnitPrice = this.block.LandScapePrice_99;
            else if (item.DecreeType1Id == DecreeEnum.ND_34)
                item.LandUnitPrice = this.block.CaseApply_34 = TypeCaseApply_34Enum.KHOAN_2 ? this.block.LandScapePrice_34 : this.block.AlleyLandScapePrice_34;
            else
                item.LandUnitPrice = this.block.LandScapePrice_61;


            //Tính InLimitArea, InLimitPercent, OutLimitArea, OutLimitPercent, SumLimitArea
            sumLimitArea += item.PrivateArea ?? 0;
            sumLimitAreaStr = sumLimitAreaStr == "" ? item.PrivateArea + "" : sumLimitAreaStr + " + " + item.PrivateArea;
            item.SumLimitAreaStr = sumLimitAreaStr;
            item.SumLimitArea = sumLimitArea;

            let landscapeLimitItem = landscapeLimitItems.find(x => x.DecreeType1Id == item.DecreeType1Id && x.TypeReportApply == this.block.TypeReportApply && x.DistrictId == this.block.District)

            if (landscapeLimitItem != undefined) {
                item.InLimitPercent = landscapeLimitItem.InLimitPercent;       //Cần tính
                item.OutLimitPercent = landscapeLimitItem.OutLimitPercent;     //Cần tính
                item.LandscapeAreaLimit = landscapeLimitItem.LimitAreaNormal;    //Cần tính

                if (sumLimitArea <= item.LandscapeAreaLimit) {
                    item.InLimitArea = item.PrivateArea;
                    item.OutLimitArea = 0;
                }
                else {
                    let area = sumLimitArea - item.PrivateArea;
                    if (area >= item.LandscapeAreaLimit) {
                        item.InLimitArea = 0;
                        item.OutLimitArea = item.PrivateArea;
                    }
                    else {
                        item.InLimitArea = item.LandscapeAreaLimit - area;
                        item.OutLimitArea = item.PrivateArea - item.InLimitArea;
                    }
                }

                item.LandPrice = item.LandUnitPrice * (item.InLimitArea * item.InLimitPercent / 100 + item.OutLimitArea * item.OutLimitPercent / 100);

                this.pricingApartmentLandDetails.push(item);
                // this.calcTotalLandPrice();
                // this.calcTotalPrice();
            }
            else {
                item.InLimitPercent = 100;       //Cần tính
                item.OutLimitPercent = 100;     //Cần tính
                item.LandscapeAreaLimit = undefined;    //Cần tính

                item.InLimitArea = 0;
                item.OutLimitArea = item.PrivateArea;

                item.LandPrice = item.LandUnitPrice * (item.InLimitArea * item.InLimitPercent / 100 + item.OutLimitArea * item.OutLimitPercent / 100);
                this.pricingApartmentLandDetails.push(item);
                // this.calcTotalLandPrice();
                // this.calcTotalPrice();
            }
        });

        this.calcTotalLandPrice();
        this.calcTotalPrice();
    }

    calcTotalPrice() {
        let landPrice = this.validateForm.value.LandPrice;
        let apartmentPriceRemaining = this.validateForm.value.ApartmentPriceRemaining;

        this.validateForm.get('TotalPrice')?.setValue(landPrice + apartmentPriceRemaining);
    }
}

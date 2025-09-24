import { Inject, Injectable } from '@angular/core';
import { DecreeEnum } from 'src/app/shared/utils/enums';
import { Decree } from 'src/app/shared/utils/consts';
import { SettingsService } from '@delon/theme';
import { CheckRoleModel } from 'src/app/core/models/check-role-model';
@Injectable({
    providedIn: 'root',
})

export class CommonService {
    constructor(
        private settingService: SettingsService
    ) { }

    convertMoneyToString(input: number) {
        if (!input) return "";

        let money: string = input.toString();

        let dv: string[] = ["", "mươi", "trăm", "nghìn", "triệu", "tỉ"];
        let cs: string[] = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];

        let doc: string;
        let i, j, k, n, len, found, ddv, rd: number;

        let arrListStr = money.split('.');
        let number: string = arrListStr[0];
        len = number.length;
        number += "ss";
        doc = "";
        found = 0;
        ddv = 0;
        rd = 0;

        i = 0;
        while (i < len) {
            //So chu so o hang dang duyet
            n = (len - i + 2) % 3 + 1;

            //Kiem tra so 0
            found = 0;
            for (j = 0; j < n; j++) {
                if (number[i + j] != '0') {
                    found = 1;
                    break;
                }
            }

            //Duyet n chu so
            if (found == 1) {
                rd = 1;
                for (j = 0; j < n; j++) {
                    ddv = 1;
                    switch (number[i + j]) {
                        case '0':
                            if (n - j == 3) doc += cs[0] + " ";
                            if (n - j == 2) {
                                if (number[i + j + 1] != '0') doc += "lẻ ";
                                ddv = 0;
                            }
                            break;
                        case '1':
                            if (n - j == 3) doc += cs[1] + " ";
                            if (n - j == 2) {
                                doc += "mười ";
                                ddv = 0;
                            }
                            if (n - j == 1) {
                                if (i + j == 0) k = 0;
                                else k = i + j - 1;

                                if (number[k] != '1' && number[k] != '0')
                                    doc += "mốt ";
                                else
                                    doc += cs[1] + " ";
                            }
                            break;
                        case '5':
                            if ((i + j == len - 1 || (n != 1 && n - j == 1)) && number[i + j - 1] != '0') {
                                doc += "lăm ";
                            }
                            else
                                doc += cs[5] + " ";
                            break;
                        default:
                            doc += cs[parseInt(number[i + j])] + " ";
                            break;
                    }

                    //Doc don vi nho
                    if (ddv == 1) {
                        doc += dv[n - j - 1] + " ";
                    }
                }
            }


            //Doc don vi lon
            if (len - i - n > 0) {
                if ((len - i - n) % 9 == 0) {
                    if (rd == 1)
                        for (k = 0; k < Math.floor((len - i - n) / 9); k++)
                            doc += "tỉ ";
                    rd = 0;
                }
                else
                    if (found != 0) doc += dv[Math.floor(((len - i - n + 1) % 9) / 3) + 2] + " ";
            }

            i += n;
        }

        if (len == 1)
            if (number[0] == '0' || number[0] == '5') return cs[parseInt(number[0])] + " đồng";

        return doc.replace(/\s\s+/g, ' ') + "đồng";
    }

    checkDecree(decree: DecreeEnum, decreeMaps: any) {
        if (!decreeMaps) return false;

        return decreeMaps?.find((x: any) => x.key == decree || x.DecreeType1Id == decree) ? true : false;
    }

    viewNameDecree(decree: DecreeEnum) {
        return Decree[decree as unknown as keyof typeof Decree];
    }

    //Tính hệ số phân bổ đất
    getCoefficient(floorCode: number, floorApplyPriceChange: number, isMezzanine: boolean, decree: DecreeEnum, distribution_floor_coefficient_data: any[], specialCase: boolean) {
        let coefficient;
        let distribution_floor_coefficient = distribution_floor_coefficient_data.filter(x => x.DecreeType1Id == decree)[0];

        if (!distribution_floor_coefficient) return coefficient;

        let distribution_floor_coefficient_item;
        switch (true) {
            case floorApplyPriceChange == 1 || floorApplyPriceChange == 2:
                distribution_floor_coefficient_item = distribution_floor_coefficient.distributionFloorCoefficientDetails.find((x: any) => x.NumberFloor == 2);
                break;
            case floorApplyPriceChange == 3:
                distribution_floor_coefficient_item = distribution_floor_coefficient.distributionFloorCoefficientDetails.find((x: any) => x.NumberFloor == 3);
                break;
            case floorApplyPriceChange == 4:
                distribution_floor_coefficient_item = distribution_floor_coefficient.distributionFloorCoefficientDetails.find((x: any) => x.NumberFloor == 4);
                break;
            case floorApplyPriceChange >= 5:
                distribution_floor_coefficient_item = distribution_floor_coefficient.distributionFloorCoefficientDetails.find((x: any) => x.NumberFloor == 5);
                break;
            default: break;
        }

        if (!distribution_floor_coefficient_item) return coefficient;

        if (isMezzanine && specialCase) {
            coefficient = distribution_floor_coefficient.MezzanineCoefficient;
        }
        else {
            switch (true) {
                case floorCode == 1:
                    coefficient = distribution_floor_coefficient_item.Value1;
                    break;
                case floorCode == 2:
                    coefficient = distribution_floor_coefficient_item.Value2;
                    break;
                case floorCode == 3:
                    coefficient = distribution_floor_coefficient_item.Value3;
                    break;
                case floorCode == 4:
                    coefficient = distribution_floor_coefficient_item.Value4;
                    break;
                case floorCode == 5:
                    coefficient = distribution_floor_coefficient_item.Value5;
                    break;
                case floorCode >= 6:
                    coefficient = distribution_floor_coefficient_item.Value6;
                    break;
                default: break;
            }
        }

        return coefficient;
    }

    CheckAccessKeyRole(code: string): CheckRoleModel {
        let role = new CheckRoleModel;
        let access_key = this.settingService.user["AccessKey"]
        let arrKey = access_key.split('-').map((key: any) => {
          let objectConvert = key.split(':');
          let obj:any = {};
          obj['key'] = objectConvert[0];
          obj["value"] = objectConvert[1];
          return obj;
        });
    
        let keyIndex = arrKey.find((x: any) => x.key == code);
        if (keyIndex) {
          role.ViewOrActionSpecial = keyIndex.value.substr(0, 1) == '1' ? true : false;
          role.Create = keyIndex.value.substr(1, 1) == '1' ? true : false;
          role.Update = keyIndex.value.substr(2, 1) == '1' ? true : false;
          role.Delete = keyIndex.value.substr(3, 1) == '1' ? true : false;
          role.Import = keyIndex.value.substr(4, 1) == '1' ? true : false;
          role.Export = keyIndex.value.substr(5, 1) == '1' ? true : false;
          role.Other = keyIndex.value.substr(6, 1) == '1' ? true : false;
          role.Menu = keyIndex.value.substr(7, 1) == '1' ? true : false;
        }
    
        return role;
    }
}
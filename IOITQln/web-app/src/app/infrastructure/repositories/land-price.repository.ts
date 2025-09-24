import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { LandPriceType } from 'src/app/shared/utils/enums';

@Injectable({
    providedIn: 'root',
})
export class LandPriceRepository {
    public baseUrl = "/api/LandPrice";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel, txtSearch?: string) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage/${LandPriceType.NOC}${objectToQueryString(params)}&txtSearch=${txtSearch ?? ""}`,
        });
    }

    public addNew(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}`,
            body: param
        });
    }

    public async update(param: any) {
        return this.baseHttpClient.putRequest({
            url: `${this.baseUrl}/${param.Id}`,
            body: param
        });
    }

    public async delete(param: any) {
        return this.baseHttpClient.deleteRequest({
            url: `${this.baseUrl}/${param.Id}`
        });
    }

    public getLandPriceItems(decreeType1Id: number, district: number, txtSg: string) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/getLandPriceItems/${decreeType1Id}/${district}?txtSg=${txtSg}`,
        });
    }

    public getLandPriceItemsMultiDecreeType1Id(district: number, list_decree: any, landPriceType: LandPriceType, txtSg: string) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/getLandPriceItemsMultiDecreeType1Id/${district}/${landPriceType}?txtSg=${txtSg}`,
            body: list_decree
        });
    }

    public async importDataExcel(param: any) {
        return this.baseHttpClient.uploadRequest({
          url: `${this.baseUrl}/ImportDataExcel/${LandPriceType.NOC}`,
          body: param
        });
    }
}

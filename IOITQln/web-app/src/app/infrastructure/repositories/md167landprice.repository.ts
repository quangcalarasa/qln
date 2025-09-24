import { Injectable } from '@angular/core';
import GetByPageLandPriceModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { LandPriceType } from 'src/app/shared/utils/enums';

@Injectable({
    providedIn: 'root',
})
export class Md167LandPriceRepository {
    public baseUrl = "/api/LandPrice";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageLandPriceModel, txtSearch?: string) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage/${LandPriceType.MD167}${objectToQueryString(params)}&txtSearch=${txtSearch ?? ""}`,
        });
    }

    public addNew(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}` + "?filterLandPrice=2",
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

    public getDataNzTree() {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetDataNzTree`,
        });
    }

    public async importDataExcel(param: any) {
        return this.baseHttpClient.uploadRequest({
          url: `${this.baseUrl}/ImportDataExcel/${LandPriceType.MD167}`,
          body: param
        });
    }
}

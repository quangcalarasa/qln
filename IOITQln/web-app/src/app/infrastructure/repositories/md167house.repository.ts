import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
    providedIn: 'root',
})
export class Md167HouseRepository {
    public baseUrl = "/api/House";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}`,
        });
    }
    public getAreaValue(param:any) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetAreaValue?id=${param}`,
        });
    }
    public getAreaValueHouse(param:any) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetAreaValueHouse?districtId=${param}`,
        });
    }
    public getPriceHouse(param1:any,param2:any) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetPriceHouse?districtId=${param1}&laneId=${param2}`,
        });
    }
    public addNew(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}`,
            body: param
        });
    }
    public GetInfoApartment(id: any) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetInfoApartment?id=${id}`
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
          url: `${this.baseUrl}/ImportDataExcel`,
          body: param
        });
    }

    public GetLandPriceData(districtId:any, laneId:any, decree: any) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetLandPriceData?districtId=${districtId}&laneId=${laneId}&decree=${decree}`,
        });
    }
}

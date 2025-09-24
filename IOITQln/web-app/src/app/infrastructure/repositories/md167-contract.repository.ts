import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { ImportHistoryTypeEnum } from 'src/app/shared/utils/enums';

@Injectable({
    providedIn: 'root',
})
export class Md167ContractRepository {
    public baseUrl = "/api/md167contract";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}`,
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

    public GetDataDebt(id: number) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetDataDebt/${id}`,
        });
    }

    public async exportReport(id: number) {
        return this.baseHttpClient.exportFileWord({
            url: `${this.baseUrl}/ExportReport/${id}`,
            body: undefined
        });
    }

    public GetHouseData() {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/getHouseData`,
        });
    }

    public async importDataExcel(param: any, importHistoryType: ImportHistoryTypeEnum) {
        return this.baseHttpClient.uploadRequest({
          url: `${this.baseUrl}/ImportDataExcel/${importHistoryType}`,
          body: param
        });
    }

    public async refundPaidDeposit(param: any) {
        return this.baseHttpClient.putRequest({
            url: `${this.baseUrl}/RefundPaidDeposit/${param.Id}`
        });
    }
}

import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
    providedIn: 'root',
})
export class ReportMd167Repository {
    public baseUrl = "/api/ReportMd167";

    constructor(private baseHttpClient: BaseHttpClient) { }


    public getReport08(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/GetReport08`,
            body: param
        });
    }
    public getReport07(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/GetReport07`,
            body: param
        });
    }
    public ExportExcel08(param: any) {
        return this.baseHttpClient.exportReport("BaoCao08.xlsx", {
            url: `${this.baseUrl}/ExportReport08Md167`,
            body: param,
        });
    }
    public ExportExcel07(param: any) {
        return this.baseHttpClient.exportReport("BaoCao07.xlsx", {
            url: `${this.baseUrl}/ExportReport07Md167`,
            body: param,
        });
    }
    public getReportDebtInfor(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/GetReportDebt`,
            body: param
        });
    }
    public ExportExcelDebtInfor(param: any) {
        return this.baseHttpClient.exportReport("thong_tin_cong_no.xlsx", {
            url: `${this.baseUrl}/ExportReportDebt`,
            body: param,
        });
    }

    public getReportPayment(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/GetReportPayment`,
            body: param
        });
    }
}

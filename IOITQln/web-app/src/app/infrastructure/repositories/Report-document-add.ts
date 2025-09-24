import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { async } from 'rxjs';

@Injectable({
    providedIn: 'root'
  })
  export class ReportTdcRepository {
    public baseUrl = '/api/SyntheticReport';
  
    constructor(private baseHttpClient: BaseHttpClient) {}

    public async ReportTDC2(typeLegal: number) {
      return this.baseHttpClient.getRequest({
        url: `${this.baseUrl}/ReportTDC2/${typeLegal}`
      });
    }

    public ExportReportTDC2(param: any) {
      return this.baseHttpClient.downloadFile({
        url: `${this.baseUrl}/ExportReportTDC2`,
        body: param
      });
    }

    public async ReportTDC3() {
      return this.baseHttpClient.getRequest({
        url: `${this.baseUrl}/ReportTDC3`
      });
    }

    public ExportReportTDC3(param: any) {
      return this.baseHttpClient.downloadFile({
        url: `${this.baseUrl}/ExportReportTDC3`,
        body: param
      });
    }

}
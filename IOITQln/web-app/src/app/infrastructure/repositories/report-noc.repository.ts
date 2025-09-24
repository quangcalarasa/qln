import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { async } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReportNocRepository {
  public baseUrl = '/api/Report';

  constructor(private baseHttpClient: BaseHttpClient) { }

  public async GetSyntheticReportNoc(req: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetSyntheticReportNoc`,
      body: req
    });
  }

  public async GetCustomerNocReport(req: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetCustomerNocReport`,
      body: req
    });
  }

  public async GetStatusBlockNocReport(req: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetStatusBlockNocReport`,
      body: req
    });
  }

  public async GetDataExport(req: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/getDataExport`,
      body: req
    });
  }

  public async GetDataExport2(req: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/getDataExport2`,
      body: req
    });
  }

  public async GetDataExport3(req: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/getDataExport3`,
      body: req
    });
  }
}

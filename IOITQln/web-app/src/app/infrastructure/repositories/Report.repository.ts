import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { async } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ReportRepository {
  public baseUrl = '/api/Identifier';

  constructor(private baseHttpClient: BaseHttpClient) {}

  public async getReport5(dateStart: any, dateEnd: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/ReportNOC5/${dateStart}/${dateEnd}`
    });
  }

  public async ReportNOC4(dateStart: any, dateEnd: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/ReportNOC4/${dateStart}/${dateEnd}`
    });
  }

  public async ReportNOC3(dateStart: any, dateEnd: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/ReportNOC3/${dateStart}/${dateEnd}`
    });
  }

  public async ReportNOC2(typeBlock: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/ReportNOC2/${typeBlock}`
    });
  }

  public async ReportNOC1(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/ExportNOC1`,
      body: param
    });
  }

  public async ReportNOC6(dateStart: any, dateEnd: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/ReportNOC6/${dateStart}/${dateEnd}`
    });
  }

  public async ReportNOC7(dateStart: any, dateEnd: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/ReportNOC7/${dateStart}/${dateEnd}`
    });
  }

  public ExportReportNOC5(param: any, dateStart: any, dateEnd: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_5.xlsx",{
      url: `${this.baseUrl}/ExportReportNOC5/${dateStart}/${dateEnd}`,
      body: param
    });
  }

  public ExportReportNOC4(param: any, dateStart: any, dateEnd: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_4.xlsx",{
      url: `${this.baseUrl}/ExportNOC4/${dateStart}/${dateEnd}`,
      body: param
    });
  }

  public ExportReportNOC3(param: any, dateStart: any, dateEnd: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_3.xlsx",{
      url: `${this.baseUrl}/ExportNOC3/${dateStart}/${dateEnd}`,
      body: param
    });
  }

  public ExportReportNOC6(param: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_6.xlsx",{
      url: `${this.baseUrl}/ExportReportNOC6`,
      body: param
    });
  }

  public ExportReportNOC7(param: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_7.xlsx",{
      url: `${this.baseUrl}/ExportReportNOC7`,
      body: param
    });
  }

  public ExportReportNOC2(param: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_2.xlsx",{
      url: `${this.baseUrl}/ExportReportNOC2`,
      body: param
    });
  }

  public ExportReportNOC1(param: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_1.xlsx",{
      url: `${this.baseUrl}/ExportReportNOC1`,
      body: param
    });
  }
}

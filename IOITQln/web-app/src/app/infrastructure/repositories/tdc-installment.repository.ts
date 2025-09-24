import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
  providedIn: 'root'
})
export class TDCInstallmentPriceRepository {
  public baseUrl = '/api/TDCInstallmentPrice';

  constructor(private baseHttpClient: BaseHttpClient) { }

  public getByPage(params: GetByPageModel) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}`
    });
  }

  public addNew(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}`,
      body: param
    });
  }
  public getLog(Id: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/getLogTdcIPOff/${Id}`
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
  public getWorkSheet(Id: number, param: any, isPay?: boolean, payOff?: boolean, payCount?: number) {
    if (payOff != undefined) {
      if (payCount != undefined)
        return this.baseHttpClient.getRequest({
          url: `${this.baseUrl}/GetWorkSheet/${Id}?dateTime=${param}&isPay=${isPay}&payOff=${payOff}&payCount=${payCount}`
        });
    }
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetWorkSheet/${Id}?dateTime=${param}&isPay=${isPay}`
    });
  }
  public getWorkSheetUpdate(Id: number, param: any, isPay?: boolean, payOff?: boolean, payCount?: number) {
    if (payOff != undefined) {
      if (payCount != undefined)
        return this.baseHttpClient.getRequest({
          url: `${this.baseUrl}/GetWorkSheetUpdate/${Id}?dateTime=${param}&isPay=${isPay}&payOff=${payOff}&payCount=${payCount}`
        });
    }
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetWorkSheet/${Id}?dateTime=${param}&isPay=${isPay}`
    });
  }
  public ExportExcel(Id: number, param: any) {
    return this.baseHttpClient.downloadFile({
      url: `${this.baseUrl}/ExportExcel/${Id}`,
      body: param
    });
  }
  public getById(id: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/${id}`
    });
  }

  public Import(param: any, id: number) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/ImportExcel/${id}`,
      body: param
    });
  }
  public GetReportTable(Id: number, param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetReportTable/${Id}`,
      body: param
    });
  }

  public ExportReport(param: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_1.xlsx",{
      url: `${this.baseUrl}/ExportReport`,
      body: param
    });
  }

  public GetReport(Id: number, param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetReport/${Id}`,
      body: param
    });
  }

  public ExportReport3(param: any) {
    return this.baseHttpClient.exportReport( "BaoCao03.xlsx",{
      url: `${this.baseUrl}/Export`,
      body: param
    });
  }

  public GetReportNo2(Id: number, param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetReportNo2/${Id}`,
      body: param
    });
  }
  public ExportReportNo2(param: any) {
    return this.baseHttpClient.downloadFile({
      url: `${this.baseUrl}/ExportReportNo2`,
      body: param
    });
  }
}

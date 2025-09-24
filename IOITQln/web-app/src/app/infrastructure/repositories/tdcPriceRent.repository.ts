import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { lastValueFrom, Observable } from 'rxjs';
import ResponseModel from 'src/app/core/models/reponse-model';

@Injectable({
  providedIn: 'root'
})
export class TdcPriceRentRepository {
  public baseUrl = '/api/TdcPriceRent';

  constructor(private baseHttpClient: BaseHttpClient) {}
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
  public getById(id: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/${id}`
    });
  }

  public getWorkSheet(tdcPriceRentId: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetWorkSheet/${tdcPriceRentId}`
    });
  }
  public getExcelTable(tdcPriceRentId: number, param: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTable/${tdcPriceRentId}?dateTime=${param}`
    });
  }

  public Import(param: any, id: number) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/ImportExcel/${id}`,
      body: param
    });
  }

  public ExportExcel(Id: number, param: any) {
    return this.baseHttpClient.downloadFile({
      url: `${this.baseUrl}/ExportExcel/${Id}`,
      body: param
    });
  }

  public GetReportTable(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetReportTable`,
      body: param
    });
  }

  public ExportReport(param: any) {
    return this.baseHttpClient.downloadFile({
      url: `${this.baseUrl}/ExportReport`,
      body: param
    });
  }

  public getLog(tdcPriceRentId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/getLogTdcPriceRentOff/${tdcPriceRentId}`
    });
  }
}

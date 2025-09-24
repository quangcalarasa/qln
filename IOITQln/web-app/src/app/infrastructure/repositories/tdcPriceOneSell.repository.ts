import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
    providedIn: 'root'
  })
  export class TdcPriceOneSellRepository {
    public baseUrl = '/api/TdcPriceOneSell';
  
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
  public GetReportTable( param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetExTable`,
      body: param
    });
  }

  public getExportExcel( param:any){
    return this.baseHttpClient.downloadFile({
      url: `${this.baseUrl}/ExportExcel`,
      body: param
    })
  }

  public getLog(tdcPriceOneSellId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/getLogTdcOneSellOff/${tdcPriceOneSellId}`
    });
  }
}
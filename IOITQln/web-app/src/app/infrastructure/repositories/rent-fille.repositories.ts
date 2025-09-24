import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
  providedIn: 'root'
})
export class RentFileRepository {
  public baseUrl = '/api/RentFile';

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

  public async getDataGroupByCode(code: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GroupDataRentFile/${code}`
    });
  }

  public async GetExportWordPT09(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordPT9/GetExportWordPT09/${id}`,
      body: undefined
    });
  }

  public async GetExportWordPT10(id: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordPT9/GetExportHdctNoc/${id}`,
      body: undefined
    });
  }

  public async importDataExcel(param: any) {
    return this.baseHttpClient.uploadRequest({
      url: `${this.baseUrl}/ImportReceiptDataExcel`,
      body: param
    });
  }

  public async importContractDataExcel(param: any) {
    return this.baseHttpClient.uploadRequest({
      url: `${this.baseUrl}/ImportContractDataExcelType2`,
      body: param
    });
  }

  public async getPromissoryReport(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GetPromissoryReport`,
      body: param
    });
  }

  public async updatePromissoryReport(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/UpdatePromissoryReport`,
      body: param
    });
  }

  public async exportPromissoryReport(param: any) {
    return this.baseHttpClient.exportReport("danh_sach_phieu_thu_hop_dong_thue.xlsx", {
      url: `${this.baseUrl}/ExportPromissoryReport`,
      body: param
    });
  }
}

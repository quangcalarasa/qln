import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { AnyCatcher } from 'rxjs/internal/AnyCatcher';

@Injectable({
  providedIn: 'root'
})
export class RentFileBCTRepository {
  public baseUrl = '/api/RentFileBCT';

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

  public async getWorkSheetCN22(Id: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTableCNQD22/${Id}`
    });
  }

  public async getWorkSheetBCT22(param: any) {
    console.log("Tính toán bảng chiết tính")
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTableBCT22/${param.Id}`
    });
  }

  public async getWorkSheet1753(param: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTable1753/${param.Id}`
    });
  }

  public async getWorkSheet09(param: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTable09/${param.Id}`
    });
  }

  public GetByRentFileId(id: number, TypeBCT: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetByRentFileId/${id}/${TypeBCT}`
    });
  }

  public GetById(id: number) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetById/${id}`
    });
  }

  public ExportGetWorkSheet(id: number, param: any) {
    return this.baseHttpClient.exportReport("Bang_Chiet_Tinh_NOC.xlsx",{
      url: `${this.baseUrl}/ExportGetWorkSheet/${id}`,
      body: param
    });
  }

  public ExportGetWorkSheet2(id: number, param: any) {
    return this.baseHttpClient.exportReport("Bang_Chiet_Tinh_NOC.xlsx",{
      url: `${this.baseUrl}/ExportGetWorkSheet2/${id}`,
      body: param
    });
  }

  public async GetExcelTableBCT22TT(Id: any, rentFileBCTId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTableBCT22TT/${Id}/${rentFileBCTId}`
    });
  }

  public async GetExcelTable1753TT(Id: any, rentFileBCTId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTable1753TT/${Id}/${rentFileBCTId}`
    });
  }

  public async GetExcelTable09TT(Id: any, rentFileBCTId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTable09TT/${Id}/${rentFileBCTId}`
    });
  }

  /////

  public async GetExcelTableBCT22Us(Id: any, rentFileBCTId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTableBCT22Us/${Id}/${rentFileBCTId}`
    });
  }

  public async GetExcelTable1753Us(Id: any, rentFileBCTId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTable1753Us/${Id}/${rentFileBCTId}`
    });
  }

  public async GetExcelTable09Us(Id: any, rentFileBCTId: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetExcelTable09Us/${Id}/${rentFileBCTId}`
    });
  }

  public async exportReportTT(id: number, RentFileBCTId: number) {
    return this.baseHttpClient.exportFileWord({
      url: `/api/ExportWordTT/GetExportWordTT/${id}/${RentFileBCTId}`,
      body: undefined
    });
  }

  public async addNewBCT(body : any){
    console.log("Lưu bảng chiết tínhs")
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/addNewBCT`,
      body : body
    })
  }

  public async GetDataRentBCTTable(id : any){
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GetDataRentBCTTable/${id}`
    });
  }
  
  public GroupedData(Id: number, Type: any) {
    return this.baseHttpClient.getRequest({
      url: `${this.baseUrl}/GroupedData/${Id}/${Type}`
    });
  }
}

import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
  providedIn: 'root'
})
export class DebtsTableRepository {
  public baseUrl = '/api/DebtsTable';

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

  public async update(param: any,SurplusBalance? : any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/${param.Id}/${SurplusBalance}`,
      body: param
    });
  }

  public async delete(param: any) {
    return this.baseHttpClient.deleteRequest({
      url: `${this.baseUrl}/${param.Id}`
    });
  }

  public Post(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/DebtsTable`,
      body: param
    });
  }

  public Debts(id: number, param: any) {
    return this.baseHttpClient.exportReport("Bao_Cao_7.xlsx",{
      url: `${this.baseUrl}/Debts/${id}`,
      body: param
    });
  }

  public groupData(code: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/GroupDataDebtsTable`,
      body: { Code: code }
    });
  }

  public Import(param: any) {
    return this.baseHttpClient.postRequest({
      url: `${this.baseUrl}/ImportExcelDebts}`,
      body: param
    });
  }
}

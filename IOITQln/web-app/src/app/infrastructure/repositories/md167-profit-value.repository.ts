import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { TypeDecree } from 'src/app/shared/utils/enums';

@Injectable({
    providedIn: 'root',
})
export class Md167ProfitValueRepository {
    public baseUrl = "/api/Md167ProfitValue";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}`,
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

    public ExportExcel() {
        return this.baseHttpClient.downloadFile({
            url: `${this.baseUrl}/ExportExcel`
        });
    }

    public ImportDataExcel(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/ImportExcel`,
            body: param
        });
    }

}

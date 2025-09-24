import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
    providedIn: 'root',
})
export class QuickmathRepository {
    public baseUrl = "/api/QuickMath";

    constructor(private baseHttpClient: BaseHttpClient) { }
    
    public addNew(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/QuickMathPrice`,
            body: param
        });
    }
    
    public getByPage(params: GetByPageModel) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}`,
        });
    }

    public getView(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/GetView`,
            body: param
        });
    }

    public getLogs(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/GetLogView`,
            body: param
        });
    }
}

import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
    providedIn: 'root',
})
export class ExtraConfigNotiRepository {
    public baseUrl = "/api/ExtraConfigNotii";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public updatePort(param: any) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}`,
            body: param
        });
    }
    public getPort() {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}`,
        });
    }

}

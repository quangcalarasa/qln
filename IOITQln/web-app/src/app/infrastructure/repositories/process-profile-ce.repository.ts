import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';

@Injectable({
    providedIn: 'root',
})
export class ProcessProfileCeRepository {
    public baseUrl = "/api/ProcessProfileCe";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage${objectToQueryString(params)}`,
        });
    }
}

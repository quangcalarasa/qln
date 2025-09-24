import { Injectable } from '@angular/core';
import GetByPageModel from 'src/app/core/models/get-by-page-model';
import { BaseHttpClient } from '../http/base-http-client';
import { objectToQueryString } from '../utils/object-to-query-string';
import { TypeDecree } from 'src/app/shared/utils/enums';

@Injectable({
    providedIn: 'root',
})
export class DecreeRepository {
    public baseUrl = "/api/Decree";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public getByPage(params: GetByPageModel, type: TypeDecree) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetByPage/${type}${objectToQueryString(params)}`,
        });
    }

    public addNew(param: any, type: TypeDecree) {
        return this.baseHttpClient.postRequest({
            url: `${this.baseUrl}/${type}`,
            body: param
        });
    }

    public async update(param: any, type: TypeDecree) {
        return this.baseHttpClient.putRequest({
            url: `${this.baseUrl}/${type}/${param.Id}`,
            body: param
        });
    }

    public async delete(param: any, type: TypeDecree) {
        return this.baseHttpClient.deleteRequest({
            url: `${this.baseUrl}/${type}/${param.Id}`
        });
    }
}

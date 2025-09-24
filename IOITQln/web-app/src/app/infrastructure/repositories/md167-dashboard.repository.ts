import { Injectable } from '@angular/core';
import { BaseHttpClient } from '../http/base-http-client';

@Injectable({
    providedIn: 'root',
})
export class Md167DashboardRepository {
    public baseUrl = "/api/Md167Dashboard";

    constructor(private baseHttpClient: BaseHttpClient) { }

    public GetHouseBase() {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetHouseBase`,
        });
    }

    public GetNewHouseBase(month: number, year: number) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetNewHouseBase/${year}/${month}`,
        });
    }

    public GetRevenue(month: number, year: number) {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetRevenue/${year}/${month}`,
        });
    }

    public GetTaxInfo() {
        return this.baseHttpClient.getRequest({
            url: `${this.baseUrl}/GetTaxInfo`,
        });
    }
}

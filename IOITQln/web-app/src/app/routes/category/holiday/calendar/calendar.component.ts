import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { HolidayRepository } from 'src/app/infrastructure/repositories/holiday.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';

@Component({
    selector: 'app-add-or-update-calendar',
    templateUrl: './calendar.component.html',
    styles: [
        `
        .events {
            list-style: none;
            margin: 0;
            padding: 0;
        }

        .events .ant-badge-status {
            width: 100%;
            font-size: 12px;
            overflow-x: hidden;
        }

        .notes-month {
            text-align: center;
            font-size: 28px;
        }

        .notes-month section {
            font-size: 28px;
        }
        `
    ]
})

export class CalendarComponent implements OnInit {
    validateForm!: FormGroup;
    loading: boolean = false;
    curr_date: Date = new Date();
    data: [] = [];

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder,
        private holidayRepository: HolidayRepository) { }

    ngOnInit(): void {
        this.getByMonthAndYear();
    }

    close(): void {
        this.drawerRef.close();
    }

    genMsg(date: any): any {
        let res: any = [];
        this.data.forEach((element: any) => {
            if (new Date(element.StartDate).getTime() <= date.getTime() && date.getTime() <= new Date(element.EndDate).getTime()) {
                res.push(element.Name);
            }
        });

        return res;
    }

    async getByMonthAndYear() {
        let month = this.curr_date.getMonth();
        let year = this.curr_date.getFullYear();

        const resp = await this.holidayRepository.getByMonthAndYear(month + 1, year);
        if (resp.meta?.error_code == 200) {
            this.data = resp.data;
        }
    }
}

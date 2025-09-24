import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';

import { convertDate } from 'src/app/infrastructure/utils/common';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import * as XLSX from 'xlsx';
import { DebtsTableRepository } from 'src/app/infrastructure/repositories/DebtsTable.repository';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
@Component({
  selector: 'app-import-debt',
  templateUrl: './import-debt.component.html',
  styles: []
})
export class ImportDebtComponent implements OnInit {
  @Input() code: string;
  @Input() RentFileId: NzSafeAny;
  dataImport: any[] = [];

  columns: string[] = [
    'DateStart',
    'DateEnd',
    'Price',
    'Check',
    'Executor',
    'Date',
    'NearestActivities',
    'PriceDiff',
    'AmountExclude',
    'VATPrice',
    'CheckPayDepartment',
    'RentFileId'
  ];
  data: any[] = [];
  dataTable: {
    DateStart: any;
    DateEnd: any;
    Price: any;
    Check: any;
    Executor: any;
    Date: any;
    NearestActivities: any;
    PriceDiff: any;
    AmountExclude: any;
    VATPrice: any;
    CheckPayDepartment: any;
    Index: any;
    RentFileId: any;
  }[] = [];
  loading: boolean = false;

  constructor(private drawerRef: NzDrawerRef<string>, private debtsTableRepository: DebtsTableRepository) {}

  ngOnInit(): void {}

  convertExcelNumberToDate(excelNumber: number): any {
    const timestamp = (excelNumber - 2) * (1000 * 60 * 60 * 24) + new Date('1900-01-01').getTime();
    const dateObj = new Date(timestamp);
    const year = dateObj.getFullYear();
    const month = String(dateObj.getMonth() + 1).padStart(2, '0');
    const day = String(dateObj.getDate()).padStart(2, '0');
    const dateStr = `${month}/${day}/${year}`;
    const date = convertDate(dateStr);
    return date;
  }

  onFileChange(event: any) {
    const target: DataTransfer = event.target;
    if (target.files.length !== 1) throw new Error('Cannot use multiple files');

    const reader: FileReader = new FileReader();
    reader.onload = (e: any) => {
      const bstr: string = e.target.result;
      const wb: XLSX.WorkBook = XLSX.read(bstr, { type: 'binary' });

      const wsname: string = wb.SheetNames[0];
      const ws: XLSX.WorkSheet = wb.Sheets[wsname];

      const jsonData: any[] = XLSX.utils.sheet_to_json(ws, { header: 1 });

      this.data = jsonData.slice(1).map((row: any[]) => {
        const obj: any = {};
        for (let i = 0; i < this.columns.length; i++) {
          const column = this.columns[i];
          let value;
          if (column === 'DateStart' && typeof row[i] === 'number') {
            value = this.convertExcelNumberToDate(row[i]);
          } else if (column === 'DateEnd' && typeof row[i] === 'number') {
            value = this.convertExcelNumberToDate(row[i]);
          } else if (column === 'Check') {
            value = row[i] === 'x' ? true : false;
          } else if (column === 'Date' && typeof row[i] === 'number') {
            value = this.convertExcelNumberToDate(row[i]);
          } else if (column === 'CheckPayDepartment') {
            value = row[i] === 'x' ? true : false;
          } else value = row[i];
          obj[column] = value;
        }
        return obj;
      });
      this.dataTable = this.data.map(row => {
        return {
          DateStart: row.DateStart,
          DateEnd: row.DateEnd,
          Price: row.Price,
          Check: row.Check,
          Executor: row.Executor,
          Date: row.Date,
          NearestActivities: row.NearestActivities,
          PriceDiff: row.PriceDiff,
          AmountExclude: row.AmountExclude,
          VATPrice: row.VATPrice,
          CheckPayDepartment: row.CheckPayDepartment,
          Index: 0,
          RentFileId: this.RentFileId
        };
      });
    };

    reader.readAsBinaryString(target.files[0]);
  }
  async onSubmit() {
    let data = [...this.dataTable];
    let input = {
      Code: this.code,
      data: data
    };
    const resp = await this.debtsTableRepository.Import(input);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }
}

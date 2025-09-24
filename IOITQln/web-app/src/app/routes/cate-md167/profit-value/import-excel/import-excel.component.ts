import { Component, Input, OnInit } from '@angular/core';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import * as XLSX from 'xlsx';
import { Md167ProfitValueRepository } from 'src/app/infrastructure/repositories/md167-profit-value.repository';
import { convertDate } from '../../../../infrastructure/utils/common';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';

@Component({
  selector: 'app-import-excel',
  templateUrl: './import-excel.component.html',
  styles: [
  ]
})
export class ImportExcelMd167PVComponent implements OnInit {

  @Input() record: NzSafeAny;
  dataImport: any[] = [];
  selectedFile: File | undefined;
  columns: string[] = [
    'STT',
    'DoApply',
    'Value',
    'UnitPriceName',
    'Note'
  ];
  data: any[] = [];
  dataTable: {
    DoApply: any;
    Value: number;
    UnitPriceName: any;
    Note: any;
  }[] = [];
  loading: boolean = false;
  constructor(private drawerRef: NzDrawerRef<string>, private md167ProfitValueRepository: Md167ProfitValueRepository) { }
  ngOnInit(): void { }

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
    this.selectedFile = event.target.files[0];
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
          if (column === 'DoApply' && row[i] !== undefined && typeof row[i] !== 'string') {
            value = this.convertExcelNumberToDate(row[i]);
          }
          else if (column === 'Value' && row[i] !== undefined && typeof row[i] !== 'number') {
            value = this.convertExcelNumberToDate(row[i]);
          }
          else if (column === 'UnitPriceName' && row[i] !== undefined && typeof row[i] !== 'string') {
            value = this.convertExcelNumberToDate(row[i]);
          }
          else if (column === 'Note' && row[i] !== undefined && typeof row[i] !== 'string') {
            value = this.convertExcelNumberToDate(row[i]);
          }
          else
            value = row[i];

          if (typeof value === 'number' && isNaN(value))
            value = null
          obj[column] = value;
        }
        return obj;
      });
      this.dataTable = this.data.map(row => {
        return {
          DoApply: row.DoApply,
          Value: row.Value,
          UnitPriceName: row.UnitPriceName,
          Note: row.Note,

        }
      });
      console.log(this.dataTable);

    };
    reader.readAsBinaryString(target.files[0]);
  }

  async onSubmit() {
    let data = [...this.dataTable]
    console.log(data);

    const resp = await this.md167ProfitValueRepository.ImportDataExcel(data);
    if (resp.meta?.error_code == 200) {
      this.loading = false;
      this.drawerRef.close(data);
    } else {
      this.loading = false;
    }
  }
  close() {
    this.drawerRef.close();
  }

  btn() {
    console.log(this.data);
  }
}
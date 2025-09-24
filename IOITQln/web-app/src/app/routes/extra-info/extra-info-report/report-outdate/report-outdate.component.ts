import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-outdate',
  templateUrl: './report-outdate.component.html',
  styles: [
  ]
})
export class ReportOutdateComponent implements OnInit {

  validateForm!: FormGroup;

  loading = false;
  dataCopy: any[];
  data =[{
      Id:'1',
      Code: 'HD1',
      Status:'Đã hỗ trợ',
      FromDate:'1/10/2023',
      ToDate:'16/10/2023',
      HouseNb: '1',
      Lane: 'Nguyễn Thị Kiêu',
      Ward:'Phường Hiệp Thành',
      District:'Quận 12',
      Name:'Vũ Mạnh Dũng',
      CCCD:'120321034234',
      Cast:'1,000,000',
      
    },
    { 
      Id:'2',
      Code: 'HD2',
      Status:'Đã tiếp nhận',
      FromDate:'1/10/2023',
      ToDate:'16/10/2023',
      HouseNb: 'Nhà 1',
      Lane: 'Lương Định Của',
      Ward:'Phường An Khánh',
      District:'Quận 2',
      Name:'Phạm Thành Quang',
      CCCD:'120432034234',
      Cast:'5,000,000',
    },
    {
      Id:'3',
      Code: 'HD3',
      Status:'Không tiếp nhận',
      FromDate:'1/10/2023',
      ToDate:'16/10/2023',
      HouseNb: '4',
      Lane: 'Nguyễn Hữu Cảnh',
      Ward:'Phường Tân Định',
      District:'Quận 1',
      Name:'Nguyễn Hải Chiều',
      CCCD:'120675034234',
      Cast:'10,000,000',
    },
  ]

  constructor() { }

  ngOnInit(
    dataCopy = [...this.data]
  ): void {
  }

  onBack() {
    window.history.back();
  }

  onViewData() {
    this.loading = !this.loading;
  }
}

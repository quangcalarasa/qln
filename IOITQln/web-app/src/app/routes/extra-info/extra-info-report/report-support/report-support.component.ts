import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-support',
  templateUrl: './report-support.component.html',
  styles: [
  ]
})
export class ReportSupportComponent implements OnInit {
  validateForm!: FormGroup;

  loading = false;
  dataCopy: any[];
  data =[{
    Id:'1',
    Code: 'MDD1',
    House: '88',
    Apartment: 'D3',
    TypeRequest:'Đang soạn',
    Content:'Yêu cầu hỗ trợ bổ sung',
    Requester:'Vũ Mạnh Dũng',
    FromDate:'1/10/2023',
    ToDate:'16/10/2023',
    Reciever:'Nhân viên',
    Status:'Hoàn thành',
  },
  {
    Id:'2',
    Code: 'MDD2',
    House: '20',
    Apartment: '56', 
    TypeRequest:'Đã gửi yêu cầu',
    Content:'Yêu cầu hỗ trợ sửa',
    Requester:'Đặng Hồng Nhung',
    FromDate:'1/10/2023',
    ToDate:'16/10/2023',
    Reciever:'Nhân viên',
    Status:'Hoàn thành',
  },
  {
    Id:'3',
    Code: 'MDD3',
    House: '87',
    Apartment: '5',
    TypeRequest:'Đã tiếp nhận',
    Content:'Yêu cầu hỗ trợ',
    Requester:'Vũ Ngọc Thiện',
    FromDate:'1/10/2023',
    ToDate:'16/10/2023',
    Reciever:'Quản lý',
    Status:'Hoàn thành',
  },
  {
    Id:'4',
    Code: 'MDD4',
    House: '66',
    Apartment: 'C6',
    TypeRequest:'Đã tiếp nhận',
    Content:'Yêu cầu hỗ trợ tiếp nhận',
    Requester:'Nguyễn Hải Chiều',
    FromDate:'1/10/2023',
    ToDate:'16/10/2023',
    Reciever:'Nhân viên',
    Status:'Hủy',
  },
  {
    Id:'5',
    Code: 'MDD5',
    House: 'B102',
    Apartment: 'B103',
    TypeRequest:'Đang soạn',
    Content:'Yêu cầu hỗ trợ',
    Requester:'Phạm Thành Quang',
    FromDate:'1/10/2023',
    ToDate:'16/10/2023',
    Reciever:'Quản lý',
    Status:'Hủy',
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

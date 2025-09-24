import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'app-report-news',
  templateUrl: './report-news.component.html',
  styles: [
  ]
})
export class ReportNewsComponent implements OnInit {

  validateForm!: FormGroup;

  loading = false;
  dataCopy: any[];
  data =[{
      Id:'1',
      TypeNew:'Tin nóng',
      CountNew:'1',
      CountImage:'2',
      CountFile:'1',
      FromDate:'1/10/2023',
      ToDate:'16/10/2023',
    },
    {
      Id:'2',
      TypeNew:'Thời tiết',
      CountNew:'2',
      CountImage:'5',
      CountFile:'1',
      FromDate:'1/10/2023',
      ToDate:'16/10/2023', 
    },
    {
      Id:'3',
      TypeNew:'Bóng đá',
      CountNew:'2',
      CountImage:'6',
      CountFile:'1',
      FromDate:'1/10/2023',
      ToDate:'16/10/2023',
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


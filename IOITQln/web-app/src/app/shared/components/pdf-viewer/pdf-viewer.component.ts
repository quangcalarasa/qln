import { Component, Input, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { _HttpClient } from '@delon/theme';
@Component({
  selector: 'app-pdf-viewer',
  templateUrl: './pdf-viewer.component.html',
})
export class PDFViewerComponent implements OnInit {
  @Input() url?: string;
  @Input() base64?: string;
  src!: any;
  constructor(private sanitizer: DomSanitizer) {
  }
  ngOnInit(): void {
    if (this.url)
      this.src = this.sanitizer.bypassSecurityTrustResourceUrl(this.url);
    else if (this.base64)
      this.src = this.sanitizer.bypassSecurityTrustResourceUrl('data:application/pdf;base64,' + this.base64);
  }
}

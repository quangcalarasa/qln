import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { FormGroup } from '@angular/forms';
import { LevelBlock } from 'src/app/shared/utils/consts';
import { TypeReportApplyEnum } from 'src/app/shared/utils/enums';
import { NzUploadFile, NzUploadXHRArgs } from 'ng-zorro-antd/upload';
import { UploadRepository } from 'src/app/infrastructure/repositories/upload.repository';
import { UserService } from 'src/app/core/services/user.service';

@Component({
    selector: 'app-block-block-info',
    templateUrl: './block-info.component.html'
})

export class BlockInfoComponent implements OnInit {
    @Output() eventEmitter: EventEmitter<any> = new EventEmitter();

    @Input() validateForm: FormGroup;
    @Input() typehouse_data: any[] = [];

    levelblockmap_data = LevelBlock;
    typeReportApplyEnum = TypeReportApplyEnum;

    userId = this.userService.getLoggedUser()['Id'];

    constructor(
        private uploadRepository: UploadRepository, private userService: UserService
    ) { }

    ngOnInit(): void {
    }

    compareFn = (o1: any, o2: any) => {
        return (o1 && o2 ? o1.key === o2.key || o1.LevelId === parseInt(o2.key) : o1 === o2);
    };

    changeLevelBlock(evt: any) {
        this.eventEmitter.emit(evt);
    }

    beforeUpload = (file: NzUploadFile): boolean => {
        this.handleChange(file);
        return false;
    };

    async handleChange(file: any) {
        const formData = new FormData();
        formData.append(file.name, file);

        const resp = await this.uploadRepository.uploadFile(formData);
        this.validateForm.get('Attactment')?.setValue(resp?.data.toString());
    }

    downloadFile(fileName: string) {
        this.uploadRepository.downloadFile(fileName);
    }
}

import { Component, Input, OnInit } from '@angular/core';
import { _HttpClient } from '@delon/theme';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { RoleRepository } from 'src/app/infrastructure/repositories/role.repository';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzSafeAny } from 'ng-zorro-antd/core/types';
import { FunctionRepository } from 'src/app/infrastructure/repositories/function.repository';

@Component({
    selector: 'app-add-or-update-role',
    templateUrl: './add-or-update-role.component.html'
})

export class AddOrUpdateRoleComponent implements OnInit {
    @Input() record: NzSafeAny;
    @Input() listFunctionInput: any[] = [];

    nzScrollYValue = window.innerHeight - 450 + 'px';
    validateForm!: FormGroup;
    loading: boolean = false;
    action = { View: false, Create: false, Update: false, Delete: false, Import: false, Export: false, Print: false, Other: false, Menu: false };
    indeterminate = { View: false, Create: false, Update: false, Delete: false, Import: false, Export: false, Print: false, Other: false, Menu: false };

    constructor(private drawerRef: NzDrawerRef<string>, private fb: FormBuilder, private roleRepository: RoleRepository, private message: NzMessageService, private functionRepository: FunctionRepository) { }

    ngOnInit(): void {
        this.validateForm = this.fb.group({
            Id: [this.record ? this.record.Id : undefined],
            Code: [this.record ? this.record.Code : undefined, [Validators.required]],
            Name: [this.record ? this.record.Name : undefined, [Validators.required]],
            LevelRole: [this.record ? this.record.LevelRole : undefined, [Validators.min(1)]],
            Note: [this.record ? this.record.Note : undefined],
            functionRoles: []
        });

        this.getListFunction();
    }

    convertListFunctionInput() {
        if (this.record) {
            for (let i = 0; i < this.listFunctionInput.length; i++) {
                for (let j = 0; j < this.record.functionRoles.length; j++) {
                    if (this.listFunctionInput[i].Id == this.record.functionRoles[j].FunctionId) {
                        this.listFunctionInput[i].View = this.record.functionRoles[j].ActiveKey[0] == "1" ? true : false;
                        this.listFunctionInput[i].Create = this.record.functionRoles[j].ActiveKey[1] == "1" ? true : false;
                        this.listFunctionInput[i].Update = this.record.functionRoles[j].ActiveKey[2] == "1" ? true : false;
                        this.listFunctionInput[i].Delete = this.record.functionRoles[j].ActiveKey[3] == "1" ? true : false;
                        this.listFunctionInput[i].Import = this.record.functionRoles[j].ActiveKey[4] == "1" ? true : false;
                        this.listFunctionInput[i].Export = this.record.functionRoles[j].ActiveKey[5] == "1" ? true : false;
                        this.listFunctionInput[i].Print = this.record.functionRoles[j].ActiveKey[6] == "1" ? true : false;
                        this.listFunctionInput[i].Other = this.record.functionRoles[j].ActiveKey[7] == "1" ? true : false;
                        this.listFunctionInput[i].Menu = this.record.functionRoles[j].ActiveKey[8] == "1" ? true : false;
                        this.listFunctionInput[i].All = this.record.functionRoles[j].ActiveKey.indexOf("0") == -1 ? true : false;
                        this.listFunctionInput[i].indeterminateAll = this.record.functionRoles[j].ActiveKey.indexOf("1") != -1 && !this.listFunctionInput[i].All ? true : false;
                        break;
                    }
                    else {
                        this.listFunctionInput[i].View = false;
                        this.listFunctionInput[i].Create = false;
                        this.listFunctionInput[i].Update = false;
                        this.listFunctionInput[i].Delete = false;
                        this.listFunctionInput[i].Import = false;
                        this.listFunctionInput[i].Export = false;
                        this.listFunctionInput[i].Print = false;
                        this.listFunctionInput[i].Other = false;
                        this.listFunctionInput[i].Menu = false;
                        this.listFunctionInput[i].All = false;
                        this.listFunctionInput[i].indeterminateAll = false;
                    }
                }

                this.listFunctionInput[i].Space = "";
                for (let idx = 0; idx < (this.listFunctionInput[i].Level) * 7; idx++) {
                    this.listFunctionInput[i].Space += "&nbsp;";
                }
            }
        }
        else {
            this.listFunctionInput.forEach(item => {
                item.Space = "";
                item.View = false;
                item.Create = false;
                item.Update = false;
                item.Delete = false;
                item.Import = false;
                item.Export = false;
                item.Print = false;
                item.Other = false;
                item.Menu = false;
                item.All = false;
                item.indeterminateAll = false;
                for (var i = 0; i < (item.Level) * 7; i++) {
                    item.Space += "&nbsp;";
                }
            });
        }

        if (this.listFunctionInput.length > 0) {
            let index = 0;
            Object.keys(this.listFunctionInput[index]).forEach(key => {
                if (key == "View" || key == "Create" || key == "Update" || key == "Delete" || key == "Import" || key == "Export" || key == "Print" || key == "Other" || key == "Menu") {
                    this.refreshStatus(key);
                }
            });
        }
    }

    checkAll(value: boolean, keyCheck: string) {
        this.listFunctionInput.forEach((data, index) => {
            if (keyCheck == "View" || (keyCheck != "View" && !data.IsSpecialFunc)) {
                data[keyCheck] = value;
                this.checkCell(index);
            }
        });

        this.refreshStatus(keyCheck);
    }

    refreshStatus(keyCheck: string) {
        let allChecked = undefined;
        let allUnChecked = undefined;

        if (keyCheck == "View") {
            allChecked = this.listFunctionInput.length > 0 && this.listFunctionInput.every(value => value[keyCheck] === true);
            allUnChecked = this.listFunctionInput.every(value => !value[keyCheck]);
        }
        else {
            allChecked = this.listFunctionInput.length > 0 && this.listFunctionInput.filter(x => !x.IsSpecialFunc).every(value => value[keyCheck] === true);
            allUnChecked = this.listFunctionInput.filter(x => !x.IsSpecialFunc).every(value => !value[keyCheck]);
        }

        switch (keyCheck) {
            case "View":
                this.action.View = allChecked;
                this.indeterminate.View = !allChecked && !allUnChecked;
                break;
            case "Create":
                this.action.Create = allChecked;
                this.indeterminate.Create = !allChecked && !allUnChecked;
                break;
            case "Update":
                this.action.Update = allChecked;
                this.indeterminate.Update = !allChecked && !allUnChecked;
                break;
            case "Delete":
                this.action.Delete = allChecked;
                this.indeterminate.Delete = !allChecked && !allUnChecked;
                break;
            case "Import":
                this.action.Import = allChecked;
                this.indeterminate.Import = !allChecked && !allUnChecked;
                break;
            case "Export":
                this.action.Export = allChecked;
                this.indeterminate.Export = !allChecked && !allUnChecked;
                break;
            case "Print":
                this.action.Print = allChecked;
                this.indeterminate.Print = !allChecked && !allUnChecked;
                break;
            case "Other":
                this.action.Other = allChecked;
                this.indeterminate.Other = !allChecked && !allUnChecked;
                break;
            case "Menu":
                this.action.Menu = allChecked;
                this.indeterminate.Menu = !allChecked && !allUnChecked;
                break;
            default:
                break;
        }
    }

    checkAllRow(index: number) {
        const check = this.listFunctionInput[index].All;

        Object.keys(this.listFunctionInput[index]).forEach(key => {
            if (key == "View" || key == "Create" || key == "Update" || key == "Delete" || key == "Import" || key == "Export" || key == "Print" || key == "Other" || key == "Menu") {
                this.listFunctionInput[index][key] = check;
                this.refreshStatus(key);
            }
        });

        this.listFunctionInput[index].indeterminateAll = false;
    }

    checkCell(index: number) {
        this.listFunctionInput[index].indeterminateAll = false;
        this.listFunctionInput[index].All = true;
        for (let key of Object.keys(this.listFunctionInput[index])) {
            this.listFunctionInput[index].indeterminateAll = (key == "View" || key == "Create" || key == "Update" || key == "Delete" || key == "Import" || key == "Export" || key == "Print" || key == "Other" || key == "Menu") && this.listFunctionInput[index][key] == true;
            if (this.listFunctionInput[index].indeterminateAll) break;
        }

        for (let key of Object.keys(this.listFunctionInput[index])) {
            if (key == "View" || key == "Create" || key == "Update" || key == "Delete" || key == "Import" || key == "Export" || key == "Print" || key == "Other" || key == "Menu") {
                this.listFunctionInput[index].All = !this.listFunctionInput[index][key] ? false : true;
                if (!this.listFunctionInput[index].All) break;
            }
        }

        this.listFunctionInput[index].indeterminateAll = !this.listFunctionInput[index].All && this.listFunctionInput[index].indeterminateAll;
    }

    async submitForm() {
        this.loading = true;
        let data = { ...this.validateForm.value };
        data.functionRoles = [];

        this.listFunctionInput.forEach(item => {
            let newFunc: { FunctionId?: number, ActiveKey?: string } = {};
            newFunc.FunctionId = item.Id;
            newFunc.ActiveKey = "";
            newFunc.ActiveKey += item.View == true ? 1 : 0;
            newFunc.ActiveKey += item.Create == true ? 1 : 0;
            newFunc.ActiveKey += item.Update == true ? 1 : 0;
            newFunc.ActiveKey += item.Delete == true ? 1 : 0;
            newFunc.ActiveKey += item.Import == true ? 1 : 0;
            newFunc.ActiveKey += item.Export == true ? 1 : 0;
            newFunc.ActiveKey += item.Print == true ? 1 : 0;
            newFunc.ActiveKey += item.Other == true ? 1 : 0;
            newFunc.ActiveKey += item.Menu == true ? 1 : 0;

            data.functionRoles.push(newFunc);
        });

        const resp = data.Id ? await this.roleRepository.update(data) : await this.roleRepository.addNew(data);
        if (resp.meta?.error_code == 200) {
            this.loading = false;
            this.drawerRef.close(data);
        }
        else {
            this.loading = false;
        }
    }

    close(): void {
        this.drawerRef.close();
    }

    async getListFunction() {
        const resp = await this.functionRepository.getFunctionTreeModel();
        if (resp.meta?.error_code == 200) {
            this.listFunctionInput = resp.data;
            this.convertListFunctionInput();
        }
    }
}

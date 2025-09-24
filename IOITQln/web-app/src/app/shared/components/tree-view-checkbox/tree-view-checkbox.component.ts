import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { Component, Input, OnInit } from '@angular/core';

import { NzTreeFlatDataSource, NzTreeFlattener } from 'ng-zorro-antd/tree-view';

interface TreeNode {
    id: number;
    name: string;
    disabled?: boolean;
    selected?: boolean;
    children?: TreeNode[];
}

interface FlatNode {
    id: number;
    expandable: boolean;
    name: string;
    level: number;
    disabled: boolean;
    selected?: boolean;
}

@Component({
    selector: 'shared-tree-view-checkbox',
    template: `
    <div style="height: 350px; overflow-y: scroll;padding: 3px;border: 1px solid #ccc;border-radius: 3px;">
        <div class="p-md">
            <label class="mr-xxl" (click)="treeControl.collapseAll()" style="cursor: pointer;">
                <span nz-icon nzType="compress" nzTheme="outline"></span>
                &nbsp;Đóng tất cả
            </label>
            <label class="mr-xxl" (click)="treeControl.expandAll()" style="cursor: pointer;">
                <span nz-icon nzType="expand" nzTheme="outline"></span>
                &nbsp;Mở tất cả
            </label>
            <label nz-checkbox [(ngModel)]="isSelectAll" (ngModelChange)="selectAll()">
                <span>
                    Chọn tất cả
                </span>
            </label>
        </div>
        <nz-tree-view [nzTreeControl]="treeControl" [nzDataSource]="dataSource">
            <nz-tree-node *nzTreeNodeDef="let node" nzTreeNodePadding>
                <nz-tree-node-toggle nzTreeNodeNoopToggle></nz-tree-node-toggle>
                <nz-tree-node-checkbox
                [nzDisabled]="node.disabled"
                [nzChecked]="checklistSelection.isSelected(node)"
                (nzClick)="leafItemSelectionToggle(node)"
                ></nz-tree-node-checkbox>
                <nz-tree-node-option [nzDisabled]="node.disabled" (nzClick)="leafItemSelectionToggle(node)">
                {{ node.name }}
                </nz-tree-node-option>
            </nz-tree-node>

            <nz-tree-node *nzTreeNodeDef="let node; when: hasChild" nzTreeNodePadding>
                <nz-tree-node-toggle>
                <span nz-icon nzType="caret-down" nzTreeNodeToggleRotateIcon></span>
                </nz-tree-node-toggle>
                <nz-tree-node-checkbox
                [nzDisabled]="node.disabled"
                [nzChecked]="descendantsAllSelected(node)"
                [nzIndeterminate]="descendantsPartiallySelected(node)"
                (nzClick)="itemSelectionToggle(node)"
                ></nz-tree-node-checkbox>
                <nz-tree-node-option [nzDisabled]="node.disabled" (nzClick)="itemSelectionToggle(node)">
                {{ node.name }}
                </nz-tree-node-option>
            </nz-tree-node>
        </nz-tree-view>
    </div>
  `,
})
export class SharedTreeViewCheckboxComponent implements OnInit {
    private transformer = (node: TreeNode, level: number): FlatNode => {
        const existingNode = this.nestedNodeMap.get(node);
        const flatNode =
            existingNode && existingNode.name === node.name
                ? existingNode
                : {
                    id: node.id,
                    expandable: !!node.children && node.children.length > 0,
                    name: node.name,
                    level,
                    disabled: !!node.disabled
                };
        this.flatNodeMap.set(flatNode, node);
        this.nestedNodeMap.set(node, flatNode);
        return flatNode;
    };
    flatNodeMap = new Map<FlatNode, TreeNode>();
    nestedNodeMap = new Map<TreeNode, FlatNode>();
    checklistSelection = new SelectionModel<FlatNode>(true);

    treeControl = new FlatTreeControl<FlatNode>(
        (node) => node.level,
        (node) => node.expandable
    );

    treeFlattener = new NzTreeFlattener(
        this.transformer,
        (node) => node.level,
        (node) => node.expandable,
        (node) => node.children
    );

    dataSource = new NzTreeFlatDataSource(this.treeControl, this.treeFlattener);

    @Input() districts: any[];
    @Input() wards: any[];
    @Input() wardManagements: any[];

    data: TreeNode[] = [];
    isSelectAll?: boolean;

    constructor() {
    }

    ngOnInit(): void {
        //Convert data
        this.districts.forEach((district: any) => {
            let flatNode:TreeNode = { id: district.Id, name: district.Name, children: [] };
            flatNode.children = this.wards.filter((x:any) => x.DistrictId == district.Id).map((item: any) => {
                let  flatItemNode:TreeNode = { id: item.Id, name: item.Name, children: [] };
                return flatItemNode;
            });

            this.data.push(flatNode);
        });

        this.dataSource.setData(this.data);

        if(this.wardManagements) {
            this.treeControl.dataNodes.map(node => {
                let ward = this.wardManagements.find(x => x.WardId == node.id);
                if(ward) 
                    this.checklistSelection.select(node);
            });
        }

        this.treeControl.expandAll();
    }

    hasChild = (_: number, node: FlatNode): boolean => node.expandable;

    descendantsAllSelected(node: FlatNode): boolean {
        const descendants = this.treeControl.getDescendants(node);
        return (
            descendants.length > 0 &&
            descendants.every((child) => this.checklistSelection.isSelected(child))
        );
    }

    descendantsPartiallySelected(node: FlatNode): boolean {
        const descendants = this.treeControl.getDescendants(node);
        const result = descendants.some((child) =>
            this.checklistSelection.isSelected(child)
        );
        return result && !this.descendantsAllSelected(node);
    }

    leafItemSelectionToggle(node: FlatNode): void {
        this.checklistSelection.toggle(node);
        this.checkAllParentsSelection(node);
    }

    itemSelectionToggle(node: FlatNode): void {
        this.checklistSelection.toggle(node);
        const descendants = this.treeControl.getDescendants(node);
        this.checklistSelection.isSelected(node)
            ? this.checklistSelection.select(...descendants)
            : this.checklistSelection.deselect(...descendants);

        descendants.forEach((child) => this.checklistSelection.isSelected(child));
        this.checkAllParentsSelection(node);
    }

    checkAllParentsSelection(node: FlatNode): void {
        let parent: FlatNode | null = this.getParentNode(node);
        while (parent) {
            this.checkRootNodeSelection(parent);
            parent = this.getParentNode(parent);
        }
    }

    checkRootNodeSelection(node: FlatNode): void {
        const nodeSelected = this.checklistSelection.isSelected(node);
        const descendants = this.treeControl.getDescendants(node);
        const descAllSelected =
            descendants.length > 0 &&
            descendants.every((child) => this.checklistSelection.isSelected(child));
        if (nodeSelected && !descAllSelected) {
            this.checklistSelection.deselect(node);
        } else if (!nodeSelected && descAllSelected) {
            this.checklistSelection.select(node);
        }
    }

    getParentNode(node: FlatNode): FlatNode | null {
        const currentLevel = node.level;

        if (currentLevel < 1) {
            return null;
        }

        const startIndex = this.treeControl.dataNodes.indexOf(node) - 1;

        for (let i = startIndex; i >= 0; i--) {
            const currentNode = this.treeControl.dataNodes[i];

            if (currentNode.level < currentLevel) {
                return currentNode;
            }
        }
        return null;
    }

    selectAll() {
        // console.log(evt);
        if(this.isSelectAll) {
            this.checklistSelection.select(...this.treeControl.dataNodes);
        }
        else {
            this.checklistSelection.deselect(...this.treeControl.dataNodes);
        }
    }

    getDataSelected() {
        let res = this.treeControl.dataNodes.filter(x => x.level == 1).map(node => {
            node.selected = this.checklistSelection.isSelected(node);
            let item:any = {};
            item.WardId = node.id;
            item.WardName = node.name;
            item.selected = node.selected;
            return item;
        });

        return res.filter( x => x.selected);
    }
}

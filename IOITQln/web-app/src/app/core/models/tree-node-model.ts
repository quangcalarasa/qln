export default class TreeNodeModel {
    public title?: string;
    public key?: number;
    public isLeaf?: boolean;
    public disabled?: boolean = false;
    public children?: Array<TreeNodeModel>;
}
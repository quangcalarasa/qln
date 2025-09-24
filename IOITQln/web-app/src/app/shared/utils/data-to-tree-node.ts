import TreeNodeModel from 'src/app/core/models/tree-node-model';

export function dataToTreeNode(data: any[], key: string, keyTitle: string, keyParent: string, targetId?: number, parentId?: number, disabled?: boolean) {
    let dataRes = data.filter(x => x[keyParent] == parentId);

    let dataInput = data.filter(x => x[keyParent] != parentId);
    let dataOutput = Array<TreeNodeModel>();
    dataRes.forEach(item => {
        let treeNodeModel = new TreeNodeModel();
        treeNodeModel.key = item[key];
        treeNodeModel.title = item[keyTitle];
        treeNodeModel.disabled = targetId ? (treeNodeModel.key == targetId ? true : (disabled ? disabled : false)) : false;

        treeNodeModel.children = dataToTreeNode(dataInput, key, keyTitle, keyParent, targetId, treeNodeModel.key, treeNodeModel.disabled);
        treeNodeModel.isLeaf = treeNodeModel.children?.length == 0 ? true : false;

        dataOutput.push(treeNodeModel);
    });
    
    return dataOutput;
}
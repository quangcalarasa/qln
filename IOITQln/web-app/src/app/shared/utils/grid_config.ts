import { ColDef, GridApi, GridOptions, SuppressKeyboardEventParams } from "ag-grid-community";
import { AG_GRID_LOCALE_VI } from "./ag_grid_locale_vi";
import { RowStatus } from "./enums";

export const gridOptionsBase: GridOptions = {
    pagination: false,
    rowSelection: "multiple",
    suppressRowClickSelection: true,
    stopEditingWhenCellsLoseFocus: true,
    enableFillHandle: true,
    enableRangeSelection: true,
    headerHeight: 28,
    getRowHeight: () => 28,
    localeText: AG_GRID_LOCALE_VI,
    suppressContextMenu: true,
    // suppressKeyboardEvent: params => {
    //     if (!params.editing) {
    //         let isBackspaceKey = params.event.keyCode === 8;
    //         let isDeleteKey = params.event.keyCode === 46;

    //         if (isBackspaceKey || isDeleteKey) {
    //             params.api.getCellRanges()?.forEach(range => {
    //                 let colIds = range.columns.map(col => col.getColId());

    //                 if (range.startRow != undefined && range.startRow != undefined && range.endRow != undefined) {
    //                     let startRowIndex = Math.min(
    //                         range.startRow?.rowIndex,
    //                         range.endRow?.rowIndex
    //                     );

    //                     let endRowIndex = Math.max(
    //                         range.startRow?.rowIndex,
    //                         range.endRow?.rowIndex
    //                     );

    //                     clearCells(startRowIndex, endRowIndex, colIds, params.api);
    //                 }
    //             })
    //         }
    //         return false;
    //     }
    //     return true;
    // },
}

export const defaultColDefBase: ColDef = {
    editable: true,
    suppressMenu: true,
};
function clearCells(start: number, end: number, columns: any[], gridApi: GridApi) {
    let itemsToUpdate = [];

    for (let i = start; i <= end; i++) {

        let data = (gridApi as any).rowModel.rowsToDisplay[i].data;
        columns.forEach((column: string | number) => {
            data[column] = "";
        });
        data['rowStatus'] = RowStatus.Update;
        itemsToUpdate.push(data);
    }

    gridApi.applyTransaction({ update: itemsToUpdate });
}
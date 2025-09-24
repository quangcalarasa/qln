import { Pipe, PipeTransform } from '@angular/core';
import { ContractStatus167 } from 'src/app/shared/utils/consts';

@Pipe({
    name: 'showContractStatus167Name',
    pure: false
})
export class ShowContractStatus167NamePipe implements PipeTransform {
    transform(key: number): any {
        return ContractStatus167[key as unknown as keyof typeof ContractStatus167];
    }

}

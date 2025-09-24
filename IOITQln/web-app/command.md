**Tạo module**

ng g ng-alain:module ten_module
vd npm run ng g ng-alain:module categories

**Tạo component vào 1 module có sẵn**

ng g ng-alain:list ten_component -m=ten_module
vd: npm run ng g ng-alain:list test -m=categories

**Tạo component con vào 1 folder**

ng g ng-alain:list ten_component_con- -m=ten_module
vd: npm run ng g ng-alain:list components/add-or-update -m=categories

**Tạo modal vào 1 module có sẵn**

ng g ng-alain:view ten_component_con- -m=ten_module -t=ten_component_cha
vd: npm run ng g ng-alain:view add-test -m=categories -t=test

**Tạo modal vào 1 folder**

ng g ng-alain:view ten_component_con- -m=ten_module -t=ten_component_cha
vd: npm run ng g ng-alain:view employee/components/add-or-update -m=categories -t=employee
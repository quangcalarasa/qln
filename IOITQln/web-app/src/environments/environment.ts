// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

import { DelonMockModule } from '@delon/mock';
import { Environment } from '@delon/theme';

import * as MOCKDATA from '../../_mock';

export const environment = {
  production: false,
  useHash: false,
  api: {
    baseUrl: './',
    refreshTokenEnabled: true,
    refreshTokenType: 'auth-refresh'
  },
  keycloak: {
    // url: 'https://fixqln.digipro.com.vn/auth',?
    url: 'https://keycloak.hmcic.vn/auth',
    realm: 'qln',
    clientId: 'client_local'
  },
  linkCe: 'http://shqthcm.cnttvietnam.com.vn/',
  firebase: {
    apiKey: "AIzaSyDigbpq059nmCkuzvlUuWdgpi2OSTezbpg",
    authDomain: "qln-hcm.firebaseapp.com",
    projectId: "qln-hcm",
    storageBucket: "qln-hcm.appspot.com",
    messagingSenderId: "105442962473",
    appId: "1:105442962473:web:204b16daa568b2b4f80038",
    measurementId: "G-6C9G6034TN",
    vapidKey: "BKiJn7qT8uQBr8YbgeKK5etQ976OCQNmZ0dxr0ndqJv0R0Msi1jpxiwFZ-tultujYXjpFRqYPpohtnPJdB20Gr4"
  },
  modules: [DelonMockModule.forRoot({ data: MOCKDATA })]
} as Environment;

/*
 * In development mode, to ignore zone related error stack frames such as
 * `zone.run`, `zoneDelegate.invokeTask` for easier debugging, you can
 * import the following file, but please comment it out in production mode
 * because it will have performance impact when throw error
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.

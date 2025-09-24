import { Environment } from '@delon/theme';

export const environment = {
  production: true,
  useHash: false,
  api: {
    baseUrl: './',
    refreshTokenEnabled: true,
    refreshTokenType: 'auth-refresh'
  },
  keycloak: {
    url: 'https://keycloak.hmcic.vn/auth',
    // url: 'https://fixqln.digipro.com.vn/auth',?
    realm: 'qln',
    clientId: 'client_qln_prod'
  },
  linkCe: 'https://quytrinh.hmcic.vn/',
  firebase: {
    apiKey: "AIzaSyDigbpq059nmCkuzvlUuWdgpi2OSTezbpg",
    authDomain: "qln-hcm.firebaseapp.com",
    projectId: "qln-hcm",
    storageBucket: "qln-hcm.appspot.com",
    messagingSenderId: "105442962473",
    appId: "1:105442962473:web:204b16daa568b2b4f80038",
    measurementId: "G-6C9G6034TN",
    vapidKey: "BKiJn7qT8uQBr8YbgeKK5etQ976OCQNmZ0dxr0ndqJv0R0Msi1jpxiwFZ-tultujYXjpFRqYPpohtnPJdB20Gr4"
  }
} as Environment;


// import { Environment } from '@delon/theme';

// export const environment = {
//   production: true,
//   useHash: false,
//   api: {
//     baseUrl: './',
//     refreshTokenEnabled: true,
//     refreshTokenType: 'auth-refresh'
//   },
//   keycloak: {
//     url: 'https://keycloak.cnttvietnam.com.vn/auth',
//     realm: 'qln',
//     clientId: 'client_qln'
//   },
//   linkCe: 'http://shqthcm.cnttvietnam.com.vn/',
//   firebase: {
//     apiKey: "AIzaSyDigbpq059nmCkuzvlUuWdgpi2OSTezbpg",
//     authDomain: "qln-hcm.firebaseapp.com",
//     projectId: "qln-hcm",
//     storageBucket: "qln-hcm.appspot.com",
//     messagingSenderId: "105442962473",
//     appId: "1:105442962473:web:204b16daa568b2b4f80038",
//     measurementId: "G-6C9G6034TN",
//     vapidKey: "BKiJn7qT8uQBr8YbgeKK5etQ976OCQNmZ0dxr0ndqJv0R0Msi1jpxiwFZ-tultujYXjpFRqYPpohtnPJdB20Gr4"
//   }
// } as Environment;

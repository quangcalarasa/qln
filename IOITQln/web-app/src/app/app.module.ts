/* eslint-disable import/order */
/* eslint-disable import/no-duplicates */
import { HttpClientModule } from '@angular/common/http';
import { APP_INITIALIZER, LOCALE_ID, NgModule, Type } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { SimpleInterceptor } from '@delon/auth';
import { NzNotificationModule } from 'ng-zorro-antd/notification';
import { APP_BASE_HREF } from '@angular/common';

// #region default language
// 参考：https://ng-alain.com/docs/i18n
import { I18NService } from '@core';
import { default as ngLang } from '@angular/common/locales/vi';
import { ALAIN_I18N_TOKEN, DELON_LOCALE, en_US as delonLang } from '@delon/theme';
import localeVI from '@angular/common/locales/vi';
import { vi as dateLang, zhCN } from 'date-fns/locale';
import { vi_VN as zorroLang, NZ_DATE_LOCALE, NZ_I18N } from 'ng-zorro-antd/i18n';

const LANG = {
  abbr: 'vi',
  ng: ngLang,
  zorro: zorroLang,
  date: dateLang,
  delon: delonLang
};
// register angular
import { registerLocaleData } from '@angular/common';
import localeDe from '@angular/common/locales/de';
registerLocaleData(LANG.ng, LANG.abbr);
registerLocaleData(localeVI);
registerLocaleData(localeDe);

const LANG_PROVIDES = [
  // { provide: LOCALE_ID, useValue: LANG.abbr },
  { provide: NZ_I18N, useValue: LANG.zorro },
  { provide: NZ_DATE_LOCALE, useValue: LANG.date },
  { provide: DELON_LOCALE, useValue: LANG.delon }
];
// #endregion

// #region i18n services

const I18NSERVICE_PROVIDES = [{ provide: ALAIN_I18N_TOKEN, useClass: I18NService, multi: false, userValue: "de-DE" }];

// #endregion

// #region global third module

import { BidiModule } from '@angular/cdk/bidi';
const GLOBAL_THIRD_MODULES: Array<Type<any>> = [BidiModule];

// #endregion

// #region JSON Schema form (using @delon/form)
import { JsonSchemaModule } from '@shared';
const FORM_MODULES = [JsonSchemaModule];
// #endregion

// #region Http Interceptors
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { DefaultInterceptor } from '@core';

const INTERCEPTOR_PROVIDES = [
  { provide: HTTP_INTERCEPTORS, useClass: SimpleInterceptor, multi: true },
  { provide: HTTP_INTERCEPTORS, useClass: DefaultInterceptor, multi: true }
];
// #endregion

// #region Startup Service
import { StartupService } from '@core';
export function StartupServiceFactory(startupService: StartupService): () => Observable<void> {
  return () => startupService.load();
}
const APPINIT_PROVIDES = [
  StartupService,
  {
    provide: APP_INITIALIZER,
    useFactory: StartupServiceFactory,
    deps: [StartupService],
    multi: true
  }
];
// #endregion

import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { GlobalConfigModule } from './global-config.module';
import { LayoutModule } from './layout/layout.module';
import { RoutesModule } from './routes/routes.module';
import { SharedModule } from './shared/shared.module';
import { STWidgetModule } from './shared/st-widget/st-widget.module';
import { Observable } from 'rxjs';
import { KeycloakAngularModule, KeycloakService } from 'keycloak-angular';
import { environment } from '../environments/environment';

function initializeKeycloak(keycloak: KeycloakService) {
  return () => {
    keycloak.init({
      config: {
        url: environment['keycloak'].url,
        realm: environment['keycloak'].realm,
        clientId: environment['keycloak'].clientId
      },
      initOptions: {
        onLoad: 'check-sso',
        checkLoginIframe: false,
        responseMode: 'query'
      },
      enableBearerInterceptor: true,
      bearerExcludedUrls: ['/'],
      loadUserProfileAtStartUp: true
    });
  };
}

import { AlainConfig, ALAIN_CONFIG } from '@delon/util/config';
const alainConfig: AlainConfig = {
  st: {
    safeType: "text"
  }
};

import { initializeApp } from "firebase/app";
initializeApp(environment['firebase']);

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    GlobalConfigModule.forRoot(),
    CoreModule,
    SharedModule,
    LayoutModule,
    RoutesModule,
    STWidgetModule,
    NzNotificationModule,
    ...GLOBAL_THIRD_MODULES,
    ...FORM_MODULES,
    KeycloakAngularModule
  ],
  // providers: [...LANG_PROVIDES, ...INTERCEPTOR_PROVIDES, ...I18NSERVICE_PROVIDES, ...APPINIT_PROVIDES],
  providers: [
    ...LANG_PROVIDES,
    ...INTERCEPTOR_PROVIDES,
    ...I18NSERVICE_PROVIDES,
    ...APPINIT_PROVIDES,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeKeycloak,
      multi: true,
      deps: [KeycloakService]
    },
    { provide: ALAIN_CONFIG, useValue: alainConfig }       //Cấu hình dạng dữ liệu của St table: text,html,safeHtml
  ],
  // providers: [...INTERCEPTOR_PROVIDES, ...I18NSERVICE_PROVIDES, ...APPINIT_PROVIDES],
  bootstrap: [AppComponent]
})
export class AppModule { }

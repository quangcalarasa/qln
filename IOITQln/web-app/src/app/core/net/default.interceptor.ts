import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpHeaders,
  HttpInterceptor,
  HttpRequest,
  HttpResponseBase
} from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { ALAIN_I18N_TOKEN, _HttpClient } from '@delon/theme';
import { environment } from '@env/environment';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { BehaviorSubject, from, Observable, of, throwError } from 'rxjs';
import { catchError, filter, mergeMap, switchMap, take } from 'rxjs/operators';
import { UserService } from '../services/user.service';

const CODEMESSAGE: { [key: number]: string } = {
  200: 'The server successfully returned the requested data. ',
  201: 'Create or modify data successfully. ',
  202: 'A request has entered the background queue (asynchronous task). ',
  204: 'Delete data successfully. ',
  400: 'There was an error in the request sent, and the server did not create or modify data. ',
  401: 'Bạn không có quyền truy cập chức năng này',
  403: 'Từ chối truy cập.',
  404: 'Không tìm thấy trang.',
  406: 'Lỗi định dạng truy vấn',
  410: 'The requested resource has been permanently deleted and will no longer be available. ',
  422: 'When creating an object, a validation error occurred. ',
  500: 'Đã có lỗi xảy ra trên server',
  502: 'Đã có lỗi xảy ra trên server',
  503: 'Đã có lỗi xảy ra trên server',
  504: 'Thời gian chờ phản hồi quá lâu',
};

/**
 * 默认HTTP拦截器，其注册细节见 `app.module.ts`
 */
@Injectable()
export class DefaultInterceptor implements HttpInterceptor {
  private refreshTokenEnabled = environment.api.refreshTokenEnabled;
  private refreshTokenType?: 're-request' | 'auth-refresh';// = environment.api.refreshTokenType;
  private refreshToking = false;
  private refreshToken$: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private injector: Injector, private userService: UserService, private router: Router) {
    const tokenModel: any = this.tokenSrv.get() || {};
    const { expired } = tokenModel;

    if (expired && Number(expired) < +new Date() + 1000 * 60 * 1 * 23) {
      this.refreshTokenType = 'auth-refresh';
    }

    if (this.refreshTokenType === 'auth-refresh') {
      this.buildAuthRefresh();
    }
  }

  private get notification(): NzNotificationService {
    return this.injector.get(NzNotificationService);
  }

  private get tokenSrv(): ITokenService {
    return this.injector.get(DA_SERVICE_TOKEN);
  }

  private get http(): _HttpClient {
    return this.injector.get(_HttpClient);
  }

  private goTo(url: string): void {
    // setTimeout(() => this.injector.get(Router).navigateByUrl(url));
    setTimeout(() => window.location.href = url);
  }

  private checkStatus(ev: HttpResponseBase): void {
    const { body } = ev as any;
    const { meta } = body || {};


    if (((ev.status >= 200 && ev.status < 300) && (meta ? meta.error_code == 200 : true)) || ev.status === 401) {
      return;
    }

    const errortext = meta?.error_message || CODEMESSAGE[ev.status] || ev.statusText;
    this.notification.error(`Thông báo lỗi: `, errortext);
  }

  /**
   * Refresh Token request
   */
  private refreshTokenRequest(): Observable<any> {
    // const model = this.tokenSrv.get();
    // return this.http.post(`/api/auth/refresh`, null, null, { headers: { refresh_token: model?.['refresh_token'] || '' } });
    return from(this.userService.refreshTokenRequest());
  }

  // #region Refresh Token Method 1: Use 401 to refresh the Token
  private tryRefreshToken(ev: HttpResponseBase, req: HttpRequest<any>, next: HttpHandler): Observable<any> {
    // 1. If the request is a refresh token request, it means that the login page can be directly jumped from the refresh token
    if ([`/api/auth/refresh`].some(url => req.url.includes(url))) {
      this.toLogin();
      return throwError(ev);
    }
    // 2. If `refreshToking` is `true`, it means that it is already in the request to refresh the Token, all subsequent requests will enter the waiting state, and then re-initiate the request until the result is returned    
    if (this.refreshToking) {
      return this.refreshToken$.pipe(
        filter(v => !!v),
        take(1),
        switchMap(() => next.handle(this.reAttachToken(req)))
      );
    }
    // 3. Try to call refresh Token
    this.refreshToking = true;
    this.refreshToken$.next(null);

    return this.refreshTokenRequest().pipe(
      switchMap(res => {
        // Notify subsequent requests to proceed
        this.refreshToking = false;
        this.refreshToken$.next(res);
        // resave new token
        this.tokenSrv.set(res);
        // restart the request
        return next.handle(this.reAttachToken(req));
      }),
      catchError(err => {
        this.refreshToking = false;

        this.toLogin();
        return throwError(err);
      })
    );
  }

  /**
     * Re-attach new Token information
     *
     * Due to the request that has already been initiated, it will not go through `@delon/auth` again, so you need to re-attach a new Token according to the business situation
     */
  private reAttachToken(req: HttpRequest<any>): HttpRequest<any> {
    // The following example uses `SimpleInterceptor` by default for NG-ALAIN     
    const token = this.tokenSrv.get()?.token;
    return req.clone({
      setHeaders: {
        token: `Bearer ${token}`
      }
    });
  }

  // #endregion

  // #region 刷新Token方式二：使用 `@delon/auth` 的 `refresh` 接口

  private buildAuthRefresh(): void {
    if (!this.refreshTokenEnabled) {
      return;
    }
    this.tokenSrv.refresh
      .pipe(
        filter(() => !this.refreshToking),
        switchMap(res => {
          this.refreshToking = true;
          let resp = this.refreshTokenRequest();
          return resp;
        })
      )
      .subscribe(
        res => {
          // TODO: Mock expired value
          res.expired = +new Date() + 1000 * 60 * 59 * 23;
          this.refreshToking = false;
          this.tokenSrv.set(res);
        },
        (error) => { console.log(error); this.toLogin(); }
      );
  }

  // #endregion

  private toLogin(): void {
    // this.notification.error(`Not logged in or the login has expired, please log in again.`, ``);
    this.goTo(this.tokenSrv.login_url! + `?url=${this.router.url}`);
  }

  private handleData(ev: HttpResponseBase, req: HttpRequest<any>, next: HttpHandler): Observable<any> {
    this.checkStatus(ev);
    // 业务处理：一些通用操作
    switch (ev.status) {
      case 200:
        // 业务层级错误处理，以下是假定restful有一套统一输出格式（指不管成功与否都有相应的数据格式）情况下进行处理
        // 例如响应内容：
        //  错误内容：{ status: 1, msg: '非法参数' }
        //  正确内容：{ status: 0, response: {  } }
        // 则以下代码片断可直接适用
        // if (ev instanceof HttpResponse) {
        //   const body = ev.body;
        //   if (body && body.status !== 0) {
        //     this.injector.get(NzMessageService).error(body.msg);
        //     // 注意：这里如果继续抛出错误会被行254的 catchError 二次拦截，导致外部实现的 Pipe、subscribe 操作被中断，例如：this.http.get('/').subscribe() 不会触发
        //     // 如果你希望外部实现，需要手动移除行254
        //     return throwError({});
        //   } else {
        //     // 忽略 Blob 文件体
        //     if (ev.body instanceof Blob) {
        //        return of(ev);
        //     }
        //     // 重新修改 `body` 内容为 `response` 内容，对于绝大多数场景已经无须再关心业务状态码
        //     return of(new HttpResponse(Object.assign(ev, { body: body.response })));
        //     // 或者依然保持完整的格式
        //     return of(ev);
        //   }
        // }
        break;
      case 401:
        console.log('toLogin');
        if (this.refreshTokenEnabled && this.refreshTokenType === 're-request') {
          return this.tryRefreshToken(ev, req, next);
        }
        this.toLogin();
        break;
      case 403:
      case 404:
      case 500:
        // this.goTo(`/exception/${ev.status}?url=${req.urlWithParams}`);
        break;
      default:
        if (ev instanceof HttpErrorResponse) {
          console.warn(
            'Unknown errors are mostly caused by the backend not supporting cross-domain CORS or invalid configuration. Please refer to https://ng-alain.com/docs/server to solve cross-domain problems',
            ev
          );
        }
        break;
    }
    if (ev instanceof HttpErrorResponse) {
      return throwError(ev);
    } else {
      return of(ev);
    }
  }

  private getAdditionalHeaders(headers?: HttpHeaders): { [name: string]: string } {
    const res: { [name: string]: string } = {};
    const lang = this.injector.get(ALAIN_I18N_TOKEN).currentLang;
    if (!headers?.has('Accept-Language') && lang) {
      res['Accept-Language'] = lang;
    }

    return res;
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // 统一加上服务端前缀
    let url = req.url;
    if (!url.startsWith('https://') && !url.startsWith('http://')) {
      const { baseUrl } = environment.api;
      url = baseUrl + (baseUrl.endsWith('/') && url.startsWith('/') ? url.substring(1) : url);
    }

    const newReq = req.clone({ url, setHeaders: this.getAdditionalHeaders(req.headers) });
    return next.handle(newReq).pipe(
      mergeMap(ev => {
        // 允许统一对请求错误处理
        if (ev instanceof HttpResponseBase) {
          return this.handleData(ev, newReq, next);
        }
        // 若一切都正常，则后续操作
        return of(ev);
      }),
      catchError((err: HttpErrorResponse) => this.handleData(err, newReq, next))
    );
  }
}

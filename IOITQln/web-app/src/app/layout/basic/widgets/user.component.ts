import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { SettingsService } from '@delon/theme';
import { KeycloakService } from 'keycloak-angular';
import { UserRepository } from 'src/app/infrastructure/repositories/user.repository';

@Component({
  selector: 'header-user',
  template: `
    <div class="alain-default__nav-item d-flex align-items-center px-sm" nz-dropdown nzPlacement="bottomRight" [nzDropdownMenu]="userMenu">
      <nz-avatar
        nzIcon="user"
        [nzSrc]="user.BaseUrlImg + user.Avatar"
        style="background-color:#87d068;"
        nzSize="small"
        class="mr-sm"
      ></nz-avatar>
      {{ user.FullName || user.username || user.userName | titlecase }}
    </div>
    <nz-dropdown-menu #userMenu="nzDropdownMenu">
      <div nz-menu class="width-sm">
        <div nz-menu-item routerLink="/account/changeinfo">
          <i nz-icon nzType="user" class="mr-sm"></i>
          Tài khoản
        </div>
        <div nz-menu-item routerLink="/account/changepass">
          <i nz-icon nzType="lock" class="mr-sm"></i>
          Đổi mật khẩu
        </div>
        <li nz-menu-divider></li>
        <div nz-menu-item (click)="logoutAll()">
          <i nz-icon nzType="logout" class="mr-sm"></i>
          Đăng xuất
        </div>
      </div>
    </nz-dropdown-menu>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderUserComponent {
  get user(): any {
    return this.settings.user;
  }

  constructor(private settings: SettingsService, private router: Router, @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService, private keycloakService: KeycloakService, private userRepository: UserRepository) { }

  logout(): void {
    this.userRepository.logout();
    this.tokenService.clear();
    localStorage.removeItem('user');
    this.settings.setUser({});
    // this.router.navigateByUrl(this.tokenService.login_url!);
    this.router.navigateByUrl('/login');
  }

  logoutAll(): void {
    this.userRepository.logout();
    this.tokenService.clear();
    localStorage.removeItem('user');
    this.settings.setUser({});
    this.keycloakService.logout();
    // this.router.navigateByUrl(this.tokenService.login_url!);
  }
}

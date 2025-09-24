import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from 'src/app/core/services/user.service';
import { KeycloakService, KeycloakEventType } from 'keycloak-angular';
import { async } from 'rxjs';
import { NzNotificationService } from 'ng-zorro-antd/notification';

@Component({
  templateUrl: './login.component.html',
  styleUrls: [`login.compnent.less`]
})
export class LoginComponent implements OnInit {
  validateForm!: FormGroup;
  processing: boolean = false;
  isLoginViaSso = localStorage.getItem("isLoginViaSso");

  constructor(private fb: FormBuilder, private router: Router, private userService: UserService, private route: ActivatedRoute, private keycloakService: KeycloakService, private nzNotificationService: NzNotificationService) { }

  ngOnInit(): void {
    this.validateForm = this.fb.group({
      username: [null, [Validators.required, Validators.maxLength(30)]],
      password: [null, [Validators.required, Validators.maxLength(50)]],
      remember: [true]
    });

    localStorage.removeItem('isLoginViaSso');
    if (this.isLoginViaSso !== null) {
      setTimeout(() => {
        this.keycloakService.isLoggedIn().then(logged => {
          if (logged) {
            this.keycloakService.loadUserProfile().then(user => {
              this.keycloakService.getToken().then(async token => {
                let input = { username: user.username, token: token };
                const resp = await this.userService.login(input, true);

                if (resp?.code === 0) {
                  let first_url = resp?.url;
                  if (first_url == '') {
                    this.nzNotificationService.error('Đăng nhập không thành công', 'Tài khoản chưa được phân quyền, Vui lòng liên hệ với quản trị viên!');
                    this.userService.logout();
                  }
                  else {
                    let url = this.route.snapshot.queryParams['url'];
                    if (url) {
                      let spl = url.split("?");
                      this.router.navigateByUrl(spl[spl.length - 1].replace("url=", ""));
                    }
                    else {
                      this.router.navigateByUrl(first_url);
                    }
                    // if (resp?.subSystem) {
                    //   let url = this.route.snapshot.queryParams['url'];
                    //   if (url) {
                    //     let spl = url.split("?");
                    //     this.router.navigateByUrl(spl[spl.length - 1].replace("url=", ""));
                    //   }
                    //   else {
                    //     this.router.navigateByUrl(url);
                    //   }
                    // }
                    // else {
                    //   this.router.navigateByUrl('/subsystem');
                    // }
                  }
                }
                else {
                  this.keycloakService.logout();
                }
              })
            });
          }
        });
      }, 100);
    }
    else {
      this.loginViaSso();
    }
  }

  async submitForm() {
    try {
      this.processing = true;
      for (const i in this.validateForm.controls) {
        this.validateForm.controls[i].markAsDirty();
        this.validateForm.controls[i].updateValueAndValidity();
      }

      if (this.validateForm.valid) {
        const resp = await this.userService.login(this.validateForm.value, false);

        if (resp?.code === 0) {
          // if (resp?.subSystem) {
          //   let url = this.route.snapshot.queryParams['url'];
          //   if (url) {
          //     let spl = url.split("?");
          //     this.router.navigateByUrl(spl[spl.length - 1].replace("url=", ""));
          //   }
          //   else {
          //     this.router.navigateByUrl('/');
          //   }
          // }
          // else {
          //   this.router.navigateByUrl('/subsystem');
          // }

          let url = this.route.snapshot.queryParams['url'];
          if (url) {
            let spl = url.split("?");
            this.router.navigateByUrl(spl[spl.length - 1].replace("url=", ""));
          }
          else {
            this.router.navigateByUrl('/');
          }
        }
      }
    } catch (error) {
      console.log(error);
    }
    this.processing = false;
  }

  loginViaSso() {
    //set 1 biến localStorage để đánh dấu là login bằng SSO
    //call đến hàm login của keycloakService, nếu login rồi thì nó tự redirect về còn k thì nhập username password để nó redirect về
    localStorage.setItem("isLoginViaSso", "1");
    this.keycloakService.login({
      redirectUri: window.location.origin + this.router.url
    });
  }
}


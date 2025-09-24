import { Component, OnInit, ViewChild } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SettingsService, User } from '@delon/theme';
import { UserService } from 'src/app/core/services/user.service';

@Component({
  templateUrl: './subsystem.component.html',
  styleUrls: [`subsystem.compnent.less`]
})
export class SubSystemComponent implements OnInit {
  constructor(private settings: SettingsService, private router: Router, private userService: UserService) { }

  ngOnInit(): void {
  }

  chooseSubSystem(subSystem: number) {
    let user = this.settings.user;
    this.settings.setUser({ ...user, subSystem });

    this.router.navigateByUrl(this.userService.getFirstRoute(this.settings.user["listMenus"]));
  }
}

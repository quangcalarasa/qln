import { NgModule, Type } from '@angular/core';
import { SharedModule } from '@shared';
import { ExtraInfoRoutingModule } from './extra-info-routing.module';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzTreeSelectModule } from 'ng-zorro-antd/tree-select';
import { NzUploadModule } from 'ng-zorro-antd/upload';
import { NewsArticleComponent } from './News/News-article/News-article.component';
import { NewsArticleListComponent } from './News/News-article-list/News-article-list.component';
import { PostingConfiguration } from './News/Posting-configuration/PostingConfiguration.component';
import { AddOrUpdateNewsArticleComponent } from './News/News-article/add-or-update/add-or-update-NewsArticle.component';
import { AddOrUpdateNewsArticleListComponent } from './News/News-article-list/add-or-update/add-or-update-NewsArticleList.component';

import { EditContactManageComponent } from './Contact-Management/edit/edit-contact-management.component';
import { ContactManageComponent } from './Contact-Management/ContactManageComponent';
import { SupportComponent } from './Contact/Support.component';
import { ExtraSupportRequestsComponent } from './support/extra-support-requests/extra-support-requests.component';
import { HandleSupportRequestsComponent } from './support/handle-support-requests/handle-support-requests.component';
import { AddOrUpdateComponent } from './support/extra-support-requests/add-or-update/add-or-update.component';
import { AddOrUpdateHandleComponent } from './support/handle-support-requests/add-or-update-handle/add-or-update-handle.component';
import { EmailDebtComponent } from './email/email-debt/email-debt.component';
import { EmailNotificationComponent } from './email/email-notification/email-notification.component';
import { EmailSentComponent } from './email/email-sent/email-sent.component';
import { EmailTemplateComponent } from './email/email-template/email-template.component';
import { AddOrUpdateEmaiDebtComponent } from './email/email-debt/add-or-update/add-or-update.component';
import { AddOrUpdateTemplateComponent } from './email/email-template/add-or-update/add-or-update.component';
import { DebtInfoComponent } from 'src/app/routes/extra-info/debt-info/debt-info.component';
import { AddOrUpdateEmailSentComponent } from './email/email-sent/add-or-update/add-or-update.component'; 

import { ReportDeadlineComponent } from './extra-info-report/report-deadline/report-deadline.component';
import { ReportNewsComponent } from './extra-info-report/report-news/report-news.component';
import { ReportOutdateComponent } from './extra-info-report/report-outdate/report-outdate.component';
import { ReportSupportComponent } from './extra-info-report/report-support/report-support.component';
import { ConfigDebtComponent } from './debt-info/config-debt/config-debt.component';
import { DebtNotificationComponent } from './debt-info/debt-notification/debt-notification.component';
import { DebtReminderComponent } from './debt-info/debt-reminder/debt-reminder.component';
import { NotificationSentComponent } from './debt-info/notification-sent/notification-sent.component';
import { OverdueDebtComponent } from './debt-info/overdue-debt/overdue-debt.component';
import { ExtraConfigNotiComponent } from './debt-info/config-noti/config.component';
import { UsageHistoryComponent } from './debt-info/usage-history/usage-history.component';
import { AddUsageHistoryComponent } from './debt-info/usage-history/add-usage-history/add-usage-history.component';
import { AddOverdueDebtComponent } from './debt-info/overdue-debt/add-overdue-debt/add-overdue-debt.component';
import { AddNotificationSentComponent } from './debt-info/notification-sent/add-notification-sent/add-notification-sent.component';
import { AddDebtReminderComponent } from './debt-info/debt-reminder/add-debt-reminder/add-debt-reminder.component';
import { AddDebtNotificationComponent } from './debt-info/debt-notification/add-debt-notification/add-debt-notification.component';

const COMPONENTS: Array<Type<void>> = [
  ExtraSupportRequestsComponent,
  HandleSupportRequestsComponent,
  DebtInfoComponent,
  EmailDebtComponent,
  EmailNotificationComponent,
  EmailSentComponent,
  EmailTemplateComponent,
  AddOrUpdateEmaiDebtComponent,
  AddOrUpdateTemplateComponent,
  AddOrUpdateEmailSentComponent,
  AddOrUpdateComponent,
  AddOrUpdateHandleComponent,
  ReportDeadlineComponent,
  ReportNewsComponent,
  ReportOutdateComponent,
  ReportSupportComponent,
  NewsArticleComponent,
  NewsArticleListComponent,
  PostingConfiguration,
  AddOrUpdateNewsArticleComponent,
  AddOrUpdateNewsArticleListComponent,
  EditContactManageComponent,
  ContactManageComponent,
  SupportComponent,
  ConfigDebtComponent,
  DebtNotificationComponent,
  DebtReminderComponent,
  NotificationSentComponent,
  OverdueDebtComponent,
  ExtraConfigNotiComponent,
  UsageHistoryComponent,
  AddUsageHistoryComponent,
  AddOverdueDebtComponent,
  AddNotificationSentComponent,
  AddDebtReminderComponent,
  AddDebtNotificationComponent
];

@NgModule({
  imports: [SharedModule, ExtraInfoRoutingModule, NzPageHeaderModule, NzTreeSelectModule, NzUploadModule],
  declarations: COMPONENTS
})
export class ExtraInfoModule { }

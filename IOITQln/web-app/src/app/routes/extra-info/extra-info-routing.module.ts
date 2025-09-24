import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExtraSupportRequestsComponent } from './support/extra-support-requests/extra-support-requests.component';
import { HandleSupportRequestsComponent } from './support/handle-support-requests/handle-support-requests.component';
import { EmailDebtComponent } from './email/email-debt/email-debt.component';
import { EmailNotificationComponent } from './email/email-notification/email-notification.component';
import { EmailSentComponent } from './email/email-sent/email-sent.component';
import { EmailTemplateComponent } from './email/email-template/email-template.component';

import { ReportSupportComponent } from './extra-info-report/report-support/report-support.component';
import { ReportDeadlineComponent } from './extra-info-report/report-deadline/report-deadline.component';
import { ReportNewsComponent } from './extra-info-report/report-news/report-news.component';
import { ReportOutdateComponent } from './extra-info-report/report-outdate/report-outdate.component';

import { DebtInfoComponent } from 'src/app/routes/extra-info/debt-info/debt-info.component';
import { NewsArticleComponent } from './News/News-article/News-article.component';
import { NewsArticleListComponent } from './News/News-article-list/News-article-list.component';
import { PostingConfiguration } from './News/Posting-configuration/PostingConfiguration.component';
import { UsageHistoryComponent } from './debt-info/usage-history/usage-history.component';
import { DebtNotificationComponent } from './debt-info/debt-notification/debt-notification.component';
import { ConfigDebtComponent } from './debt-info/config-debt/config-debt.component';
import { DebtReminderComponent } from './debt-info/debt-reminder/debt-reminder.component';
import { NotificationSentComponent } from './debt-info/notification-sent/notification-sent.component';
import { OverdueDebtComponent } from './debt-info/overdue-debt/overdue-debt.component';
import { ExtraConfigNotiComponent } from './debt-info/config-noti/config.component';

const routes: Routes = [
  { path: 'debt/track-house', component: UsageHistoryComponent, data: { title: 'Lịch sử, tình trạng sử dụng căn nhà' } },
  { path: 'debt/notification', component: DebtNotificationComponent, data: { title: 'Thông báo công nợ' } },
  { path: 'debt/setting-notification', component: ExtraConfigNotiComponent, data: { title: 'Cấu hình gửi thông báo công nợ' } },
  { path: 'debt/reminder', component: DebtReminderComponent, data: { title: 'Quản lý nhắc nợ' } },
  { path: 'debt/view-notification', component: NotificationSentComponent, data: { title: 'Xem thông báo đã gửi' } },
  { path: 'debt/overdue', component: OverdueDebtComponent, data: { title: 'Quản lý nợ quá hạn' } },
  { path: 'debt/setting-reminder', component: ConfigDebtComponent, data: { title: 'Cấu hình nhắc nợ' } },
  { path: 'email/email-debt', component: EmailDebtComponent, data: { title: 'Gửi Email nhắc nợ tự động' } },
  { path: 'email/email-sent', component: EmailSentComponent, data: { title: 'Xem Email đã gửi' } },
  { path: 'email/email-template', component: EmailTemplateComponent, data: { title: 'Quản lý template Email nhắc nợ' } },
  { path: 'email/email-notification', component: EmailNotificationComponent, data: { title: 'Quản lý tài khoản Email gửi thông báo' } },

  { path: 'require/support-requests', component: ExtraSupportRequestsComponent, data: { title: 'Quản lý yêu cầu hỗ trợ' } },
  { path: 'require/handle-support-requests', component: HandleSupportRequestsComponent, data: { title: 'Quản lý xử lý yêu cầu hỗ trợ' } },
  { path: 'report/report-support', component: ReportSupportComponent, data: { title: 'Báo cáo yêu cầu hỗ trợ' } },
  { path: 'report/report-news', component: ReportNewsComponent, data: { title: 'Báo cáo tin bài' } },
  { path: 'report/report-deadline', component: ReportDeadlineComponent , data: { title: 'Báo cáo hợp đồng sắp đến hạn nộp' } },
  { path: 'report/report-outdate', component: ReportOutdateComponent , data: { title: 'Báo cáo hợp đồng nợ quá hạn' } },
  { path: 'news/News-article', component: NewsArticleComponent, data: { title: 'Quản lý tin bài' } },
  { path: 'news/News-article-list', component: NewsArticleListComponent, data: { title: 'Quản lý danh mục tin bài' } },
  { path: 'news/Posting-Configuration', component: PostingConfiguration, data: { title: 'Cấu hình đăng tin' } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExtraInfoRoutingModule { }

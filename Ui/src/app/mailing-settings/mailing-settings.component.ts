import { Component, OnInit } from '@angular/core';
import { MailingSettings, ScheduleType } from '../models/mailing-settings';
import { ApiService } from '../services/api.service';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-mailing-settings',
  templateUrl: './mailing-settings.component.html',
  styleUrls: ['./mailing-settings.component.css']
})
export class MailingSettingsComponent implements OnInit {
  private readonly NUMBER_OF_ARTICLES_DEFAULT: number = 5;
  private readonly SCHEDULE_TYPE_DEFAULT: ScheduleType = ScheduleType.EveryWeek;

  public NumberOfArticles: number = this.NUMBER_OF_ARTICLES_DEFAULT;
  public Schedule: ScheduleType = this.SCHEDULE_TYPE_DEFAULT;
  public ScheduleValues: { key: number, value: string; }[];

  constructor(private api: ApiService, private accountService: AccountService) {
    this.ScheduleValues = [
      { key: 2, value: "Every Day" },
      { key: 3, value: "Every Week" },
      { key: 4, value: "Every Month" },
      { key: 1, value: "Never" },
    ];
  }

  async ngOnInit() {
    this.accountService.LoginStateChanged$.subscribe(async (logedin) => {
      if (logedin) {
        const settings = await this.api.GetMyMailingSettins();
        this.NumberOfArticles = settings.numberOfArticles;
        this.Schedule = settings.scheduleType;
      } else {
        this.NumberOfArticles = this.NUMBER_OF_ARTICLES_DEFAULT;
        this.Schedule = this.SCHEDULE_TYPE_DEFAULT;
      }
    });
  }

  async Save() {
    const settings = <MailingSettings>{
      numberOfArticles: this.NumberOfArticles,
      scheduleType: Number(this.Schedule)
    };

    await this.api.SetMyMailingSettins(settings);
  }

}

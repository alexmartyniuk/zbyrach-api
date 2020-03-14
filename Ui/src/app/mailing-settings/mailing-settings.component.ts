import { Component, OnInit } from '@angular/core';
import { MailingSettings, ScheduleType } from '../models/mailing-settings';
import { ApiService } from '../services/api.service';

@Component({
  selector: 'app-mailing-settings',
  templateUrl: './mailing-settings.component.html',
  styleUrls: ['./mailing-settings.component.css']
})
export class MailingSettingsComponent implements OnInit {

  public NumberOfArticles: number = 5;
  public Schedule: ScheduleType = ScheduleType.EveryWeek;
  public ScheduleValues: { key:number, value: string; }[];

  constructor(private api: ApiService) {
    this.ScheduleValues = [
      { key: 1, value: "Every Day"},
      { key: 2, value: "Every Week"},
      { key: 3, value: "Every Month"},
    ];
   }

  async ngOnInit() {
    const settings = await this.api.GetMyMailingSettins();
    this.NumberOfArticles = settings.numberOfArticles;
    this.Schedule = settings.scheduleType;
  }

  async Save() {
    const settings = <MailingSettings> {
      numberOfArticles: this.NumberOfArticles,
      scheduleType: Number(this.Schedule)
    };

    console.log(JSON.stringify(settings));

    await this.api.SetMyMailingSettins(settings);
  }

}

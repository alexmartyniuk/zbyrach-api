export class MailingSettings {
    scheduleType: ScheduleType;
    numberOfArticles: number;
}

export enum ScheduleType
{
    Undefined = 0,
    EveryDay = 1,
    EveryWeek = 2,
    EveryMonth = 3
}
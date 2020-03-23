export class MailingSettings {
    scheduleType: ScheduleType;
    numberOfArticles: number;
}

export enum ScheduleType
{
    Undefined = 0,
    Never = 1,
    EveryDay = 2,
    EveryWeek = 3,
    EveryMonth = 4
}
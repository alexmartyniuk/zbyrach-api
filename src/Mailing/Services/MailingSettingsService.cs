using System.Threading.Tasks;
using MediumGrabber.Api.Account;
using MediumGrabber.Api.Migrations;
using Microsoft.EntityFrameworkCore;

namespace MediumGrabber.Api.Mailing
{
    public class MailingSettingsService
    {
        private readonly ApplicationContext _db;
        private readonly CronService _cronService;

        public MailingSettingsService(ApplicationContext db, CronService cronService)
        {
            _db = db;
            _cronService = cronService;
        }

        public async Task<MailingSettings> GetByUser(User user)
        {
            var settings = await _db
                .MailingSettings
                .SingleOrDefaultAsync(m => m.UserId == user.Id);

            if (settings == null)
            {
                settings = new MailingSettings
                {
                    UserId = user.Id,
                    Schedule = _cronService.ScheduleToExpression(ScheduleType.EveryWeek),
                    NumberOfArticles = 5
                };
            }

            return settings;
        }

        public async Task<bool> SetByUser(User user, MailingSettings settings)
        {
            var existingSettings = await GetByUser(user);

            if (existingSettings == null)
            {
                existingSettings = new MailingSettings
                {
                    UserId = user.Id,
                    NumberOfArticles = settings.NumberOfArticles,
                    Schedule = settings.Schedule
                };
                _db.MailingSettings.Add(existingSettings);
            }
            else
            {
                existingSettings.NumberOfArticles = settings.NumberOfArticles;
                existingSettings.Schedule = settings.Schedule;
                _db.MailingSettings.Update(existingSettings);
            }

            return await _db.SaveChangesAsync() > 0;
        }
    }
}
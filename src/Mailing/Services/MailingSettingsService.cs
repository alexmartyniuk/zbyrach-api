using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cronos;
using Zbyrach.Api.Account;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;

namespace Zbyrach.Api.Mailing
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

        public async Task<MailingSettings> Get(User user)
        {
            var settings = await _db
                .MailingSettings
                .SingleOrDefaultAsync(m => m.UserId == user.Id);

            if (settings == null)
            {
                settings = GetDefaultSettings(user);
            }

            return settings;
        }

        private MailingSettings GetDefaultSettings(User user)
        {
            return new MailingSettings
            {
                UserId = user.Id,
                Schedule = _cronService.ScheduleToExpression(ScheduleType.EveryWeek),
                NumberOfArticles = 5
            };
        }

        public async Task<bool> CreateOrUpdate(User user, MailingSettings settings)
        {
            var existingSettings = await Get(user);

            if (existingSettings == null)
            {
                existingSettings = new MailingSettings
                {
                    UserId = user.Id,
                    NumberOfArticles = settings.NumberOfArticles,
                    Schedule = settings.Schedule,
                    UpdatedAt = DateTime.UtcNow,
                };
                _db.MailingSettings.Add(existingSettings);
            }
            else
            {
                existingSettings.NumberOfArticles = settings.NumberOfArticles;
                existingSettings.Schedule = settings.Schedule;
                existingSettings.UpdatedAt = DateTime.UtcNow;
                existingSettings.LastSentAt = default;
                _db.MailingSettings.Update(existingSettings);
            }

            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<List<MailingSettings>> GetBySchedule(TimeSpan schedulePeriod)
        {
            return (await _db.MailingSettings
                .Include(m => m.User)
                .ToListAsync())
                .Where(m => IsApplicable(m, schedulePeriod))
                .ToList();
        }

        public async Task<MailingSettings> UpdateSendDate(User user)
        {
            var mailingSettingsOriginal = await Get(user);

            mailingSettingsOriginal.LastSentAt = DateTime.UtcNow;
            _db.MailingSettings.Update(mailingSettingsOriginal);

            await _db.SaveChangesAsync();

            return await Get(user);
        }

        private bool IsApplicable(MailingSettings settings, TimeSpan schedulePeriod)
        {
            var dateFrom = settings.LastSentAt != default
                ? settings.LastSentAt
                : settings.UpdatedAt;
            dateFrom = DateTime.SpecifyKind(dateFrom, DateTimeKind.Utc);

            return _cronService.HasTimeCome(dateFrom, schedulePeriod, settings.Schedule);
        }
    }
}
using System.Threading.Tasks;
using MediumGrabber.Api.Account;
using MediumGrabber.Api.Migrations;
using Microsoft.EntityFrameworkCore;

namespace MediumGrabber.Api.Mailing
{
    public class MailingSettingsService
    {
        private readonly ApplicationContext _db;

        public MailingSettingsService(ApplicationContext db)
        {
            _db = db;
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
                    Schedule = "0 0 12 * * FRI",
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
                existingSettings = new MailingSettings();
                existingSettings.UserId = user.Id;
                _db.MailingSettings.Add(existingSettings);
            }

            existingSettings.NumberOfArticles = settings.NumberOfArticles;
            existingSettings.Schedule = settings.Schedule;

            return await _db.SaveChangesAsync() > 0;                
        }
    }
}
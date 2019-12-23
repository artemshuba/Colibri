using System;
using System.Globalization;
using VkLib.Core.Users;

namespace Colibri.Helpers
{
    public static class DateFormatHelper
    {
        public static string FormatUserLastSeen(VkProfile user)
        {
            var date = user.LastSeen;

            var hours = (DateTime.Now.ToUniversalTime() - date).TotalHours;
            var days = (DateTime.Now.ToUniversalTime() - date).TotalDays;
            var result = (user.Sex == VkSex.Female ? Localizator.String("UserStatusLastSeenFemale") : Localizator.String("UserStatusLastSeenMale")) + " ";

            if (date.Date == DateTime.Now.ToUniversalTime().Date)
                result += Localizator.String("UserStatusLastSeenToday") + " " + date.ToString("t");
            else if ((DateTime.Today - date.Date) == TimeSpan.FromDays(1))
                result += Localizator.String("UserStatusLastSeenYesterday") + " " + date.ToString("t");
            else
                result += date.ToString(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ru" ? "dd.MM" : "MM/dd");

            return result;
        }
    }
}
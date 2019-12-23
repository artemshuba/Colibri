using System;
using Windows.ApplicationModel;

namespace Colibri.Helpers
{
    /// <summary>
    /// Вспомогательный класс для приложения
    /// </summary>
    public static class AppHelper
    {
        /// <summary>
        /// Получить строку с версией приложения
        /// </summary>
        /// <returns></returns>
        public static string GetAppVersionString()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        /// <summary>
        /// Получить версию приложения
        /// </summary>
        /// <returns></returns>
        public static Version GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            return new Version(packageId.Version.Major, packageId.Version.Minor, packageId.Version.Build, packageId.Version.Revision);
        }
    }
}
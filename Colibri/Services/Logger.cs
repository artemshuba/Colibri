using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using MetroLog;
using MetroLog.Targets;
using Microsoft.ApplicationInsights;
using Yandex.Metrica;

namespace Colibri.Services
{
    public static class Logger
    {
        private static readonly TelemetryClient Telemetry = new TelemetryClient();
        private static ILogger _logger;

        static Logger()
        {
#if DEBUG
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget());
            _logger = LogManagerFactory.DefaultLogManager.GetLogger<Application>();

#endif
        }

        public static void AppStart()
        {
            //may cause problems with restoring
            YandexMetrica.Activate("2a556755-72e7-450d-9bab-c3eb179ec300");

#if DEBUG
            _logger.Info("App started");
#endif
        }

        public static void AppSuspeinding()
        {
#if DEBUG
            _logger.Info("App suspending");
#endif
        }

        public static void Info(string message)
        {
            Debug.WriteLine(message);

#if DEBUG
            _logger.Info(message);
#endif
        }

        public static void Error(Exception ex, string message = null)
        {
            Debug.WriteLine(message + "\r\n" + ex);

            YandexMetrica.ReportError(message, ex);
            Telemetry.TrackException(ex);

#if DEBUG
            _logger.Error(message + ex, ex);
#endif
        }

        public static void Fatal(Exception ex, string message)
        {
            Debug.WriteLine("Fatal error: " + message + "\r\n" + ex);

            YandexMetrica.ReportUnhandledException(ex);
            Telemetry.TrackException(ex);
#if DEBUG
            _logger.Fatal(message, ex);
#endif
        }

        public static void StatsBuyInvisibleModeStart()
        {
            var eventName = "IAPInvisibleMode";
            Telemetry.TrackEvent("BuyStart", new Dictionary<string, string>()
            {
                {"IAP", eventName }
            });
        }

        public static void StatsBuyInvisibleModeComplete(bool isSuccess)
        {
            var eventName = "IAPInvisibleMode";
            Telemetry.TrackEvent("BuyFinish", new Dictionary<string, string>()
            {
                {"IAP", eventName },
                {"IsSuccess", isSuccess.ToString()}
            });
        }
    }
}

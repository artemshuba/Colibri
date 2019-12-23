using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Colibri.Services;

namespace Colibri.Helpers
{
    public class LicenseHelper
    {
        private const string IAPInvisibleMode = "ColibriInvisibleModeAccess";

        private LicenseInformation _licenseInfo;

        private static readonly LicenseHelper _instance = new LicenseHelper();

        public static LicenseHelper Instance
        {
            get { return _instance; }
        }

        private bool _hasAccessToInvisibleMode;

        public bool HasAccessToInvisibleMode
        {
            get
            {
                if (_licenseInfo == null)
                    return false;

                return _hasAccessToInvisibleMode || _licenseInfo.ProductLicenses[IAPInvisibleMode].IsActive;
            }
        }

        public void Initialize()
        {
            try
            {
#if DEBUG
                _licenseInfo = CurrentAppSimulator.LicenseInformation;
#else
                _licenseInfo = CurrentApp.LicenseInformation;
#endif
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to initialize LicenseInformation");
            }
        }

        public async Task BuyInvisibleModeAccess()
        {
            Logger.StatsBuyInvisibleModeStart();

            try
            {
#if DEBUG
                var result = await CurrentAppSimulator.RequestProductPurchaseAsync(IAPInvisibleMode);
                _hasAccessToInvisibleMode = true;
#else
                var result = await CurrentApp.RequestProductPurchaseAsync(IAPInvisibleMode);
                _hasAccessToInvisibleMode = (result.Status == ProductPurchaseStatus.Succeeded || result.Status == ProductPurchaseStatus.AlreadyPurchased);
#endif

                Logger.StatsBuyInvisibleModeComplete(_hasAccessToInvisibleMode);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occured while buying " + IAPInvisibleMode);
            }
        }

    }
}

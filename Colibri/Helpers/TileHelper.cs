using Windows.UI.Notifications;

namespace Colibri.Helpers
{
    public static class TileHelper
    {

        public static void ClearTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();

            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }
    }
}
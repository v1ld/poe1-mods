using Patchwork.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace V1ldQuickLoadMostRecentSaveGame
{
    [ModifiesType]
    class V1ld_InGameHUD : InGameHUD
    {
        [ModifiesMember("QuickLoadGameOnFadeEnd")]
        new private void QuickLoadGameOnFadeEnd()
        {
            FadeManager instance = FadeManager.Instance;
            instance.OnFadeEnded = (FadeManager.OnFadeEnd)Delegate.Remove(instance.OnFadeEnded, new FadeManager.OnFadeEnd(QuickLoadGameOnFadeEnd));
            bool flag = false;
            while (!flag)
            {
                IEnumerable<SaveGameInfo> enumerable = SaveGameInfo.CachedSaveGameInfo.OrderByDescending((SaveGameInfo sgi) => sgi?.RealTimestamp ?? DateTime.Now);
                if (enumerable != null && enumerable.Any())
                {
                    flag = GameResources.LoadGame(enumerable.First().FileName);
                    continue;
                }
                break;
            }
            if (!flag)
            {
                FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.AreaTransition, 0.35f);
            }
        }
    }
}
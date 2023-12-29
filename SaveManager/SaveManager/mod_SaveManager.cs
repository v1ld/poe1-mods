using Patchwork.Attributes;
using System;

namespace SaveManager
{
    [ModifiesType]
    public class mod_UISaveLoadPlaythroughGroup : UISaveLoadPlaythroughGroup
    {
        [ModifiesMember("CompareSaveFiles")]
        new public int CompareSaveFiles(UISaveLoadSave save1, UISaveLoadSave save2)
        {
            if (save1.IsNew)
                return -1;
            if (save2.IsNew)
                return 1;
            return -DateTime.Compare(save1.MetaData.RealTimestamp, save2.MetaData.RealTimestamp);
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Patchwork.Attributes;
using System;
using System.IO;
using UnityEngine;

namespace V1ldOverrideContainerLoot
{
    [ModifiesType]
    class V1ld_Loot: Loot
    {
        [ModifiesMember("PopulateInventory")]
        new public void PopulateInventory()
        {
            string map = GameState.Instance.CurrentMap.SceneName;
            string container = this.gameObject.name;
            OCLLog($"Map={map} Container={container}");

            if (m_populate)
            {
                return;
            }
            m_populate = true;

            Inventory inventory = GetComponent<Inventory>();
            var items = ReadContainerFile(map, container);
            if (items.Count > 0)
            {
                OCLLog($"{items.Count} new entries for {container}");
                foreach (var entry in items)
                {
                    string name = (string)entry["item"];
                    int count = (int)entry["count"];
                    OCLLog($"{count}x {name}");
                    Item item = GameResources.LoadPrefab<Item>(name, instantiate: false);
                    if (item != null)
                    {
                        inventory.AddItem(item, count, forceSlot: -1, original: true);
                    }
                    else
                    {
                        OCLLog($"ERROR: can't load item {name}!", force: true);
                    }

                }
            }
            // check to see if the game has a LootList for this container only if we didn't override
            else
            {
                object[] array = null;
                if (LootList != null)
                {
                    SetSeed();
                    array = LootList.Evaluate();
                    ResetSeed();
                }
                if (array == null || array.Length == 0)
                {
                    return;
                }

                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] is GameObject)
                    {
                        Item component2 = (array[i] as GameObject).GetComponent<Item>();
                        if ((bool)component2)
                        {
                            inventory.AddItem(component2, 1, -1, original: true);
                        }
                    }
                }
            }
        }

        [NewMember]
        private static JArray ReadContainerFile(string map, string container)
        {
            string containerFile = GetContainerFilePath(map, container);
            if (!File.Exists(containerFile))
            {
                OCLLog($"No file for {map}:{container}");
                return null;
            }
            OCLLog(containerFile);

            try
            {
                using (StreamReader streamReader = new StreamReader(containerFile))
                using (JsonReader reader = new JsonTextReader(streamReader))
                {
                    var json = (JObject)JToken.ReadFrom(reader);
                    return (JArray)json["items"];
                }
            }
            catch (Exception ex)
            {
                OCLLog($"Read failed: {ex.Message}", force: true);
                return null;
            }

            string GetContainerFilePath(string m, string c)
            {
                string modPath = Path.Combine(Application.dataPath, "../Mods/OCLmod");
                string mapPath = Path.Combine(modPath, m);
                string cFile = Path.Combine(mapPath, c + ".json");
                return Path.GetFullPath(cFile);
            }
        }

        [NewMember]
        private static void OCLLog(string message, bool force = false)
        {
            if (V1ldGameStateOCL.s_OCLVerbose || force)
            {
                Console.AddMessage($"OCL: {message}");
            }
        }
    }

    [ModifiesType]
    class V1ldGameStateOCL : GameState
    {
        [NewMember]
        public static bool s_OCLVerbose;
    }

    [ModifiesType("CommandLine")]
    public static class V1ld_CommandLineOCL
    {
        [NewMember]
        public static void OCLVerbose()
        {
            V1ldGameStateOCL.s_OCLVerbose = !V1ldGameStateOCL.s_OCLVerbose;
            Console.AddMessage($"Override Container Loot console messages: { (V1ldGameStateOCL.s_OCLVerbose ? "enabled" : "disabled") }");
        }
    }
}
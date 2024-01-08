using Patchwork.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using UnityEngine;

namespace V1ldOverrideContainerLoot
{
    [ModifiesType]
    class V1ld_Loot: Loot
    {
        [ModifiesMember("PopulateInventory")]
        new public void PopulateInventory()
        {
            Console.AddMessage("Loot: iname=" + this.gameObject.name + " map=" + GameState.Instance.CurrentMap.SceneName);
            if (m_populate)
            {
                return;
            }
            m_populate = true;
            object[] array = null;
            if (LootList != null)
            {
                Console.AddMessage("LootList: ");
                SetSeed();
                array = LootList.Evaluate();
                ResetSeed();
            }
            Inventory component = GetComponent<Inventory>();
            V1ldItemList list = V1ldItemList.Read(GameState.Instance.CurrentMap.SceneName, this.gameObject.name);
            if (list != null && list.items != null)
            {
                Console.AddMessage($"Loot: injecting {list.items.Length} items!");
                var items = list.items;
                for (int i = 0; i < items.Length; i++)
                {
                    Console.AddMessage($"{i}: {items[i].count}x {items[i].item}");
                    Item item = GameResources.LoadPrefab<Item>(items[i].item, instantiate: false);
                    component.AddItem(item, items[i].count, forceSlot: -1, original: true);
                }
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
                        component.AddItem(component2, 1, -1, original: true);
                    }
                }
            }
        }
    }

    [NewType]
    public class V1ldItemAndCount
    {
        public string item;
        public int count;
    }

    [NewType]
    public class V1ldItemList
    {
        public V1ldItemAndCount[] items;

        public static V1ldItemList GetTestData()
        {
            V1ldItemList list = new V1ldItemList { items = new V1ldItemAndCount[2] };

            for (int i = 0; i < list.items.Length; i++)
            {
                list.items[i] = new V1ldItemAndCount { item = "item" + i, count = i };
            }
            return list;
        }

        public static V1ldItemList Read(string map, string container)
        {
            string containerFile = GetContainerFilePath(map, container);
            if (!File.Exists(containerFile))
            {
                Console.AddMessage($"Can't find container file: {containerFile}");
                return null;
            }
            try
            {
                using (StreamReader inputFile = new StreamReader(containerFile))
                {
                    string json = inputFile.ReadToEnd();
                    var serializer = new JavaScriptSerializer();
                    var itemList = serializer.Deserialize<V1ldItemList>(json);
                    return itemList;
                }
            }
            catch (Exception ex)
            {
                Console.AddMessage($"Caught: {ex.Message}");
                return null;
            }
        }

        public static void Write(string map, string container, V1ldItemList Items)
        {
            try
            {
                //Console.AddMessage("Directory is " + Application.dataPath);
                //string mapPath = Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, "Managed/OCLmod"), map));
                //Console.AddMessage("Directory is " + mapPath);
                //Directory.CreateDirectory(mapPath);
                //string containerFile = Path.Combine(mapPath, container + ".json");
                string containerFile = GetContainerFilePath(map, container);
                using (StreamWriter outputFile = new StreamWriter(containerFile, false))
                {
                    var serializer = new JavaScriptSerializer();
                    var serializedResult = serializer.Serialize(Items);
                    outputFile.WriteLine(serializedResult);
                }
            }
            catch (Exception ex)
            {
                Console.AddMessage("here");
                Console.AddMessage($"Failed to write {ex.Message}");
            }
        }

        private static string GetContainerFilePath(string map, string container)
        {
            string modPath = Path.Combine(Application.dataPath, "Managed/OCLmod");
            string mapPath = Path.Combine(modPath, map);
            string containerFile = Path.Combine(mapPath, container + ".json");
            Console.AddMessage($"Container file: {containerFile}");
            return Path.GetFullPath(containerFile);
        }
    }
}
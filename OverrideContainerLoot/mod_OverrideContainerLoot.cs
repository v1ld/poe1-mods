using Patchwork.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            Console.AddMessage("Loot: Map=" + GameState.Instance.CurrentMap.SceneName + " Container=" + this.gameObject.name);
            if (m_populate)
            {
                return;
            }
            m_populate = true;

            Inventory component = GetComponent<Inventory>();
            string map = GameState.Instance.CurrentMap.SceneName;
            string container = this.gameObject.name;
            V1ldItemList list = V1ldItemList.Read(map, container);
            if (list?.items != null)
            {
                Console.AddMessage($"Loot: injecting {list.items.Length} items for {map}:{container}");
                var items = list.items;
                for (int i = 0; i < items.Length; i++)
                {
                    Console.AddMessage($"{i}: {items[i].count}x {items[i].item}");
                    Item item = GameResources.LoadPrefab<Item>(items[i].item, instantiate: false);
                    if (item != null)
                    {
                        component.AddItem(item, items[i].count, forceSlot: -1, original: true);
                    }
                    else
                    {
                        Console.AddMessage($"ERROR: can't load item {items[i].item}!");
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
                            component.AddItem(component2, 1, -1, original: true);
                        }
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

        public static V1ldItemList Read(string map, string container)
        {
            string containerFile = GetContainerFilePath(map, container);
            Console.AddMessage($"File: {Scrunch(containerFile)}");
            if (!File.Exists(containerFile))
            {
                Console.AddMessage($"No file for {map}:{container}, skipping.");
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
                Console.AddMessage($"Failed to write {ex.Message}");
            }
        }

        private static string GetContainerFilePath(string map, string container)
        {
            string modPath = Path.Combine(Application.dataPath, "Managed/OCLmod");
            string mapPath = Path.Combine(modPath, map);
            string containerFile = Path.Combine(mapPath, container + ".json");
            return Path.GetFullPath(containerFile);
        }

        private static string Scrunch(string path)
        {
            return "..." + path.Substring(path.Length - 60);
        }
    }
}
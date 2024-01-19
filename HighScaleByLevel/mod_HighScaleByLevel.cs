using Newtonsoft.Json;
using Patchwork.Attributes;
using System.IO;
using System;
using UnityEngine;

namespace HighScaleByLevel
{
    [NewType]
    internal class V1ldScaleByLevel
    {
        public int LevelStart;
        public int LevelEnd;
        public int AccuracyDefenseStart;
        public int AccuracyDefenseEnd;
        public float HealthMultiplierStart;
        public float HealthMultiplierEnd;
        public float EnemyLevelMultiplierStart;
        public float EnemyLevelMultiplierEnd;

        public V1ldScaleByLevel()
        {
            // Default to no tweaks at all
            LevelStart = 1;
            LevelEnd = 1;
            AccuracyDefenseStart = 0;
            AccuracyDefenseEnd = 0;
            HealthMultiplierStart = 1f;
            HealthMultiplierEnd = 1f;
            EnemyLevelMultiplierStart = 1f;
            EnemyLevelMultiplierEnd = 1f;
        }

        [JsonIgnore]
        private static V1ldScaleByLevel _instance;

        public static V1ldScaleByLevel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = ReadConfigFile();

                if (_instance == null) {
                    _instance = new V1ldScaleByLevel();
                    WriteConfigFile(_instance);
                }
                else if (_instance.LevelStart <= 0 || _instance.LevelEnd > 16 || _instance.LevelStart > _instance.LevelEnd) {
                    Console.AddMessage("SDBL: Level out of bounds (<= 0, > 16, or Start > End)!");
                    Console.AddMessage("SDBL: Scaling disabled. Please fix your config file.");
                    _instance = new V1ldScaleByLevel();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        public static void ReloadConfig()
        {
            var config = ReadConfigFile();
            Console.AddMessage($"SDBL: Config reload {(config != null ? "succeeded" : "failed")}");

            if (config != null)
                Instance = config;
        }

        public static void WriteConfig()
        {
            WriteConfigFile(Instance);
            Console.AddMessage($"SDBL: Config written");
        }

        private float ScaleToLevel(float start, float end, int level)
        {
            // These two checks implicitly handle the case where LevelEnd == LevelStart.
            // They MUST always have equality as part of one of the conditions therefore.
            if (level <= LevelStart)
                return start;
            if (level > LevelEnd)
                return end;

            // This check is redundant, but why not
            if (LevelStart == LevelEnd)
                return start;

            return start + (end - start) * (level - LevelStart) / (LevelEnd - LevelStart);
        }

        public float AccuracyDefenseModifier(int level) => ScaleToLevel(AccuracyDefenseStart, AccuracyDefenseEnd, level);

        public float HealthMultiplierModifier(int level) => ScaleToLevel(HealthMultiplierStart, HealthMultiplierEnd, level);

        public float EnemyLevelMultiplierModifier(int level)
        {
            float scale = ScaleToLevel(EnemyLevelMultiplierStart, EnemyLevelMultiplierEnd, level);
            return (scale > 0f) ? scale : 1f;  // 1 is the default multiplier
        }

        public static string ShowCurrentValues(int level)
        {
            string result = $"Accuracy/Defenses={Signed(Instance.AccuracyDefenseModifier(level))}"
                          + $" Health/Endurance={Instance.HealthMultiplierModifier(level)}"
                          + $" EnemyLevelMultiplier={Instance.EnemyLevelMultiplierModifier(level)}";

            return result;

            string Signed(float value)
            {
                return (value > 0 ? "+" : "") + value.ToString();
            }
        }

        private static V1ldScaleByLevel ReadConfigFile()
        {
            string configFile = GetConfigFilePath();
            if (!File.Exists(configFile))
                return null;

            try
            {
                using (StreamReader streamReader = new StreamReader(configFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (V1ldScaleByLevel)serializer.Deserialize(streamReader, typeof(V1ldScaleByLevel));
                }
            }
            catch (Exception ex)
            {
                Console.AddMessage($"Could not read config file {configFile}: {ex}");
                return null;
            }
        }

        private static void WriteConfigFile(V1ldScaleByLevel config)
        {
            string configFile = GetConfigFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(configFile));

            try
            {
                using (StreamWriter streamWriter = new StreamWriter(configFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(streamWriter, config);
                }
            }
            catch (Exception ex)
            {
                Console.AddMessage($"Could not write config file {configFile}: {ex}");
            }
        }

        private static string GetConfigFilePath()
        {
            string modPath = Path.Combine(Application.dataPath, "../Mods/");
            string cFile = Path.Combine(modPath, "ScaleDifficultyByLevel.json");
            return Path.GetFullPath(cFile);
        }
    }

    [ModifiesType]
    class V1ldCharacterStats : CharacterStats
    {
        [NewMember]
        private V1ldScaleByLevel levelScaler = new V1ldScaleByLevel();

        [ModifiesMember("get_DifficultyStatBonus")]
        public float get_DifficultyStatBonus()
        {
            if (!IsPartyMember && GameState.Instance != null)
            {
                if (GameState.Instance.IsDifficultyPotd)
                {
                    return 15f;
                }
                if (GameState.Instance.Difficulty == GameDifficulty.Hard)
                {
                    return V1ldScaleByLevel.Instance.AccuracyDefenseModifier(Level);
                }
            }
            return 0f;
        }

        [ModifiesMember("get_DifficultyHealthStaminaMult")]
        public float get_DifficultyHealthStaminaMult()
        {
            if (!IsPartyMember && !HasFactionSwapEffect() && GameState.Instance != null)
            {
                if (GameState.Instance.IsDifficultyPotd)
                {
                    return 1.25f;
                }
                if (GameState.Instance.Difficulty == GameDifficulty.Hard)
                {
                    return V1ldScaleByLevel.Instance.HealthMultiplierModifier(Level);
                }
                if (GameState.Instance.IsDifficultyStoryTime)
                {
                    return 0.5f;
                }
                return 1f;
            }
            return 1f;
        }

        [ModifiesMember("get_DifficultyLevelMult")]
        public float get_DifficultyLevelMult()
        {
            if (!IsPartyMember && !HasFactionSwapEffect() && GameState.Instance != null)
            {
                if (GameState.Instance.Difficulty == GameDifficulty.Hard)
                {
                    return V1ldScaleByLevel.Instance.EnemyLevelMultiplierModifier(Level);
                }
                if (GameState.Instance.IsDifficultyStoryTime)
                {
                    return 0.75f;
                }
            }
            return 1f;
        }
    }

    [ModifiesType("CommandLine")]
    public static class V1ld_CommandLine
    {
        [NewMember]
        public static void SDBLLoad()
        {
            V1ldScaleByLevel.ReloadConfig();
        }

        [NewMember]
        public static void SDBLWrite()
        {
            V1ldScaleByLevel.WriteConfig();
        }

        [NewMember]
        public static void SDBLShow()
        {
            int? level = GameState.s_playerCharacter?.GetComponent<CharacterStats>()?.Level;
            string result = (level != null) ? V1ldScaleByLevel.ShowCurrentValues((int)level) : "Cannot determine player level!";
            Console.AddMessage(result);
        }

        [NewMember]
        public static void SDBLSettings()
        {
            Console.AddMessage($"Level: Start={V1ldScaleByLevel.Instance.LevelStart} End={V1ldScaleByLevel.Instance.LevelEnd}");
            Console.AddMessage($"Accuracy/Defense: Start={V1ldScaleByLevel.Instance.AccuracyDefenseStart} End={V1ldScaleByLevel.Instance.AccuracyDefenseEnd}");
            Console.AddMessage($"Health/Endurance: Start={V1ldScaleByLevel.Instance.HealthMultiplierStart} End={V1ldScaleByLevel.Instance.HealthMultiplierEnd}");
            Console.AddMessage($"Enemy Level Multiplier: Start={V1ldScaleByLevel.Instance.EnemyLevelMultiplierStart} End={V1ldScaleByLevel.Instance.EnemyLevelMultiplierEnd}");
        }
    }
}
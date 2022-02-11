using LLHandlers;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LLBML.Utils;

namespace BasicStages
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    class BasicStages : BaseUnityPlugin
    {
        #region legacystrings
        private const string modVersion = PluginInfos.PLUGIN_VERSION;
        private const string repositoryOwner = "Daioutzu";
        private const string repositoryName = "LLBMM-BasicStages";
        #endregion

        public static BasicStages Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }
        public static bool InGame => World.instance != null && (DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.CDOFDJMLGLO || DNPFJHMAIBP.HHMOGKIMBNM() == JOFJHDJHJGI.LGILIJKMKOD);

#if useAssetBundle != true

        internal static DirectoryInfo PluginDirectory { get; private set; }
        internal static DirectoryInfo ModdingDirectory { get; private set; }
        private readonly static string bundleLocation = BepInEx.Utility.CombinePaths(PluginDirectory.FullName, "Bundles", "bs_materials");
        private static AssetBundle uiBundle;
        public static Dictionary<string, Material> materialAssets = new Dictionary<string, Material>();
        public static Dictionary<string, Sprite> spriteAssets = new Dictionary<string, Sprite>();
        public static bool bundleLoaded { get; private set; }

        private static void LoadAssets()
        {
            if (File.Exists(bundleLocation))
            {
                uiBundle = AssetBundle.LoadFromFile(bundleLocation);
                Material[] materials = uiBundle.LoadAllAssets<Material>();
#if DEBUG
                string txt = ""; 
#endif
                for (int i = 0; i < materials.Length; i++)
                {
                    materialAssets.Add(materials[i].name, materials[i]);
#if DEBUG
                    txt += $"Material: {materials[i].name}\n";
#endif
                }
                bundleLoaded = true;
#if DEBUG
                BasicStages.Log.LogDebug($"{txt}");
#endif
            }
            else
            {
                Log.LogWarning("The \"stagecolours\" could not be loaded. Using coded colours as a workaround");
            }
        }

#endif

        void Awake()
        {
            PluginDirectory = new DirectoryInfo(Path.GetDirectoryName(this.Info.Location));
            ModdingDirectory = ModdingFolder.GetModSubFolder(this.Info);
            Instance = this;
            Log = this.Logger;
            FileSystem.Init();
            LoadAssets();
            AddModOptions();
        }

        void Start()
        {
            Logger.LogDebug("Started");
        }

        private ConfigEntry<bool> overrideAllStagesToBasic;
        private Dictionary<Stage, ConfigEntry<bool>> regularStagesConfig = new Dictionary<Stage, ConfigEntry<bool>>();
        private Dictionary<Stage, ConfigEntry<bool>> retroStagesConfig = new Dictionary<Stage, ConfigEntry<bool>>();
        private void AddModOptions()
        {
            overrideAllStagesToBasic = this.Config.Bind<bool>("Toggles", "overrideAllStagesToBasic", true);
            this.Config.Bind<string>("Toggles", "toggles_gap1", "20", new ConfigDescription("", null, "mod_menugap"));
            this.Config.Bind<string>("Toggles", "toggles_header_basicstages", "Basic Stages", new ConfigDescription("", null, "mod_menuheader"));

            foreach (var stageName in regularStagesNames)
            {
                regularStagesConfig.Add(stageName.Key, Config.Bind<bool>("Toggles", stageName.Value, false));
            }
            this.Config.Bind<string>("Toggles", "toggles_gap2", "20", new ConfigDescription("", null, "mod_menugap"));
            this.Config.Bind<string>("Toggles", "toggles_header_retrostages", "Basic Retro Stages", new ConfigDescription("", null, "mod_menuheader"));

            foreach (var stageName in retroStagesNames)
            {
                retroStagesConfig.Add(stageName.Key, Config.Bind<bool>("Toggles", stageName.Value, false));
            }
        }

        bool initialStageCheck = false;

        static readonly Dictionary<Stage, string> regularStagesNames = new Dictionary<Stage, string>
        {
            [Stage.OUTSKIRTS] = "outskirts",
            [Stage.SEWERS] = "sewers",
            [Stage.JUNKTOWN] = "desert",
            [Stage.CONSTRUCTION] = "elevator",
            [Stage.FACTORY] = "factory",
            [Stage.SUBWAY] = "subway",
            [Stage.STADIUM] = "stadium",
            [Stage.STREETS] = "streets",
            [Stage.POOL] = "pool",
            [Stage.ROOM21] = "room21"

        };

        static readonly Dictionary<Stage, string> retroStagesNames = new Dictionary<Stage, string>
        {
            [Stage.OUTSKIRTS_2D] = "retroOutskirts",
            [Stage.POOL_2D] = "retroPool",
            [Stage.SEWERS_2D] = "retroSewers",
            [Stage.ROOM21_2D] = "retroRoom21",
            [Stage.STREETS_2D] = "retroStreets",
            [Stage.SUBWAY_2D] = "retroTrain",
            [Stage.FACTORY_2D] = "retroFactory",

        };

        void LateUpdate()
        {
            if (InGame)
            {
                if (GameObject.Find("Background") && initialStageCheck == false)
                {
                    initialStageCheck = true;
                    if (CheckBasicStageChoice())
                    {
                        gameObject.AddComponent<StageVisualHandler>();
                    }
                }
            }
            else if (initialStageCheck == true)
            {
                Destroy(gameObject.GetComponent<StageVisualHandler>());
                initialStageCheck = false;
            }
        }

        bool CheckBasicStageChoice()
        {
            if (overrideAllStagesToBasic.Value)
            {
                return true;
            }
            else
            {
                try
                {
                    return StageBackground.BG.instance.is2D ? retroStagesConfig[StageHandler.curStage].Value : regularStagesConfig[StageHandler.curStage].Value;
                }
                catch (System.Exception)
                {
                    Logger.LogWarning("Stage wasn't listed:" + StageHandler.curStage);
                    return false;
                }
            }
        }
    }
}

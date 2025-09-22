using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OutwardEnchanter.Managers;
using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// RENAME 'OutwardEnchanter' TO SOMETHING ELSE
namespace OutwardEnchanter
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class OutwardEnchanter : BaseUnityPlugin
    {
        // Choose a GUID for your project. Change "myname" and "mymod".
        public const string GUID = "gymmed.outwardenchanter";
        // Choose a NAME for your project, generally the same as your Assembly Name.
        public const string NAME = "Outward Enchanter";
        // Increment the VERSION when you release a new version of your mod.
        public const string VERSION = "0.3.0";

        public static string prefix = "[GymMed-Enchanter]";

        public const string GUI_SHOW = "Outward Enchanter GUI Show/Hide";

        internal static ManualLogSource Log;

        // If you need settings, define them like so:
        //public static ConfigEntry<bool> ExampleConfig;

        // Awake is called when your plugin is created. Use this to set up your mod.
        internal void Awake()
        {
            Log = this.Logger;
            Log.LogMessage($"Hello world from {NAME} {VERSION}!");

            // Any config settings you define should be set up like this:
            //ExampleConfig = Config.Bind("ExampleCategory", "ExampleSetting", false, "This is an example setting.");
            CustomKeybindings.AddAction(GUI_SHOW, KeybindingsCategory.CustomKeybindings, ControlType.Both);

            SL.OnPacksLoaded += ()=>
            {
                GUIManager.Instance.CreateCanvas();
            };

            // Harmony is for patching methods. If you're not patching anything, you can comment-out or delete this line.
            new Harmony(GUID).PatchAll();
        }

        // Update is called once per frame. Use this only if needed.
        // You also have all other MonoBehaviour methods available (OnGUI, etc)
        internal void Update()
        {
            if (!CustomKeybindings.GetKeyDown(GUI_SHOW))
            {
                return;
            }
            GUIMainCanvasManager GUICanvasManager = GUIManager.Instance?.MainCanvasManager;

            if(GUICanvasManager == null)
            {
                LogMessage("Can't access null! Make sure GUI Canvas exist!");
                return;
            }

            if(GUICanvasManager.gameObject.activeSelf)
            {
                GUICanvasManager.HideCanvas();
            }
            else
            {
                GUICanvasManager.ShowCanvas();
                GUICanvasManager.transform.SetAsLastSibling();
            }
        }

        public static void LogMessage(string message)
        {
            Log.LogMessage(OutwardEnchanter.prefix + " " + message);
        }

        [HarmonyPatch(typeof(CharacterManager), nameof(CharacterManager.OnAllPlayersDoneLoading))]
        public class CharacterManager_OnAllPlayersDoneLoading
        {
            static void Postfix()
            {
                #if DEBUG
                LogMessage("CharacterManager@OnAllPlayersDoneLoading called!");
                #endif
                OutwardEnchanter.FillSpawnableItemToGUI();
            }
        }

        public static void FillSpawnableItemToGUI()
        {
            try
            {
                #if DEBUG
                LogMessage("OutwardEnchanter@FillSpawnableItemToGUI called!");
                LogMessage($"OutwardEnchanter@FillSpawnableItemToGUI GUICanvasManager exist: {GUIManager.Instance?.MainCanvasManager == null}");
                #endif

                GUIMainCanvasManager GUICanvasManager = GUIManager.Instance?.MainCanvasManager;

                if (GUICanvasManager == null)
                {
                    LogMessage("Can't access null! Make sure GUI Canvas exist!");
                    return;
                }

                LogMessage($"OutwardEnchanter@FillSpawnableItemToGUI character exist: {CharacterEnchanterManager.Instance?.MainCharacter == null}");

                List<Item> spawnableItems = ResourcesPrefabManager.Instance.EDITOR_GetSpawnableItemPrefabs(CharacterEnchanterManager.Instance.MainCharacter);

                if(spawnableItems == null || spawnableItems.Count < 1)
                {
                    string warning = spawnableItems == null ? "null" : spawnableItems.Count.ToString();
                    LogMessage($"OutwardEnchanter@FillSpawnableItemToGUI spawnable items doesn't exist. " + 
                        $"spawnableItems: {warning} character exist: {CharacterEnchanterManager.Instance.MainCharacter == null}");
                    return;
                }
                List<Equipment> spawnableEquipment = new List<Equipment>(spawnableItems.OfType<Equipment>().ToList());

                GUICanvasManager.AvailableEquipment = spawnableEquipment;
                GUICanvasManager.FillItemsData();

                GUICanvasManager.transform.SetAsLastSibling();
            }
            catch (Exception ex)
            { 
                OutwardEnchanter.LogMessage("OutwardEnchanter@FillSpawnableItemToGUI error: " + ex.Message);
            }
        }
    }
}

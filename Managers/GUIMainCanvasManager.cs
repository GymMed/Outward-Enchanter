using Mono.Cecil;
using OutwardEnchanter.Helpers;
using SideLoader.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OutwardEnchanter.Managers
{
    public class GUIMainCanvasManager : MonoBehaviour
    {
        private static GUIMainCanvasManager _instance;

        private List<Equipment> _availableEquipment = new List<Equipment>();
        private List<EnchantmentRecipe> _availableEnchantmentRecipes = new List<EnchantmentRecipe>();

        private Dictionary<string, Equipment> _equipmentDictionary = new Dictionary<string, Equipment>();
        private Dictionary<string, EnchantmentRecipe> _enchantmentRecipeDictionary = new Dictionary<string, EnchantmentRecipe>();

        private Text _resultText;

        private Button _enchantButton;
        private Button _closeButton;

        private Dropdown _chooseItemDropdown;
        private Dropdown _chooseEnchantmentDropdown;

        private InputField _itemFilterInput;
        private InputField _enchantmentFilterInput;

        private int availableEquipmentCount;

        private GUIMainCanvasManager()
        {
            try 
            {
                Init();
            }
            catch(Exception ex)
            {
                OutwardEnchanter.LogMessage("GUIMainCanvasManager error: " + ex.Message);
            }
        }

        public static GUIMainCanvasManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GUIMainCanvasManager();

                return _instance;
            }
        }

        public Text ResultText { get => _resultText; set => _resultText = value; }

        public Button EnchantButton { get => _enchantButton; set => _enchantButton = value; }
        public Button CloseButton { get => _closeButton; set => _closeButton = value; }

        public Dropdown ChooseItemDropdown { get => _chooseItemDropdown; set => _chooseItemDropdown = value; }
        public Dropdown ChooseEnchantmentDropdown { get => _chooseEnchantmentDropdown; set => _chooseEnchantmentDropdown = value; }

        public List<Equipment> AvailableEquipment { get => _availableEquipment; set => _availableEquipment = value; }
        public List<EnchantmentRecipe> AvailableEnchantmentRecipes { get => _availableEnchantmentRecipes; set => _availableEnchantmentRecipes = value; }

        public Dictionary<string, Equipment> EquipmentDictionary { get => _equipmentDictionary; set => _equipmentDictionary = value; }
        public Dictionary<string, EnchantmentRecipe> EnchantmentRecipeDictionary { get => _enchantmentRecipeDictionary; set => _enchantmentRecipeDictionary = value; }

        public InputField EnchantmentFilterInput { get => _enchantmentFilterInput; set => _enchantmentFilterInput = value; }
        public InputField ItemFilterInput { get => _itemFilterInput; set => _itemFilterInput = value; }

        public int AvailableEquipmentCount { get => availableEquipmentCount; set => availableEquipmentCount = value; }

        public void Init()
        {
            string mainPanelPath = "Background-Panel/Main-Panel/";

            ResultText = this.transform.Find(mainPanelPath + "Lower-Panel/Result-Text")?.GetComponent<Text>();

            if(ResultText == null)
            {
                OutwardEnchanter.LogMessage("Couldn't find Result Text component");
            }

            Transform headerPanelTransform = this.transform.Find(mainPanelPath + "Header-Panel/");

            if (headerPanelTransform == null)
            {
                ResultAndLogMessage("Couldn't find GUI header");
            }
            else
            {
                headerPanelTransform.gameObject.AddComponent<DragParent>();

                CloseButton = headerPanelTransform.Find("Close-Button")?.GetComponent<Button>();

                if (CloseButton == null)
                {
                    ResultAndLogMessage("Couldn't find Close Button component");
                }
                else
                {
                    CloseButton.onClick.AddListener(() =>
                    {
                        this.HideCanvas();
                    });
                }
            }

            EnchantButton = this.transform.Find(mainPanelPath + "Middle-Panel/Enchant-Button")?.GetComponent<Button>();

            if(EnchantButton == null)
            {
                ResultAndLogMessage("Couldn't find Enchant Button component");
            }
            else
            {
                EnchantButton.onClick.AddListener(() => HandleOnEnchantButtonClick());
            }

            ChooseItemDropdown = this.transform.Find(mainPanelPath + "Middle-Panel/ItemPicker-Panel/ItemPick-Dropdown")?.GetComponent<Dropdown>();
            
            if(ChooseItemDropdown == null)
            {
                ResultAndLogMessage("Couldn't find Items Dropdown component");
            }
            else
            {
                try
                {
                    FillItemsData();
                    ChooseItemDropdown.onValueChanged.AddListener((int index) => HandleOnChooseItemChange(index));
                }
                catch (Exception ex) 
                {
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@Init error: " + ex.Message);
                }
            }

            ChooseEnchantmentDropdown = this.transform.Find(mainPanelPath + "Middle-Panel/EnchantmentPicker-Panel/EnchantmentPick-Dropdown")?.GetComponent<Dropdown>();

            if(ChooseEnchantmentDropdown == null)
            {
                ResultAndLogMessage("Couldn't find Enchantments Dropdown component");
            }

            ItemFilterInput = this.transform.Find(mainPanelPath + "Middle-Panel/ItemFilter-Panel/ItemFilter-Input")?.GetComponent<InputField>();

            if(ItemFilterInput == null)
            {
                ResultAndLogMessage("Couldn't find Item Filter Input component");
            }
            else
            {
                ItemFilterInput.onEndEdit.AddListener(HandleOnItemFilterEnd);
            }

            EnchantmentFilterInput = this.transform.Find(mainPanelPath + "Middle-Panel/EnchantmentFilter-Panel/EnchantmentFilter-Input")?.GetComponent<InputField>();

            if(EnchantmentFilterInput == null)
            {
                ResultAndLogMessage("Couldn't find Enchantment Filter Input component");
            }
            else
            {
                EnchantmentFilterInput.onEndEdit.AddListener(HandleOnEnchantmentFilterEnd);
            }
        }

        public void HandleOnItemFilterEnd(string text)
        {
            FilterItemsData(text);           
        }

        public void HandleOnEnchantmentFilterEnd(string text)
        {
            FilterItemsData(ItemFilterInput.text);
            FilterEnchantmentsDataBasedOnItem(text);           
        }

        public void HandleOnEnchantButtonClick()
        {
            if(ChooseItemDropdown == null)
            {
                this.ResultAndLogMessage("Couldn't find item selection dropdown!");
                return;
            }

            if(ChooseEnchantmentDropdown == null)
            {
                this.ResultAndLogMessage("Couldn't find enchantment recipe selection dropdown!");
                return;
            }
            
            if(ChooseItemDropdown.options == null || ChooseItemDropdown.options.Count < 1)
            {
                this.ResultAndLogMessage($"Couldn't generate equipment options in dropdown! Available equipment count: {AvailableEquipmentCount}");
                return;
            }

            string selectedItemValue = ChooseItemDropdown.options[ChooseItemDropdown.value].text;

            if(!EquipmentDictionary.TryGetValue(selectedItemValue, out Equipment equipment))
            {
                this.ResultAndLogMessage("Tried to enchant without providing and item!");
                return;
            }

            if(ChooseEnchantmentDropdown.options == null || ChooseEnchantmentDropdown.options.Count < 1)
            {
                this.ResultAndLogMessage($"Couldn't generate enchantments options in dropdown! Available enchantments count: {AvailableEnchantmentRecipes.Count}");
                return;
            }

            string selectedEnchantmentValue = ChooseEnchantmentDropdown.options[ChooseEnchantmentDropdown.value].text;

            if(!EnchantmentRecipeDictionary.TryGetValue(selectedEnchantmentValue, out EnchantmentRecipe enchantmentRecipe))
            {
                this.ResultAndLogMessage("Tried to enchant without providing and enchantment!");
                return;
            }

            if(selectedEnchantmentValue == "None")
            {
                try
                {
                    Item item = SpawnEquipment(equipment);
                    item.Start();

                    if (item is Equipment newEquipment)
                    {
    #if DEBUG
                        OutwardEnchanter.LogMessage("spawned item!");
    #endif
                        item.ApplyVisualModifications();
#if DEBUG
                        OutwardEnchanter.LogMessage("applied visuals!");
#endif
                        EnchantmentsHelper.SetItemAsGenerated(item);
                        this.ResultMessage($"Successfully spawned! \nequipment: {ChooseItemDropdown.options[ChooseItemDropdown.value].text}");
                    }
                }
                catch(Exception ex) 
                {
                    this.ResultAndLogMessage("Spawning error: " + ex.Message);
                }

            }

            if(!enchantmentRecipe.GetHasMatchingEquipment(equipment))
            {
                this.ResultAndLogMessage("Provided incompatible enchantment recipe with equipment!");
                return;
            }

            try
            {
                Item item = SpawnEquipment(equipment);
                item.Start();

                if (item is Equipment newEquipment)
                {
                    Enchantment enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantmentRecipe.RecipeID);
                    newEquipment.AddEnchantment(enchantment.PresetID, false);
#if DEBUG
                    OutwardEnchanter.LogMessage("added enchantment!");
#endif
                    item.ApplyVisualModifications();
#if DEBUG
                    OutwardEnchanter.LogMessage("applied visuals!");
#endif

                    EnchantmentsHelper.SetItemAsGenerated(item);
                    this.ResultMessage($"Successfully enchanted! \nequipment: {ChooseItemDropdown.options[ChooseItemDropdown.value].text} \n" + 
                        $"enchantment: {ChooseEnchantmentDropdown.options[ChooseEnchantmentDropdown.value].text}");
                }
            }
            catch(Exception ex) 
            {
                this.ResultAndLogMessage("Enchanting error: " + ex.Message);
            }
        }

        public void HandleOnChooseItemChange(int index)
        {
            try
            {
                FilterEnchantmentsDataBasedOnItem(EnchantmentFilterInput.text);
            }
            catch(Exception ex) 
            {
                OutwardEnchanter.LogMessage("GUIMainCanvasManager@HandleOnChooseItemChange error: " + ex.Message);
            }
        }

        public void FilterItemsData(string filter)
        {
            string previousValue = "";
            int selectionValue = 0;

            if (ChooseItemDropdown.options.Count > 0)
                previousValue = ChooseItemDropdown.options[ChooseItemDropdown.value].text;

            this.ResultMessage("Loading Items . . .");

            ChooseEnchantmentDropdown.interactable = false;
            ChooseItemDropdown.interactable = false;

            Task.Run(() =>
                {
                    List<string> dropdownOptions = new List<string>();
                    string keyName = "";
                    Dictionary<string, Equipment> localEquipmentDictionary = new Dictionary<string, Equipment>();
                    List<Equipment> filteredEquipment = AvailableEquipment;

                    if(EnchantmentFilterInput.text != "")
                    {
                        filteredEquipment = ItemsHelper.GetEquipmentsByEnchantmentName(EnchantmentFilterInput.text, AvailableEquipment, EnchantmentRecipeDictionary);
                    }

                    int availableCount = filteredEquipment.Count;

                    if (string.IsNullOrEmpty(previousValue))
                    {
                        foreach (Equipment equipment in filteredEquipment)
                        {
                            keyName = ItemsHelper.GetUniqueEquipmentsName(equipment, localEquipmentDictionary);

                            if (EnchantmentsHelper.ContainsIgnoreCase(keyName, filter))
                            {
                                dropdownOptions.Add(keyName);
                                localEquipmentDictionary.Add(keyName, equipment);
                            }
                        }
                    }
                    else
                    {
                        foreach (Equipment equipment in filteredEquipment)
                        {
                            keyName = ItemsHelper.GetUniqueEquipmentsName(equipment, localEquipmentDictionary);

                            if (EnchantmentsHelper.ContainsIgnoreCase(keyName, filter))
                            {
                                if(string.Equals(keyName, previousValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    selectionValue = dropdownOptions.Count;
                                }
                                dropdownOptions.Add(keyName);
                                localEquipmentDictionary.Add(keyName, equipment);
                            }
                        }
                    }
                return (dropdownOptions, localEquipmentDictionary, selectionValue, availableCount);
            })
            .ContinueWith(t =>
            {
                // Main thread: update UI safely
                var (dropdownOptions, localEquipmentDict, selectionValue, availableCount) = t.Result;

                EquipmentDictionary = localEquipmentDict;
                AvailableEquipmentCount = availableCount;

                FillDropdownChoices(ChooseItemDropdown, dropdownOptions);

                ChooseItemDropdown.value = selectionValue;
                ChooseItemDropdown.RefreshShownValue();
                ChooseItemDropdown.onValueChanged.Invoke(ChooseItemDropdown.value);

                this.ResultMessage("Finished Loading Equipment!");

                ChooseEnchantmentDropdown.interactable = true;
                ChooseItemDropdown.interactable = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void FilterEnchantmentsDataBasedOnItem(string filter)
        {
            if(ChooseItemDropdown.value < 0 || ChooseItemDropdown.value >= ChooseItemDropdown.options.Count)
            {
                ResultAndLogMessage($"Invalid dropdown selection: {ChooseItemDropdown.value}, options count: {ChooseItemDropdown.options.Count}");
                return;
            }

            string selectedValue = ChooseItemDropdown.options[ChooseItemDropdown.value].text;
            if (!EquipmentDictionary.TryGetValue(selectedValue, out Equipment equipment))
            {
                ResultAndLogMessage($"Item: {selectedValue} is not found! Tried to access: {ChooseItemDropdown.value}");
                return;
            }

            string previousValue = "";
            int selectionValue = 0;

            if (ChooseEnchantmentDropdown.options.Count > 0)
                previousValue = ChooseEnchantmentDropdown.options[ChooseEnchantmentDropdown.value].text;

            this.ResultMessage("Loading Enchantments and Items . . .");

            ChooseEnchantmentDropdown.interactable = false;
            ChooseItemDropdown.interactable = false;

            Task.Run(() =>
                {
                    List<EnchantmentRecipe> availableRecipes = EnchantmentsHelper.GetAvailableEnchantmentRecipies(equipment);
                    Dictionary<string, EnchantmentRecipe> localRecipeDictionary = new Dictionary<string, EnchantmentRecipe>();
                    List<string> availableRecipesOptions = new List<string>();
                    string keyName = "";

                    //Default spawn item option
                    localRecipeDictionary.Add("None", new EnchantmentRecipe());
                    availableRecipesOptions.Add("None");

                    if (string.IsNullOrEmpty(previousValue))
                    {
                        foreach (EnchantmentRecipe recipe in availableRecipes)
                        {
                            keyName = EnchantmentsHelper.GetUniqueEnchantmentsName(recipe, localRecipeDictionary);

                            if (keyName == null || !EnchantmentsHelper.ContainsIgnoreCase(keyName, filter))
                                continue;

                            localRecipeDictionary.Add(keyName, recipe);
                            availableRecipesOptions.Add(keyName);
                        }
                    }
                    else
                    {
                        foreach (EnchantmentRecipe recipe in availableRecipes)
                        {
                            keyName = EnchantmentsHelper.GetUniqueEnchantmentsName(recipe, localRecipeDictionary);

                            if (keyName == null || !EnchantmentsHelper.ContainsIgnoreCase(keyName, filter))
                                continue;

                            if(string.Equals(keyName, previousValue, StringComparison.OrdinalIgnoreCase))
                            {
                                selectionValue = availableRecipesOptions.Count;
                            }

                            localRecipeDictionary.Add(keyName, recipe);
                            availableRecipesOptions.Add(keyName);
                        }
                    }

                    return (availableRecipesOptions, localRecipeDictionary, selectionValue);
                })
                .ContinueWith(t =>
                {
                    var (availableRecipesOptions, localRecipeDictionary, selectionValue) = t.Result;

                    if (ChooseEnchantmentDropdown != null)
                    {
                        EnchantmentRecipeDictionary = localRecipeDictionary;
                        FillDropdownChoices(ChooseEnchantmentDropdown, availableRecipesOptions);

                        ChooseEnchantmentDropdown.value = selectionValue;
                        ChooseEnchantmentDropdown.RefreshShownValue();
                        ChooseEnchantmentDropdown.onValueChanged.Invoke(ChooseEnchantmentDropdown.value);

                        this.ResultMessage("Finished Loading Enchantments and Equipment!");

                        ChooseEnchantmentDropdown.interactable = true;
                        ChooseItemDropdown.interactable = true;
                    }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void FillItemsData()
        {
            try
            {
                this.ResultMessage("Loading Enchantments . . .");

                ChooseEnchantmentDropdown.interactable = false;
                ChooseItemDropdown.interactable = false;

                Task.Run(() =>
                    {
                    List<string> dropdownOptions = new List<string>();
                    Dictionary<string, Equipment> localEquipmentDictionary = new Dictionary<string, Equipment>();
                    string keyName = "";

                    foreach (Equipment equipment in AvailableEquipment)
                    {
                        #if DEBUG
                        if(equipment == null)
                            OutwardEnchanter.LogMessage("GUIMainCanvasManager@FillItemsData equipment is null!");
                        #endif

                        keyName = ItemsHelper.GetUniqueEquipmentsName(equipment, localEquipmentDictionary);

                        dropdownOptions.Add(keyName);
                        localEquipmentDictionary.Add(keyName, equipment);

                        #if DEBUG
                            OutwardEnchanter.LogMessage($"GUIMainCanvasManager@FillItemsData equipment keyName added: {keyName}!");
                        #endif
                    }

                    return (dropdownOptions, localEquipmentDictionary);
                })
                .ContinueWith(t =>
                {
                    var (dropdownOptions, localEquipmentDictionary) = t.Result;

                    #if DEBUG
                    OutwardEnchanter.LogMessage($"GUIMainCanvasManager@FillItemsData AvailableEquipment: {AvailableEquipment.Count} " +
                        $"options: {dropdownOptions.Count} !");
                    #endif

                    EquipmentDictionary = localEquipmentDictionary;
                    AvailableEquipmentCount = localEquipmentDictionary.Count;
                    FillDropdownChoices(ChooseItemDropdown, dropdownOptions);

                    ChooseItemDropdown.value = 0;
                    ChooseItemDropdown.RefreshShownValue();
                    ChooseItemDropdown.onValueChanged.Invoke(ChooseItemDropdown.value);

                    this.ResultMessage("Finished Loading Enchantments!");

                    ChooseEnchantmentDropdown.interactable = true;
                    ChooseItemDropdown.interactable = true;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex) 
            {
                OutwardEnchanter.LogMessage("GUIMainCanvasManager@FillItemsData error: " + ex.Message);
            }
        }

        public void FillDropdownChoices(Dropdown dropdown, List<string> options)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        public void ResultAndLogMessage(string message)
        {
            OutwardEnchanter.LogMessage(message);
            ResultMessage(message);
        }

        public void ResultMessage(string message)
        {
            if(ResultText == null)
            {
                OutwardEnchanter.LogMessage($"Tried to display result message: \"{message}\" on null UI Result reference");
                return;
            }

            ResultText.text = message;
        }

        public void HideCanvas()
        {
            this.gameObject.SetActive(false);
            ForceUnlockCursor.RemoveUnlockSource();
        }
        
        public void ShowCanvas() 
        {
            this.gameObject.SetActive(true);
            ForceUnlockCursor.AddUnlockSource();
        }

        public Item SpawnEquipment(Equipment equipment)
        {
            Character localCharacter = CharacterEnchanterManager.Instance.MainCharacter;

            Vector3 vector = localCharacter.CenterPosition + localCharacter.transform.forward * 1.5f;
            Quaternion localRotation = localCharacter.transform.localRotation;

            Item item = ItemManager.Instance.GenerateItemNetwork(equipment.ItemID);
            item.transform.position = vector;
            item.transform.rotation = localRotation;

            MultipleUsage component = equipment.GetComponent<MultipleUsage>();
            if (component != null)
            {
            	item.GetComponent<MultipleUsage>().RemainingAmount = 1;
            }
            item.gameObject.AddComponent<SafeFalling>();

            return item;
        }
    }
}
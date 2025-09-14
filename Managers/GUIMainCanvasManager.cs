using Mono.Cecil;
using OutwardEnchanter.Helpers;
using SideLoader.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            FilterEnchantmentsData(text);           
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
                this.ResultAndLogMessage($"Couldn't generate equipment options in dropdown! Available equipment count: {AvailableEquipment.Count}");
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
                FilterEnchantmentsData(EnchantmentFilterInput.text);
            }
            catch(Exception ex) 
            {
                OutwardEnchanter.LogMessage("GUIMainCanvasManager@HandleOnChooseItemChange error: " + ex.Message);
            }
        }

        public string GetUniqueEnchantmentsName(EnchantmentRecipe recipe)
        {
            Enchantment enchantment = null;
            string keyName = "";

            keyName = recipe.name;

            if(keyName == "")
            {
                enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(recipe.RecipeID);

                if(enchantment == null)
                {
                    return null;
                }

                keyName = enchantment.PresetID + "_" + enchantment.Name;
            }

            //some modders included duplicate names
            if(EnchantmentRecipeDictionary.TryGetValue(keyName, out EnchantmentRecipe foundEnchantment))
            {
                keyName += "_" + Guid.NewGuid();
            }

            return keyName;
        }

        public string GetUniqueEquipmentsName(Equipment equipment)
        {
            string keyName = "";

            keyName = equipment.ItemID + "_" + equipment.Name.Replace(" ", "_");

            //some modders included duplicate names
            if(EquipmentDictionary.TryGetValue(keyName, out Equipment foundEquipment))
            {
                keyName += "_" + Guid.NewGuid();
            }

            return keyName;
        }

        public void FilterItemsData(string filter)
        {
            List<string> dropdownOptions = new List<string>();
            string keyName = "";
            EquipmentDictionary = new Dictionary<string, Equipment>();

            foreach (Equipment equipment in AvailableEquipment)
            {
                keyName = GetUniqueEquipmentsName(equipment);

                if (EnchantmentsHelper.ContainsIgnoreCase(keyName, filter))
                {
                    dropdownOptions.Add(keyName);
                    EquipmentDictionary.Add(keyName, equipment);
                }
            }

            FillDropdownChoices(ChooseItemDropdown, dropdownOptions);

            ChooseItemDropdown.value = 0;
            ChooseItemDropdown.RefreshShownValue();
            ChooseItemDropdown.onValueChanged.Invoke(ChooseItemDropdown.value);
        }

        public void FilterEnchantmentsData(string filter)
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

            List<EnchantmentRecipe> availableRecipes = EnchantmentsHelper.GetAvailableEnchantmentRecipies(equipment);
            EnchantmentRecipeDictionary = new Dictionary<string, EnchantmentRecipe>();
            List<string> availableRecipesOptions = new List<string>();
            string keyName = "";

            //Default spawn item option
            EnchantmentRecipeDictionary.Add("None", new EnchantmentRecipe());
            availableRecipesOptions.Add("None");

            foreach (EnchantmentRecipe recipe in availableRecipes)
            {
                keyName = GetUniqueEnchantmentsName(recipe);

                if (keyName == null || !EnchantmentsHelper.ContainsIgnoreCase(keyName, filter))
                    continue;

                EnchantmentRecipeDictionary.Add(keyName, recipe);
                availableRecipesOptions.Add(keyName);
            }

            if (ChooseEnchantmentDropdown != null)
                FillDropdownChoices(ChooseEnchantmentDropdown, availableRecipesOptions);
        }

        public void FillItemsData()
        {
            try
            {
                List<string> dropdownOptions = new List<string>();
                EquipmentDictionary = new Dictionary<string, Equipment>();
                string keyName = "";

                foreach (Equipment equipment in AvailableEquipment)
                {
                    #if DEBUG
                    if(equipment == null)
                        OutwardEnchanter.LogMessage("GUIMainCanvasManager@FillItemsData equipment is null!");
                    #endif

                    keyName = GetUniqueEquipmentsName(equipment);

                    dropdownOptions.Add(keyName);
                    EquipmentDictionary.Add(keyName, equipment);

                    #if DEBUG
                        OutwardEnchanter.LogMessage($"GUIMainCanvasManager@FillItemsData equipment keyName added: {keyName}!");
                    #endif
                }

                #if DEBUG
                OutwardEnchanter.LogMessage($"GUIMainCanvasManager@FillItemsData AvailableEquipment: {AvailableEquipment.Count} " +
                    $"options: {dropdownOptions.Count} !");
                #endif
                FillDropdownChoices(ChooseItemDropdown, dropdownOptions);

                ChooseItemDropdown.value = 0;
                ChooseItemDropdown.RefreshShownValue();
                ChooseItemDropdown.onValueChanged.Invoke(ChooseItemDropdown.value);
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
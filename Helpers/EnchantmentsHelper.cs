using OutwardEnchanter.Managers;
using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OutwardEnchanter.Helpers
{
    public class EnchantmentsHelper
    {
        public static bool ContainsIgnoreCase(string source, string toCheck)
        {
            return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static List<EnchantmentRecipe> GetAvailableEnchantmentRecipies(Item item)
        {
            List<EnchantmentRecipe> enchantmentRecipes = RecipeManager.Instance.GetEnchantmentRecipes();
            List<EnchantmentRecipe> availableEnchantments = new List<EnchantmentRecipe>();

            foreach (EnchantmentRecipe enchantmentRecipe in enchantmentRecipes)
            {
                if (enchantmentRecipe.GetHasMatchingEquipment(item))
                {
                    #if DEBUG
                    OutwardEnchanter.LogMessage($"EnchantmentsHelper@GetAvailableEnchantmentRecipies equiment {item.Name} can be enchanted with {enchantmentRecipe.name}");
                    #endif
                    availableEnchantments.Add(enchantmentRecipe);
                }
            }

            return availableEnchantments;
        }

        /// <summary>
        /// Under this are used method to test enchanting process
        /// </summary>
        /// <param name="stats"></param>
        //for testing
        public static void RefreshEnchantmentStatModifications(EquipmentStats stats)
        {

            OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications called!");
            stats.m_statModifications.Clear();
            Equipment equipment = stats.m_item as Equipment;
            float num = 0f;
            float num2 = 0f;
            int num3 = 0;
            float num4 = 1f;
            bool flag = false;
            stats.m_totalStatusEffectResistance = stats.m_baseStatusEffectResistance;
            if (stats.m_totalDamageBonus == null)
            {
                stats.m_totalDamageBonus = new float[stats.m_damageAttack.Length];
            }
            stats.m_damageAttack.CopyTo(stats.m_totalDamageBonus, 0);
            OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 1!");
            //??new float[9];
            stats.m_bonusDamageProtection[0] = 0f;
            stats.m_bonusDamageResistance = new float[9];
            if (equipment)
            {
            OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 2!");
                for (int i = 0; i < equipment.ActiveEnchantments.Count; i++)
                {
                    Enchantment.StatModificationList statModifications = equipment.ActiveEnchantments[i].StatModifications;
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 3!");
                    stats.m_statModifications.AddRange(statModifications);
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 4!");
                    stats.m_totalStatusEffectResistance += equipment.ActiveEnchantments[i].GlobalStatusResistance;
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 5!");
                    num += statModifications.GetBonusValue(Enchantment.Stat.Weight);
                    num2 += statModifications.GetModifierValue(Enchantment.Stat.Weight);
                    num3 += (int)statModifications.GetBonusValue(Enchantment.Stat.Durability);
                    num4 += statModifications.GetModifierValue(Enchantment.Stat.Durability);
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 6!");
                    if (equipment.ActiveEnchantments[i].Indestructible)
                    {
                        flag = true;
                    }
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 7!");
                    foreach (DamageType damageType in equipment.ActiveEnchantments[i].ElementalResistances.List)
                    {
                        stats.m_bonusDamageResistance[(int)damageType.Type] += damageType.Damage;
                    }
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 8!");
                    foreach (DamageType damageType2 in equipment.ActiveEnchantments[i].DamageModifier.List)
                    {
                        stats.m_totalDamageBonus[(int)damageType2.Type] += damageType2.Damage;
                    }
                    OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 9!");
                    stats.m_bonusDamageProtection[0] += statModifications.GetBonusValue(Enchantment.Stat.Protection);
                }
            }
            OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 10!");
            stats.m_realWeight = Mathf.Clamp(stats.RawWeight * (1f + num2 * 0.01f) + num, 0.1f, float.MaxValue);
            if (flag)
            {
                stats.m_realDurability = -1;
                stats.m_item.ResetDurabiltiy();
                return;
            }
            OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 11!");
            stats.m_realDurability = ((stats.m_baseMaxDurability != -1) ? (Mathf.RoundToInt((float)stats.m_baseMaxDurability * num4) + num3) : -1);
            OutwardEnchanter.LogMessage("GUIMainCanvasManager@RefreshEnchantmentStatModifications 12!");
            if (stats.m_realDurability < 1 && stats.m_baseMaxDurability != -1)
            {
                stats.m_realDurability = 1;
            }
        }

        //for testing only
        public static void FindEnchantingBugs(Item item, EnchantmentRecipe enchantmentRecipe)
        {
            if(!(item is Equipment newEquipment))
            {
                return;
            }
            GUIMainCanvasManager canvasManager = GUIManager.Instance.MainCanvasManager;
            Enchantment enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantmentRecipe.RecipeID);
            OutwardEnchanter.LogMessage("got enchantment!");

            if (enchantment == null)
            {
                canvasManager.ResultAndLogMessage("Couldn't find Enchantment through ResourcesPrefabManager@GetEnchantmentPrefab!");
                return;
            }

            OutwardEnchanter.LogMessage("before enchanting!");

            if(newEquipment == null)
            {
                OutwardEnchanter.LogMessage("You dont have and equipment derived class!");
            }

            if (!newEquipment.m_enchantmentIDs.Contains(enchantment.PresetID))
            {
                Enchantment enchantmentTest = ResourcesPrefabManager.Instance.GenerateEnchantment(enchantment.PresetID, item.transform);
                OutwardEnchanter.LogMessage("before enchanting!1");
                if (enchantmentTest)
                {
                    OutwardEnchanter.LogMessage("before enchanting!1.5");
                    enchantmentTest.ApplyEnchantment(newEquipment);
                    OutwardEnchanter.LogMessage("before enchanting!2");
                    EnchantmentRecipe enchantmentRecipeForID = RecipeManager.Instance.GetEnchantmentRecipeForID(enchantment.PresetID);
                    OutwardEnchanter.LogMessage("before enchanting!3");
                    newEquipment.m_enchantmentIDs.Add(enchantment.PresetID);
                    OutwardEnchanter.LogMessage("before enchanting!4");
                    newEquipment.m_activeEnchantments.Add(enchantmentTest);
                    OutwardEnchanter.LogMessage("before enchanting!5");
                    newEquipment.m_enchantmentsHaveChanged = true;
                    OutwardEnchanter.LogMessage("before enchanting!6");

                    if(item.Stats != null)
                        OutwardEnchanter.LogMessage("item have stats!");

                    if(newEquipment.Stats != null)
                    {
                        OutwardEnchanter.LogMessage("equipment have stats!");

                        if (newEquipment.ActiveEnchantments != null)
                        {
                            if (newEquipment.ActiveEnchantments[0].StatModifications == null)
                                OutwardEnchanter.LogMessage("trying to access missing stat modifications of enchantment!");
                        }
                        else
                            OutwardEnchanter.LogMessage("missing active enchantments!");

                        RefreshEnchantmentStatModifications(newEquipment.Stats);
                    }
                    else
                    {
                        OutwardEnchanter.LogMessage("equipment doesn't have stats!");
                        if (newEquipment is Weapon weapon)
                        {
                            weapon.m_stats = newEquipment.GetComponent<WeaponStats>();

                            if (weapon.m_stats == null)
                                OutwardEnchanter.LogMessage("weapon failed to add stats!");
                        }
                        else
                        {
                            newEquipment.m_stats = newEquipment.GetComponent<EquipmentStats>();

                            if (newEquipment.m_stats == null)
                                OutwardEnchanter.LogMessage("armor failed to add stats!");
                        }

                        if (item.m_stats == null)
                            OutwardEnchanter.LogMessage("still no stats!");
                        else
                        {
                            if(item.m_stats.m_item == null)
                            {
                                OutwardEnchanter.LogMessage("Can't find item on stats?!");
                                item.Start();

                                if(item.m_stats.m_item == null)
                                {
                                    OutwardEnchanter.LogMessage("Can't find item on stats?! recursion");
                                    item.StartInit();
                                }
                            }
                            RefreshEnchantmentStatModifications(newEquipment.Stats);
                        }
                    }
                    newEquipment.RefreshEnchantmentModifiers();
                    OutwardEnchanter.LogMessage("before enchanting!7");
                }
            }
        }

        public static string GetUniqueEnchantmentsName(EnchantmentRecipe recipe, Dictionary<string, EnchantmentRecipe>EnchantmentRecipeDictionary)
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


        public static void SetItemAsGenerated(Item item)
        {
            item.IsGenerated = true;
            item.ClientGenerated = true;
            string addedSepartion = string.IsNullOrWhiteSpace(item.Description) ? "" : item.Description + "\n\n";
            item.m_localizedDescription = addedSepartion + "Item is generated using GymMed Enchanter mod!";
        }
    }
}

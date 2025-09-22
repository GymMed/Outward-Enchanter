using OutwardEnchanter.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OutwardEnchanter.Helpers
{
    public class ItemsHelper
    {
        public static List<Equipment> GetEquipmentsByEnchantmentName(string filter, List<Equipment>availableEquipment, Dictionary<string, EnchantmentRecipe> EnchantmentDictionary)
        {
            List<Equipment> finalEquipments = new List<Equipment>();
            string keyName = "";
            
            foreach(Equipment item in availableEquipment)
            {
                List<EnchantmentRecipe> availableEnchantments = EnchantmentsHelper.GetAvailableEnchantmentRecipies(item);

                foreach(EnchantmentRecipe currentEnchantment in availableEnchantments)
                {
                    keyName = EnchantmentsHelper.GetUniqueEnchantmentsName(currentEnchantment, EnchantmentDictionary);

                    if (keyName == null || !EnchantmentsHelper.ContainsIgnoreCase(keyName, filter))
                        continue;

                    finalEquipments.Add(item);
                    break;
                }
            }

            return finalEquipments;
        }

        public static string GetUniqueEquipmentsName(Equipment equipment, Dictionary<string, Equipment> EquipmentDictionary)
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
    }
}

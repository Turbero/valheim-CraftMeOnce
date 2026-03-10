using HarmonyLib;
using UnityEngine;

namespace CraftMeOnce
{
	[HarmonyPatch(typeof(InventoryGui), "Show")]
	public static class InventoryGuiRepairAllItemsText
	{
		[HarmonyPostfix]
		public static void Postfix(InventoryGui __instance)
		{
			InventoryGui.instance.transform.Find("root/Crafting/RepairButton").GetComponent<UITooltip>().m_text =
				ConfigurationFile.repairAll.Value == ConfigurationFile.Toggle.On
					? ConfigurationFile.repairAllItemsText.Value
					: "$inventory_repairbutton";
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnRepairPressed")]
	public static class InventoryGuiRepairAllItemsClick
	{
		[HarmonyPostfix]
		public static void Postfix(InventoryGui __instance)
		{
			if (ConfigurationFile.repairAll.Value == ConfigurationFile.Toggle.Off)
				return;
			
			CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
			if (currentCraftingStation != null)
			{
				int numRepaired = 0;
				while ((bool)GameManager.GetPrivateMethod(__instance, "HaveRepairableItems"))
				{
					GameManager.CallPrivateMethod(__instance, "RepairOneItem");
					numRepaired++;
				}
				if (numRepaired > 0)
				{
					currentCraftingStation.m_repairItemDoneEffects.Create(currentCraftingStation.transform.position, Quaternion.identity);
				}
			}
		}
	}
}
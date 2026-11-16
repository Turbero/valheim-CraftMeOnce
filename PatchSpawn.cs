using HarmonyLib;

namespace CraftMeOnce
{
    [HarmonyPatch(typeof(Player), "OnSpawned")]
    public static class Player_OnSpawned
    {
        static void Postfix(bool spawnValkyrie)
        {
            //Workstations
            Logger.Log("Player_OnSpawned");
            Caching.AddItemDrops();
            
            //Hammer menu
            Achievement achievement = Achievements.m_instance.m_achievementLists[0].m_achievements
                .Find(achList => achList.name.Equals("AllBuildPieces"));
            foreach (Achievement.DictStatRequirement dicStatReq in achievement.m_piecePlacedTriggers)
            {
                string pieceNameAllLang = dicStatReq.m_stat;
                PlayerProfile playerProfile = (PlayerProfile)GameManager.GetPrivateValue(Game.instance, "m_playerProfile");
                if (dicStatReq.IsMet(playerProfile.m_playerStats[(int) DifficultyRequirement.Any].m_piecesPlacedStats, DifficultyRequirement.Any))
                    if (!GameManager.HasPieceEverBeenCrafted(pieceNameAllLang))
                        GameManager.SetPieceEverBeenCrafted(pieceNameAllLang);
            }
        }
    }
    
    [HarmonyPatch(typeof(Localization), "SetLanguage")]
    public class Localization_SetLanguage_Patch
    {
        static void Postfix(string language)
        {
            Logger.Log($"Language changed to: {language}");
            Caching.AddItemDrops();
        }
    }
}
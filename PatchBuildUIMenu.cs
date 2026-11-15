using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CraftMeOnce
{
    [HarmonyPatch(typeof(BuildUiPieceButton), "Setup")]
    public class BuildUiPieceButtonPatch
    {

        static void Postfix(BuildUiPieceButton __instance, Piece pieceInfo, BuildUi buildUi)
        {
            Logger.Log("BuildUiPieceButtonPatch - Postfix");

            string pieceName = pieceInfo.name.Replace("(Clone)", "");
            if (pieceName.Equals("piece_repair")) return;

            Logger.Log("Piece name: "+pieceName);
            GameObject buildPieceSelectButtonClone = __instance.gameObject;
            GameObject hintMarkInBuildPiece = buildPieceSelectButtonClone.transform.Find("Hint Mark")?.gameObject;
            if (hintMarkInBuildPiece == null)
            {
                hintMarkInBuildPiece = new GameObject("Hint Mark", typeof(RectTransform), typeof(TextMeshProUGUI));
                hintMarkInBuildPiece.transform.SetParent(buildPieceSelectButtonClone.transform, false);
                hintMarkInBuildPiece.transform.SetAsLastSibling();
                hintMarkInBuildPiece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -20);
                hintMarkInBuildPiece.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
                TextMeshProUGUI enrichedText = hintMarkInBuildPiece.GetComponent<TextMeshProUGUI>();
                enrichedText.font = GameManager.getFontAsset("Valheim-AveriaSerifLibre");
                enrichedText.fontStyle = FontStyles.Normal;
                enrichedText.fontSize = 16;
                enrichedText.fontSizeMax = 16;
                enrichedText.fontSizeMin = 1;
                enrichedText.alignment = TextAlignmentOptions.Center;
                Logger.Log("hintMarkInBuildPiece created for "+pieceName);
            }
            
            bool hasPieceEverBeenCrafted = GameManager.HasPieceEverBeenCrafted(pieceName);
            Logger.Log("pieceName "+pieceName+" ever crafted? "+hasPieceEverBeenCrafted);
            hintMarkInBuildPiece.GetComponent<TextMeshProUGUI>().text = hasPieceEverBeenCrafted 
                ? "" 
                : "<color=yellow>" + ConfigurationFile.characterForNotCraftedItems.Value + "</color>";
        }
    }
    
    [HarmonyPatch(typeof(Player), "TryPlacePiece")]
    public class TryPlacePiecePatch
    {
        static void Postfix(Player __instance, Piece piece)
        {
            Logger.Log("TryPlacePiecePatch - Postfix");
            
            string pieceName = piece.name.Replace("(Clone)", "");
            bool hasPieceEverBeenCrafted = GameManager.HasPieceEverBeenCrafted(pieceName);
            if (!hasPieceEverBeenCrafted)
            {
                GameManager.SetPieceEverBeenCrafted(pieceName);
            }
        }
    }
}
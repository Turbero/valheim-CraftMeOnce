using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CraftMeOnce
{
    [HarmonyPatch(typeof(BuildUiPieceButton), "Setup")]
    public class BuildUiPieceButtonPatch
    {
        private static GameObject hammerHintButtonGo;
        public static Button hammerHintButton;
        private static TextMeshProUGUI hammerButtonText;

        public static void Postfix(BuildUiPieceButton __instance, Piece pieceInfo, BuildUi buildUi)
        {
            Logger.Log("BuildUiPieceButtonPatch - Postfix");
            if (hammerHintButtonGo == null || hammerHintButton == null || hammerButtonText == null)
            {
                createHammerHintButton();
            }
            updateHammerHintButton();

            string pieceName = pieceInfo.name.Replace("(Clone)", "");
            Logger.Log("Piece name: " + pieceName);
            Logger.Log("Piece m_name: " + pieceInfo.m_name);
            if (pieceName.Equals("piece_repair")) return;

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
                Logger.Log("hintMarkInBuildPiece created for " + pieceName);
            }

            if (ConfigurationFile.hammerHintMode.Value == ConfigurationFile.HintMode.Off) {
                hintMarkInBuildPiece.GetComponent<TextMeshProUGUI>().text = "";
                return;
            }

            bool hasPieceEverBeenCrafted = GameManager.HasPieceEverBeenCrafted(pieceInfo.m_name);
            Logger.Log("pieceName "+pieceName+" ever crafted? "+hasPieceEverBeenCrafted);
            hintMarkInBuildPiece.GetComponent<TextMeshProUGUI>().text = hasPieceEverBeenCrafted 
                ? "" 
                : "<color=yellow>" + ConfigurationFile.hammerCharacterForNotObtainedItems.Value + "</color>";
        }
        
        private static void createHammerHintButton()
        {
            Transform copyButton = InventoryGui.instance.m_skillsDialog.transform.Find("SkillsFrame/Closebutton");
            Transform parent = Hud.instance.m_buildUi.gameObject.transform;
            hammerHintButtonGo = Object.Instantiate(copyButton.gameObject, parent);
            hammerHintButtonGo.name = "HammerHintButton";
            
            hammerButtonText = hammerHintButtonGo.GetComponentInChildren<TextMeshProUGUI>();
            hammerButtonText.font = GameManager.getFontAsset("Valheim-AveriaSerifLibre");
            hammerButtonText.fontStyle = FontStyles.Normal;
            hammerButtonText.alignment = TextAlignmentOptions.Center;

            hammerHintButton = hammerHintButtonGo.GetComponent<Button>();
            hammerHintButton.onClick = new Button.ButtonClickedEvent();
            hammerHintButton.onClick.AddListener(() =>
            {
                ConfigurationFile.hammerHintMode.Value =
                    ConfigurationFile.hammerHintMode.Value == ConfigurationFile.HintMode.Off
                        ? ConfigurationFile.HintMode.On
                        : ConfigurationFile.HintMode.Off;
                //The config reload will run the reload after the previous line
            });
        }
        
        public static void updateHammerHintButton()
        {
            hammerHintButton.GetComponent<RectTransform>().anchoredPosition = ConfigurationFile.hammerBtnPosition.Value;
            hammerHintButton.GetComponent<RectTransform>().sizeDelta = ConfigurationFile.hammerBtnSize.Value;
            hammerButtonText.text = ConfigurationFile.hammerCharacterForNotObtainedItems.Value;
            hammerButtonText.color = ConfigurationFile.hammerHintMode.Value != ConfigurationFile.HintMode.Off ? Color.yellow : Color.gray;
            GameManager.BindGamePad(hammerHintButton.gameObject.transform, ConfigurationFile.hammerBtnGamepadKey.Value, new Vector2(ConfigurationFile.hammerBtnSize.Value.x - 1, 0));
            
            hammerHintButton.gameObject.SetActive(
                ConfigurationFile.modEnabled.Value == ConfigurationFile.Toggle.On &&
                ConfigurationFile.hammerHintMode.Value != ConfigurationFile.HintMode.Always);
        }
    }
    
    [HarmonyPatch(typeof(Player), "TryPlacePiece")]
    public class TryPlacePiecePatch
    {
        static void Postfix(Player __instance, Piece piece)
        {
            Logger.Log("TryPlacePiecePatch - Postfix");
            
            bool hasPieceEverBeenCrafted = GameManager.HasPieceEverBeenCrafted(piece.m_name);
            if (!hasPieceEverBeenCrafted)
            {
                GameManager.SetPieceEverBeenCrafted(piece.m_name);
            }
        }
    }
}
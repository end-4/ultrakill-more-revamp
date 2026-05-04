using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

namespace MoreRevamp {
    [BepInPlugin("com.github.end-4.moreRevamp", "MoreRevamp", "1.0.3")]
    public class Plugin : BaseUnityPlugin {
        internal static ManualLogSource Log;

        private static Sprite _largeBorderSprite = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_BorderLarge.png").WaitForCompletion();

        private static Sprite _smallBorderSprite = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_BorderSmall.png").WaitForCompletion();

        private static Sprite _largeFillSprite = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_FillLarge.png").WaitForCompletion();

        private static Sprite _smallFillSprite = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_FillSmall.png").WaitForCompletion();

        private static Sprite _crossSprite = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Check.png").WaitForCompletion();

        private static Sprite _dropdownBorderSprite = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_DropdownPanel.png").WaitForCompletion();

        private static readonly float PixelsPerUnitMultiplier = 5.4f;

        public static string GetObjectPath(GameObject g) {
            string path = g.name;
            while (g.transform.parent != null) {
                g = g.transform.parent.gameObject;
                path = g.name + "/" + path;
            }

            return path;
        }

        private void Awake() {
            Log = Logger;

            // Apply Harmony Patches
            var harmony = new Harmony("MoreRevamp");
            harmony.PatchAll();

            Log.LogInfo("Patches applied");
        }

        private static void ApplyImageSprite(Image image, Sprite sourceSprite) {
            RectTransform rect = image.rectTransform;
            Vector2 originalSize = rect.sizeDelta;
            Color originalColor = image.color;

            image.sprite = sourceSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = PixelsPerUnitMultiplier;

            rect.sizeDelta = originalSize;
            image.color = originalColor;
        }

        public static void ApplyLargeBorder(Image image) {
            ApplyImageSprite(image, _largeBorderSprite);
        }

        public static void ApplySmallBorder(Image image) {
            ApplyImageSprite(image, _smallBorderSprite);
        }

        public static void ApplyLargeFill(Image image) {
            ApplyImageSprite(image, _largeFillSprite);
        }

        public static void ApplySmallFill(Image image) {
            ApplyImageSprite(image, _smallFillSprite);
        }

        public static void ApplyDropdownBorder(Image image) {
            ApplyImageSprite(image, _dropdownBorderSprite);
        }

        public static void ApplyCross(Image image) {
            ApplyImageSprite(image, _crossSprite);
        }

        public static void ForceUIColor(Image image) {
            image.color = Color.white;
        }

        public static void ChangeSize(Image image, float width, float height) {
            image.rectTransform.sizeDelta = new Vector2(width, height);
        }

        public static void ChangeAnchor(Image image, float x, float y) {
            image.rectTransform.anchoredPosition = new Vector2(x, y);
        }

        public static void ApplyUIComponentColors(Selectable d) {
            ColorBlock colors = d.colors;
            colors.normalColor = new Color32(255, 255, 255, 255);
            colors.selectedColor = new Color32(130, 130, 130, 255);
            colors.pressedColor = new Color32(255, 0, 0, 255);
            d.colors = colors;
        }

        public static void ForceRGBAmount(Image image, float amount, float hasColorThreshold = 0) {
            float r = image.color.r > hasColorThreshold ? amount : 0;
            float g = image.color.g > hasColorThreshold ? amount : 0;
            float b = image.color.b > hasColorThreshold ? amount : 0;
            float a = image.color.a;
            image.color = new Color(r, g, b, a);
        }

        public static void HideImage(Image image) {
            image.color = Color.clear;
        }
    }

    [HarmonyPatch(typeof(Image), "OnEnable")]
    public static class ImagePatch {
        [HarmonyPostfix]
        public static void Postfix(Image __instance) {
            // Get game object path
            string path = Plugin.GetObjectPath(__instance.gameObject);
            string lName = __instance.name.ToLower();

            // Some conditions
            bool isButton = __instance.GetComponent<Button>() != null;
            bool isTmpDropdown = __instance.GetComponent<TMP_Dropdown>() != null;
            bool isDropdown = __instance.GetComponent<Dropdown>() != null;
            bool isInput = __instance.GetComponent<TMP_InputField>() != null;

            bool inVanillaThankScreenButton = path.Contains("Skippables");
            bool inGenericPluginConfLocations = path.Contains("ConcretePanel(Clone)") ||
                                                path.Contains("PresetPanel(Clone)") ||
                                                path.Contains("PluginConfigField");
            bool inAngryLeaderboard = path.Contains("AngryLeaderboardNotification");

            bool isConfiggyBorder = __instance.name == "Border" && (path.Contains("ConfigurationMenu(Clone)") || path.Contains("UI_Button_Image"));
            bool isPluginConfFieldBox = __instance.mainTexture.name == "UISprite";
            bool isPluginConfPresetButton = __instance.name == "PresetButton(Clone)";
            bool isPluginConfTextField = inGenericPluginConfLocations && __instance.name == "InputField";
            bool isPluginConfTmpDropdownBg = __instance.name == "Dropdown";
            bool isPluginConfDropdownBg = __instance.name == "Dropdown" || __instance.name == "DifficultyDropdown" ||
                                          __instance.name == "GamemodeDropdown";
            bool isPluginConfDropdownSelectionItemBg =
                path.Contains("Viewport") && __instance.name == "Item Background";
            bool isPluginConfDropdownSelectionBg =
                (path.Contains("DropdownField(Clone)/Dropdown/") || path.Contains("Dropdown/Dropdown") || __instance.name == "Dropdown List") &&
                !lName.Contains("checkmark");
            bool isPluginConfDropdownArrow =
                (path.Contains("DropdownField") || path.Contains("GamemodeDropdown") ||
                 path.Contains("DifficultyDropdown")) && __instance.name == "Arrow";
            bool isPluginConfCheckbox = inGenericPluginConfLocations && path.Contains("Toggle") &&
                                        __instance.name == "Background";
            bool isPluginConfCheckmark = inGenericPluginConfLocations && path.Contains("Toggle/Background") &&
                                         __instance.name == "Checkmark";
            bool isPluginConfColor = inGenericPluginConfLocations && path.Contains("ColorField(Clone)") &&
                                     __instance.name == "Image";
            bool isPluginConfColorSliderBg = inGenericPluginConfLocations && path.Contains("ColorField(Clone)") &&
                                             path.Contains("Button/Slider") &&
                                             __instance.name == "Background";
            bool isPluginConfColorSliderFill = inGenericPluginConfLocations && path.Contains("ColorField(Clone)") &&
                                               path.Contains("Slider/Fill Area") &&
                                               __instance.name == "Fill";
            bool isPluginConfColorSliderHandle =
                inGenericPluginConfLocations && path.Contains("ColorField(Clone)") &&
                path.Contains("Slider/Handle Slide Area") &&
                __instance.name == "Handle";
            bool isAngryVoteArrow = lName.Contains("upvote") || lName.Contains("downvote");
            bool isAngryThumbnail = lName.Contains("thumbnail");
            bool isAngryFav = __instance.name.StartsWith("FavButton");
            bool isAngryBundleSortBorder = (lName.StartsWith("button") || lName.StartsWith("frame")) && path.Contains("BundleSortField");
            bool isAngryBundleSortBg = lName.StartsWith("bg") && path.Contains("BundleSortField");
            bool isAngrySearchBar = __instance.name == "SearchBar(Clone)";
            bool isAngryRankBox = __instance.name == "RankIcon(Clone)";

            // Avoid vanilla buttons
            if (__instance.GetComponent<HudOpenEffect>() != null &&
                !( // ...with Special EXceptions
                        __instance.GetComponent<Button>() != null && ( // These buttons
                            path.Contains("Skippables") // In thank you for playing screen
                        )
                    )
               ) return;

            // Blacklist
            if (isAngryVoteArrow || isAngryThumbnail || isAngryFav) return;

            // Sprite image applications
            if (isConfiggyBorder
                || (isButton && (inGenericPluginConfLocations || inVanillaThankScreenButton
                                || inAngryLeaderboard || isPluginConfPresetButton))
                || (isTmpDropdown && isPluginConfTmpDropdownBg)
                || (isDropdown && isPluginConfDropdownBg)
                || (isInput && isPluginConfTextField)
                || ( // General objects
                    isPluginConfColorSliderBg || isAngrySearchBar
                    || isAngryRankBox || isAngryBundleSortBorder
                )
               ) {
                Plugin.ApplyLargeBorder(__instance);
            } else if (isPluginConfDropdownArrow || isPluginConfCheckbox) {
                Plugin.ApplySmallBorder(__instance);
            } else if (isPluginConfCheckmark) {
                Plugin.ApplyCross(__instance);
            } else if (isPluginConfFieldBox) {
                Plugin.ApplyLargeFill(__instance);
            } else if (isPluginConfColor || isPluginConfColorSliderFill
                || isPluginConfColorSliderHandle || isAngryBundleSortBg
               ) {
                Plugin.ApplySmallFill(__instance);
            } else if (isPluginConfDropdownSelectionBg) {
                Plugin.ApplyDropdownBorder(__instance);
            }

            // Force colors
            if (isPluginConfTmpDropdownBg || isPluginConfDropdownBg || isPluginConfDropdownArrow ||
                isPluginConfCheckbox || isPluginConfCheckmark ||
                isPluginConfTextField || isPluginConfDropdownSelectionBg) {
                Plugin.ForceUIColor(__instance);
            }

            // slider bg 0.67 handle 1 fill 0.33
            // Component-specifics
            if (isPluginConfCheckbox) {
                Plugin.ChangeSize(__instance, -10, -10);
            } else if (isPluginConfCheckmark) {
                Plugin.ChangeSize(__instance, -10, -10);
            } else if (isPluginConfTmpDropdownBg || (isInput && isPluginConfTextField) || isDropdown) {
                Selectable sel = __instance.GetComponent<Selectable>();
                Plugin.ApplyUIComponentColors(sel);
            } else if (isPluginConfDropdownArrow) {
                Plugin.ChangeSize(__instance, 11, 20);
                Plugin.ChangeAnchor(__instance, -10, 0);
            } else if (isPluginConfColorSliderBg) {
                Plugin.ChangeSize(__instance, 0, 10);
                Plugin.ForceRGBAmount(__instance, 0.67f, 0.27f);
            } else if (isPluginConfColorSliderFill) {
                Plugin.ChangeAnchor(__instance, 5, 0);
                Plugin.ForceRGBAmount(__instance, 0.33f, 0.27f);
            } else if (isPluginConfColorSliderHandle) {
                Plugin.ChangeSize(__instance, 10, -10);
                Plugin.ForceRGBAmount(__instance, 1f, 0.27f);
            } else if (isPluginConfDropdownSelectionBg) {
                RectTransform r = __instance.rectTransform;
                r.pivot = new Vector2(0, 1);
                r.sizeDelta = new Vector2(-7, 148);
            } else if (isPluginConfDropdownSelectionItemBg) {
                Plugin.HideImage(__instance);
            } else if (isAngryBundleSortBg) {
                Plugin.ChangeSize(__instance, -4, -4);
            }
        }
    }
}

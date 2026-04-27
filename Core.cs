using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace MoreRevamp {
    [BepInPlugin("com.github.end-4.moreRevamp", "MoreRevamp", "1.0.0")]
    public class Plugin : BaseUnityPlugin {
        internal static ManualLogSource Log;
        public static Sprite? SourceSprite;
        public static float SourcePPU;
        public static string SOURCE_BUTTON_NAME = "Border";

        private void Awake() {
            Log = Logger;

            // Apply Harmony Patches
            var harmony = new Harmony("MoreRevamp");
            harmony.PatchAll();

            Log.LogInfo("Patches applied");
        }

        // Search for source sprite
        private void Update() {
            if (SourceSprite != null) return;
            Image? source = GameObject.Find(SOURCE_BUTTON_NAME)?.GetComponent<Image>();
            
            if (source == null || source.sprite == null) return;
            SourceSprite = source.sprite;
            SourcePPU = source.pixelsPerUnitMultiplier * 7 / 4; // title screen border is thicc
        }
    }

    [HarmonyPatch(typeof(Image), "OnEnable")]
    public static class ImagePatch {
        [HarmonyPostfix]
        public static void Postfix(Image __instance) {
            // If we haven't found the source sprite yet, skip
            if (Plugin.SourceSprite == null) return;


            // Get game object path
            GameObject obj = __instance.gameObject;
            string path = obj.name;
            while (obj.transform.parent != null) {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }

            // Check if this image should be styled
            // Avoid vanilla buttons
            if (__instance.GetComponent<HudOpenEffect>() != null &&
                !( // ...with Special EXceptions
                        __instance.GetComponent<Button>() != null && ( // These buttons
                            path.Contains("Skippables") // In thank you for playing screen
                        )
                    )
            ) return;
            // Blacklist
            if (
                __instance.name == "UpvoteButton" || __instance.name == "Downvote button" // Angry vote arrows
            ) return;
            // Whitelist
            if ((__instance.name == "Border" && path.Contains("ConfigurationMenu(Clone)")) // Configgy overlay
                || __instance.GetComponent<Button>() != null && ( // These buttons
                    path.Contains("ConcretePanel(Clone)") // PluginConfigurator
                    || path.Contains("PresetPanel(Clone)") // PluginConfigurator
                    || path.Contains("PluginConfigField") // PluginConfigurator
                    || path.Contains("Skippables") // PluginConfigurator
                )
                || ( // General objects
                    __instance.name == "RankIcon(Clone)" // Angry rank
                    || __instance.name == "SearchBar(Clone)" // Angry search
                )
            ) {
                // Apply style while preserving size & color
                RectTransform rect = __instance.rectTransform;
                Vector2 originalSize = rect.sizeDelta;
                Color originalColor = __instance.color;

                __instance.sprite = Plugin.SourceSprite;
                __instance.pixelsPerUnitMultiplier = Plugin.SourcePPU;

                rect.sizeDelta = originalSize;
                __instance.color = originalColor;
            }
        }
    }
}

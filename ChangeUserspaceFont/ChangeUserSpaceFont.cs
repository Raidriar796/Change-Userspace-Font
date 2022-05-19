using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using System;
using BaseX;
using System.Collections.Generic;


namespace ChangeUserspaceFont
{
    public class ChangeUserspaceFont : NeosMod
    {
        public override string Name => "ChangeUserSpaceFont";
        public override string Author => "Hayden";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/Hayden-Fluff/Change-Userspace-Font";

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> Enabled =
            new ModConfigurationKey<bool>("Enabled", "Enable Font Override", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<Uri> FontURL =
            new ModConfigurationKey<Uri>("FontURL", "Font URL", () => new Uri("Neosdb:///c801b8d2522fb554678f17f4597158b1af3f9be3abd6ce35d5a3112a81e2bf39.ttf"));
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<int> Padding =
            new ModConfigurationKey<int>("Padding", "Font Padding", () => 1);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<int> PixelRange =
            new ModConfigurationKey<int>("PixelRange", "Font PixelRange", () => 4);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<int> GlyphEmSize =
            new ModConfigurationKey<int>("GlyphEmSize", "Font GlyphEmSize", () => 32);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> TryHook =
            new ModConfigurationKey<bool>("TryHook", "Try Hook", () => false);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> DebugValues =
            new ModConfigurationKey<bool>("DebugValues", "Debug Values", () => false);

        private static ModConfiguration config;
    
        public override void OnEngineInit()
        {
            config = GetConfiguration();

            Harmony harmony = new Harmony("net.Hayden.ChangeUserspaceFont");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Userspace), "OnCommonUpdate")]
        class ChangeUserspaceFont_Patch
        {
            private static StaticFont? fontAsset;
            private static bool fontHooked = false;
            private static readonly Uri targetFontURL = config.GetValue(FontURL);
            private static readonly bool tryFontHook = config.GetValue(TryHook);

            //public static bool Prefix()
            //{
            //    if (config == null) return false; 
            //}

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FontChain), "OnAwake")]
            public static void Postfix(FontChain __instance)
            {
                if (config == null) return;

                Msg("FontChain in " + __instance.World.Name + " awoke on " + __instance.Slot.Name);
                if (config.GetValue(Enabled) && __instance.World == Userspace.UserspaceWorld && __instance.Slot == Userspace.UserspaceWorld.RootSlot)
                {
                    HookFont(__instance);
                }
            }

            public static bool HookFont(FontChain targetFontChain)
            {
                try
                {
                    //Find and set all properties on the MainFont StaticFont
                    fontAsset = (StaticFont?)targetFontChain.MainFont;
                    fontAsset.URL.Value = config.GetValue(FontURL);
                    fontAsset.Padding.Value = config.GetValue(Padding);
                    fontAsset.PixelRange.Value = config.GetValue(PixelRange);
                    fontAsset.GlyphEmSize.Value = config.GetValue(GlyphEmSize);

                    Msg("Successfully hooked into Font URL");
                    fontHooked = true;
                    config.Set(TryHook, false);
                    return true;
                }
                catch (Exception ex)
                {
                    Msg("Failed to hook to FontURL");
                    fontHooked = false;
                    Msg(ex.ToString());
                    config.Set(TryHook, false);
                    return false;
                }
            }


            public static void Postfix()
            {
                if (config == null) return;

                //if (config.GetValue(Enabled) && tryFontHook && !fontHooked)
                //{
                //    try
                //    {
                //        Slot _rootSlot = Userspace.UserspaceWorld.RootSlot;
                //        Msg(_rootSlot);
                //        FontChain _fontChain = (FontChain)_rootSlot.GetComponent("FrooxEngine.FontChain");
                //        Msg(_fontChain);
                //        StaticFont _fontAsset = (StaticFont)_fontChain.MainFont;
                //        Msg(_fontAsset);
                //        fontURL = _fontAsset.URL;
                //        fontURL.Value = targetFontURL;
                //        Msg(fontURL, fontURL.Value);
                //        Msg("Successfully hooked to Font URL");
                //        fontHooked = true;
                //        config.Set(TryHook, false);
                //    }
                //    catch
                //    {
                //        Msg("Failed to hook to FontURL");
                //        config.Set(TryHook, false);
                //    }
                //}

                if (config.GetValue(DebugValues))
                {
                    Msg(fontAsset, fontAsset.URL, fontAsset.Slot.Name, fontAsset.World.Name);
                }

                //else
                //{
                //    Debug("Hook check didn't pass");
                //}

                if (fontHooked && fontAsset.URL != config.GetValue(FontURL))
                {
                    fontAsset.URL.Value = config.GetValue(FontURL);
                    Msg("Userspace font updated!");
                }
                if (fontHooked && fontAsset.Padding != config.GetValue(Padding))
                {
                    fontAsset.Padding.Value = config.GetValue(Padding);
                    Msg("Userspace font padding updated!");
                }
                if (fontHooked && fontAsset.PixelRange != config.GetValue(PixelRange))
                {
                    fontAsset.PixelRange.Value = config.GetValue(PixelRange);
                    Msg("Userspace font pixel range updated!");
                }
                if (fontHooked && fontAsset.GlyphEmSize != config.GetValue(GlyphEmSize))
                {
                    fontAsset.GlyphEmSize.Value = config.GetValue(GlyphEmSize);
                    Msg("Userspace font glyph em size updated!");
                }
                if (config.GetValue(Enabled) && config.GetValue(TryHook))
                {
                    HookFont((FontChain)Userspace.UserspaceWorld.RootSlot.GetComponent("FrooxEngine.FontChain"));
                    config.Set(TryHook, false);
                }
            }
        }
    }
}
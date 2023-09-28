﻿using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using System;
using Elements.Core;
using System.Collections.Generic;


namespace ChangeUserspaceFont
{
    public class ChangeUserspaceFont : ResoniteMod
    {
        public override string Name => "ChangeUserSpaceFont";
        public override string Author => "Hayden (ported to Resonite by Raidriar796)";
        public override string Version => "1.0.2";
        public override string Link => "https://github.com/Hayden-Fluff/Change-Userspace-Font";

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> Enabled =
            new ModConfigurationKey<bool>("Enabled", "Enable Font Override", () => true);

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<Uri> FontURL =
            new ModConfigurationKey<Uri>("FontURL", "Font URL", () => new Uri("Resonitedb:///c801b8d2522fb554678f17f4597158b1af3f9be3abd6ce35d5a3112a81e2bf39.ttf"));
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
            new ModConfigurationKey<bool>("TryHook", "Try Hook - Only check this if something breaks", () => false);

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
            private static readonly Uri defaultFontURL = new Uri("resdb:///fcff04f4bec2b3636f05ed894dc1f9a752c4cb587ee49857ec7a82abaf6ca016.ttf");

            [HarmonyPatch(typeof(Userspace), "OnAttach")]
            [HarmonyPostfix]
            public static void Postfix(Userspace __instance)
            {
                FontChain _fontChain = Userspace.UserspaceWorld.RootSlot.GetComponent<FontChain>();

                //Msg("FontChain in " + _fontChain.World.Name + " awoke on " + _fontChain.Slot.Name);
                
                if (config.GetValue(Enabled) && _fontChain.World == Userspace.UserspaceWorld && _fontChain.Slot == Userspace.UserspaceWorld.RootSlot)
                {
                    HookFont(_fontChain);
                }
            }
            public static void Postfix()
            {
                // Disable the font override on runtime
                if (!config.GetValue(Enabled))
                {
                    if (fontHooked)
                    {
                        fontAsset.URL.Value = defaultFontURL;
                        fontAsset.Padding.Value = 1;
                        fontAsset.PixelRange.Value = 4;
                        fontAsset.GlyphEmSize.Value = 32;
                    }
                }
                
                //Update any font properties on change
                if (config.GetValue(Enabled) && fontHooked && fontAsset.URL != config.GetValue(FontURL))
                {
                    fontAsset.URL.Value = config.GetValue(FontURL);
                    Msg("Userspace font updated!");
                }

                if (config.GetValue(Enabled) && fontHooked && fontAsset.Padding != config.GetValue(Padding))
                {
                    fontAsset.Padding.Value = config.GetValue(Padding);
                    Msg("Userspace font padding updated!");
                }

                if (config.GetValue(Enabled) && fontHooked && fontAsset.PixelRange != config.GetValue(PixelRange))
                {
                    fontAsset.PixelRange.Value = config.GetValue(PixelRange);
                    Msg("Userspace font pixel range updated!");
                }

                if (config.GetValue(Enabled) && fontHooked && fontAsset.GlyphEmSize != config.GetValue(GlyphEmSize))
                {
                    fontAsset.GlyphEmSize.Value = config.GetValue(GlyphEmSize);
                    Msg("Userspace font glyph em size updated!");
                }
                // Manually retries hook if it failed before
                if (config.GetValue(Enabled) && config.GetValue(TryHook))
                {
                    HookFont(Userspace.UserspaceWorld.RootSlot.GetComponent<FontChain>());
                    config.Set(TryHook, false);
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

        }
    }
}
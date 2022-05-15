using HarmonyLib;
using NeosModLoader;

namespace ChangeUserSpaceFont
{
    public class ChangeUserSpaceFont : NeosMod
    {
        public override string Name => "ChangeUserSpaceFont";
        public override string Author => "Hayden";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/GithubUsername/RepoName/";

        public static string currentFontURL;
        
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<string> FontURL =
            new ModConfigurationKey<string>("FontURL", "Font URL", () => $"neosdb:///c801b8d2522fb554678f17f4597158b1af3f9be3abd6ce35d5a3112a81e2bf39.ttf");
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> Enabled =
            new ModConfigurationKey<bool>("Enabled", "Enable Font Override", () => true);

        private static ModConfiguration config;


        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("net.Hayden.ChangeUserspaceFont");
            harmony.PatchAll();
        }
		
        [HarmonyPatch(typeof(FrooxEngine.Userspace))]
        class ModNameGoesHerePatch
        {
            public static bool Prefix(FrooxEngine.Userspace __instance)
            {
                if (config.GetValue(Enabled))
                {
                    if ()
                    {
                        currentFontURL = config.GetValue(FontURL);
                        return true;
                    }
                }
                
                return false;//dont run rest of method
            }

            public static void Postfix()
            {
                if (config == null) return;
            }
        }
    }
}
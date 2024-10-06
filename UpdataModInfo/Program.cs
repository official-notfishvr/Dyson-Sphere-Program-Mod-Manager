using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using static UpdataModInfo.ThunderstoreAPIMain;

namespace UpdataModInfo
{
    internal class Program
    {
        public static bool LogInfo = false;
        static async Task Main(string[] args)
        {
            var packageNames = new List<string> 
            { 
                // Gameplay
                "CAAP_N", "GenesisBook_Experimental", "NebulaMultiplayerMod", "DSPAutoSorter", "GigaStationsUpdated", "MoreMegaStructure", "SplitterOverBelt",

                // Tweaks / Tools
                "DeliverySlotsTweaks", "DSPModSave", "BlueprintTweaks", "LSTM", "UXAssist", "DSPJapanesePlugin", "CruiseAssist", "MoreStatInfo",

                // Libraries
                "Auxilaryfunction", "BulletTime", "CommonAPI", "IlLine", "LDBTool", "CessilCellsCeaChells", "NebulaMultiplayerModApi", "GalacticScale", "CloseError", "Multfuntion_mod"
            };
            var packagesInfo = new List<object>();

            if (LogInfo)
            {
                var package = await ThunderstoreAPI.ReturnThunderstorePackageByName("Multfuntion_mod");
                Console.WriteLine(package.FullName);
                Console.ReadKey();
            }
            else
            {
                foreach (var packageName in packageNames)
                {
                    var package = await ThunderstoreAPI.ReturnThunderstorePackageByName(packageName);

                    if (package?.Versions != null && package.Versions.Length > 0)
                    {
                        var latestVersion = package.Versions[0];
                        var dependencies = GetDependencies(package.FullName)?.Split(", ".ToCharArray());
                        var modInfo = new Dictionary<string, object> { };

                        modInfo.Add("name", latestVersion.Name);
                        modInfo.Add("author", package.Owner);
                        modInfo.Add("version", latestVersion.VersionNumber);

                        if (dependencies != null && dependencies.Length > 0) { modInfo.Add("dependencies", dependencies); }

                        modInfo.Add("git_path", latestVersion.WebsiteUrl);
                        modInfo.Add("group", GetGroup(package.FullName));
                        modInfo.Add("download_url", latestVersion.DownloadUrl);

                        packagesInfo.Add(modInfo);
                    }
                    else
                    {
                        Console.WriteLine($"No versions found for the package: {packageName}");
                    }
                }

                string jsonString = JsonSerializer.Serialize(packagesInfo, new JsonSerializerOptions { WriteIndented = true });
                string filePath = "modinfo.json";
                File.WriteAllText(filePath, jsonString);

                Console.WriteLine($"Package information saved to {filePath}");
                Console.ReadKey();
            }
        }
        public static string GetGroup(string name)
        {
            // Gameplay
            if (name == "NordLandeW-CAAP_N") { return "Gameplay"; }
            if (name == "GenesisBook-GenesisBook_Experimental") { return "Gameplay"; }
            if (name == "nebula-NebulaMultiplayerMod") { return "Gameplay"; }
            if (name == "appuns-DSPAutoSorter") { return "Gameplay"; }
            if (name == "kremnev8-GigaStationsUpdated") { return "Gameplay"; }
            if (name == "jinxOAO-MoreMegaStructure") { return "Gameplay"; }
            if (name == "hetima-SplitterOverBelt") { return "Gameplay"; }

            // Tweaks / Tools
            if (name == "starfi5h-DeliverySlotsTweaks") { return "Tweaks / Tools"; }
            if (name == "CommonAPI-DSPModSave") { return "Tweaks / Tools"; }
            if (name == "kremnev8-BlueprintTweaks") { return "Tweaks / Tools"; }
            if (name == "hetima-LSTM") { return "Tweaks / Tools"; }
            if (name == "soarqin-UXAssist") { return "Tweaks / Tools"; }
            if (name == "appuns-DSPJapanesePlugin") { return "Tweaks / Tools"; }
            if (name == "abukaff-CruiseAssist") { return "Tweaks / Tools"; }
            if (name == "blacksnipebiu-MoreStatInfo") { return "Tweaks / Tools"; }

            // Libraries
            if (name == "blacksnipebiu-Auxilaryfunction") { return "Libraries"; }
            if (name == "starfi5h-BulletTime") { return "Libraries"; }
            if (name == "CommonAPI-CommonAPI") { return "Libraries"; }
            if (name == "PhantomGamers-IlLine") { return "Libraries"; }
            if (name == "xiaoye97-LDBTool") { return "Libraries"; }
            if (name == "www_Day_Dream-CessilCellsCeaChells") { return "Libraries"; }
            if (name == "nebula-NebulaMultiplayerModApi") { return "Libraries"; }
            if (name == "Galactic_Scale-GalacticScale") { return "Libraries"; }
            if (name == "crecheng-CloseError") { return "Libraries"; }
            if (name == "blacksnipebiu-Multfuntion_mod") { return "Libraries"; }
            return null;
        }
        public static string GetDependencies(string name)
        {
            // Gameplay
            if (name == "nebula-NebulaMultiplayerMod") { return "NebulaMultiplayerModApi,IlLine,BulletTime"; }
            if (name == "jinxOAO-MoreMegaStructure") { return "LDBTool,DSPModSave,CommonAPI"; }

            // Tweaks / Tools
            if (name == "kremnev8-BlueprintTweaks") { return "LDBTool,NebulaMultiplayerModApi,CommonAPI"; }
            if (name == "soarqin-UXAssist") { return "CommonAPI,DSPModSave"; }

            // Libraries
            if (name == "CommonAPI-CommonAPI") { return "LDBTool,DSPModSave"; }
            if (name == "Galactic_Scale-GalacticScale") { return "CloseError"; }
            return null;
        }
    }
}
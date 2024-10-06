using DSPModManager.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DSPModManager.Helper.SimpleJson;

namespace DSPModManager
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );
        private string InstallDirectory = @"";
        public bool platformDetected = false;
        private bool modsDisabled = false;
        private CookieContainer PermCookie;
        private const string BaseEndpoint = "https://api.github.com/repos/";
        private const Int16 CurrentVersion = 1;
        private List<ReleaseInfo> releases;
        private Dictionary<string, int> groups = new Dictionary<string, int>();
        public Form1()
        {
            InitializeComponent();
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Button btn in Controls.OfType<Button>()) { btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btn.Width, btn.Height, 13, 13)); }
            LocationHandler();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            releases = new List<ReleaseInfo>();
            if (!File.Exists(Path.Combine(InstallDirectory, "winhttp.dll")))
            {
                if (File.Exists(Path.Combine(InstallDirectory, "mods.disable")))
                {
                    buttonToggleMods.Text = "Enable Mods";
                    modsDisabled = true;
                    buttonToggleMods.Enabled = true;
                }
                else
                {
                    buttonToggleMods.Enabled = false;
                }
            }
            else
            {
                buttonToggleMods.Enabled = true;
            }
            new Thread(() =>
            {
                LoadRequiredPlugins();
            }).Start();
        }
        private void LocationHandler()
        {
            string steam = GetSteamLocation();
            if (steam != null)
            {
                if (Directory.Exists(steam))
                {
                    if (File.Exists(steam + @"\DSPGAME.exe") | File.Exists(steam + @"\DSPGAME.exe"))
                    {
                        textBoxDirectory.Text = steam;
                        InstallDirectory = steam;
                        platformDetected = true;
                        return;
                    }
                }
            }
            MessageBox.Show("We couldn't seem to find your DSPGAME installation, please press \"OK\" and point us to it", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            NotFoundHandler();
            this.TopMost = true;
        }
        private string GetSteamLocation()
        {
            string path = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1366540", @"InstallLocation");
            if (path != null) { path = path + @"\"; }
            return path;
        }
        private void NotFoundHandler()
        {
            bool found = false;
            while (found == false)
            {
                using (var fileDialog = new OpenFileDialog())
                {
                    fileDialog.FileName = "DSPGAME Executable";
                    fileDialog.Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";
                    fileDialog.FilterIndex = 1;
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string path = fileDialog.FileName;
                        if (Path.GetFileName(path).Equals("DSPGAME.exe") | Path.GetFileName(path).Equals("DSPGAME.exe"))
                        {
                            InstallDirectory = Path.GetDirectoryName(path);
                            textBoxDirectory.Text = InstallDirectory;
                            found = true;
                        }
                        else
                        {
                            MessageBox.Show("That's not the DSPGAME exectuable! please try again!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }
        }
        private void buttonFolderBrowser_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.FileName = "DSPGAME Executable";
                fileDialog.Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";
                fileDialog.FilterIndex = 1;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = fileDialog.FileName;
                    if (Path.GetFileName(path).Equals("DSPGAME.exe") | Path.GetFileName(path).Equals("DSPGAME.exe"))
                    {
                        InstallDirectory = Path.GetDirectoryName(path);
                        textBoxDirectory.Text = InstallDirectory;
                    }
                    else
                    {
                        MessageBox.Show("That's not the DSPGAME exectuable! please try again!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void buttonOpenGameFolder_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(InstallDirectory)) { Process.Start(InstallDirectory); }
        }
        private void buttonOpenConfigFolder_Click(object sender, EventArgs e)
        {
            var configDirectory = Path.Combine(InstallDirectory, @"BepInEx\config");
            if (Directory.Exists(configDirectory)) { Process.Start(configDirectory); }
        }
        private void buttonOpenModsFolder_Click(object sender, EventArgs e)
        {
            var BepInExDirectory = Path.Combine(InstallDirectory, @"BepInEx\plugins");
            if (Directory.Exists(BepInExDirectory)) { Process.Start(BepInExDirectory); }
        }
        private void buttonToggleMods_Click(object sender, EventArgs e)
        {
            if (modsDisabled)
            {
                if (File.Exists(Path.Combine(InstallDirectory, "mods.disable")))
                {
                    File.Move(Path.Combine(InstallDirectory, "mods.disable"), Path.Combine(InstallDirectory, "winhttp.dll"));
                    buttonToggleMods.Text = "Disable Mods";
                    buttonToggleMods.BackColor = Color.FromArgb(120, 0, 0);
                    modsDisabled = false;
                }
            }
            else
            {
                if (File.Exists(Path.Combine(InstallDirectory, "winhttp.dll")))
                {
                    File.Move(Path.Combine(InstallDirectory, "winhttp.dll"), Path.Combine(InstallDirectory, "mods.disable"));
                    buttonToggleMods.Text = "Enable Mods";
                    buttonToggleMods.BackColor = Color.FromArgb(0, 120, 0);
                    modsDisabled = true;
                }
            }
        }
        private string DownloadSite(string URL)
        {
            try
            {
                if (PermCookie == null) { PermCookie = new CookieContainer(); }
                HttpWebRequest RQuest = (HttpWebRequest)HttpWebRequest.Create(URL);
                RQuest.Method = "GET";
                RQuest.KeepAlive = true;
                RQuest.CookieContainer = PermCookie;
                RQuest.ContentType = "application/x-www-form-urlencoded";
                RQuest.Referer = "";
                RQuest.UserAgent = "Dyson-Sphere-Program";
                RQuest.Proxy = null;
                HttpWebResponse Response = (HttpWebResponse)RQuest.GetResponse();
                StreamReader Sr = new StreamReader(Response.GetResponseStream());
                string Code = Sr.ReadToEnd();
                Sr.Close();
                return Code;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("403"))
                {
                    MessageBox.Show("Failed to update version info, GitHub has rate limited you, please check back in 15 - 30 minutes. If this problem persists, share this error to helpers in the modding discord:\n{ex.Message}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Failed to update version info, please check your internet connection. If this problem persists, share this error to helpers in the modding discord:\n{ex.Message}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Process.GetCurrentProcess().Kill();
                return null;
            }
        }
        private void CheckVersion()
        {
            Int16 version = Convert.ToInt16(DownloadSite("https://raw.githubusercontent.com/official-notfishvr/Dyson-Sphere-Program-Mod-Manger/refs/heads/main/ModInfo/update.txt"));
            if (version > CurrentVersion)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show("Your version of the mod installer is outdated! Please download the new one!", "Update available!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Process.Start("https://github.com/official-notfishvr/Dyson-Sphere-Program-Mod-Manger/releases/latest");
                    Process.GetCurrentProcess().Kill();
                    Environment.Exit(0);
                }));
            }
        }
        #region RegHelper
        enum RegSAM
        {
            QueryValue = 0x0001,
            SetValue = 0x0002,
            CreateSubKey = 0x0004,
            EnumerateSubKeys = 0x0008,
            Notify = 0x0010,
            CreateLink = 0x0020,
            WOW64_32Key = 0x0200,
            WOW64_64Key = 0x0100,
            WOW64_Res = 0x0300,
            Read = 0x00020019,
            Write = 0x00020006,
            Execute = 0x00020019,
            AllAccess = 0x000f003f
        }
        static class RegHive
        {
            public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
            public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
        }
        static class RegistryWOW6432
        {
            [DllImport("Advapi32.dll")]
            static extern uint RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out int phkResult);

            [DllImport("Advapi32.dll")]
            static extern uint RegCloseKey(int hKey);

            [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
            public static extern int RegQueryValueEx(int hKey, string lpValueName, int lpReserved, ref uint lpType, System.Text.StringBuilder lpData, ref uint lpcbData);

            static public string GetRegKey64(UIntPtr inHive, String inKeyName, string inPropertyName)
            {
                return GetRegKey64(inHive, inKeyName, RegSAM.WOW64_64Key, inPropertyName);
            }

            static public string GetRegKey32(UIntPtr inHive, String inKeyName, string inPropertyName)
            {
                return GetRegKey64(inHive, inKeyName, RegSAM.WOW64_32Key, inPropertyName);
            }

            static public string GetRegKey64(UIntPtr inHive, String inKeyName, RegSAM in32or64key, string inPropertyName)
            {
                int hkey = 0;

                try
                {
                    uint lResult = RegOpenKeyEx(RegHive.HKEY_LOCAL_MACHINE, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out hkey);
                    if (0 != lResult) return null;
                    uint lpType = 0;
                    uint lpcbData = 1024;
                    StringBuilder AgeBuffer = new StringBuilder(1024);
                    RegQueryValueEx(hkey, inPropertyName, 0, ref lpType, AgeBuffer, ref lpcbData);
                    string Age = AgeBuffer.ToString();
                    return Age;
                }
                finally
                {
                    if (0 != hkey) RegCloseKey(hkey);
                }
            }
        }
        #endregion // RegHelper
        #region ReleaseHandling
        private void LoadReleases()
        {
            var decodedMods = JSON.Parse(DownloadSite("https://raw.githubusercontent.com/official-notfishvr/Dyson-Sphere-Program-Mod-Manger/refs/heads/main/ModInfo/modinfo.json"));
            var decodedGroups = JSON.Parse(DownloadSite("https://raw.githubusercontent.com/official-notfishvr/Dyson-Sphere-Program-Mod-Manger/refs/heads/main/ModInfo/groupinfo.json"));

            var allMods = decodedMods.AsArray;
            var allGroups = decodedGroups.AsArray;

            for (int i = 0; i < allMods.Count; i++)
            {
                JSONNode current = allMods[i];
                ReleaseInfo release = new ReleaseInfo(current["name"], current["author"], current["version"], current["group"], current["download_url"], current["install_location"], current["git_path"], current["dependencies"].AsArray);
                releases.Add(release);
            }


            allGroups.Linq.OrderBy(x => x.Value["rank"]);
            for (int i = 0; i < allGroups.Count; i++)
            {
                JSONNode current = allGroups[i];
                if (releases.Any(x => x.Group == current["name"]))
                {
                    groups.Add(current["name"], groups.Count());
                }
            }
            groups.Add("Uncategorized", groups.Count());

            foreach (ReleaseInfo release in releases)
            {
                foreach (string dep in release.Dependencies)
                {
                    releases.Where(x => x.Name == dep).FirstOrDefault()?.Dependents.Add(release.Name);
                }
            }
        }
        private void LoadRequiredPlugins()
        {
            //CheckVersion();
            LoadReleases();
            this.Invoke((MethodInvoker)(() =>
            {
                Dictionary<string, int> includedGroups = new Dictionary<string, int>();

                for (int i = 0; i < groups.Count(); i++)
                {
                    var key = groups.First(x => x.Value == i).Key;
                    var value = listViewMods.Groups.Add(new ListViewGroup(key, HorizontalAlignment.Left));
                    groups[key] = value;
                }

                foreach (ReleaseInfo release in releases)
                {
                    ListViewItem item = new ListViewItem();
                    item.BackColor = Color.FromArgb(28, 28, 28);
                    item.ForeColor = Color.White;
                    item.Text = release.Name;
                    if (!String.IsNullOrEmpty(release.Version)) item.Text = $"{release.Name} - {release.Version}";
                    if (!String.IsNullOrEmpty(release.Tag)) { item.Text = string.Format("{0} - ({1})", release.Name, release.Tag); };
                    item.SubItems.Add(release.Author);
                    item.Tag = release;
                    if (release.Install)
                    {
                        listViewMods.Items.Add(item);
                    }
                    CheckDefaultMod(release, item);

                    if (release.Group == null || !groups.ContainsKey(release.Group))
                    {
                        item.Group = listViewMods.Groups[groups["Uncategorized"]];
                    }
                    else if (groups.ContainsKey(release.Group))
                    {
                        int index = groups[release.Group];
                        item.Group = listViewMods.Groups[index];
                    }
                }

                buttonInstall.Enabled = true;

            }));
        }
        private void CheckDefaultMod(ReleaseInfo release, ListViewItem item)
        {
            if (release.Name.Contains("BepInEx"))
            {
                item.Checked = true;
                item.ForeColor = Color.LightGray;
            }
            else
            {
                release.Install = false;
            }
        }
        #endregion // ReleaseHandling
        private void listViewMods_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void listViewMods_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ReleaseInfo release = (ReleaseInfo)e.Item.Tag;

            if (release.Dependencies.Count > 0)
            {
                foreach (ListViewItem item in listViewMods.Items)
                {
                    var plugin = (ReleaseInfo)item.Tag;

                    if (plugin.Name == release.Name) continue;
                    if (release.Dependencies.Contains(plugin.Name))
                    {
                        if (e.Item.Checked)
                        {
                            item.Checked = true;
                        }
                        else
                        {
                            release.Install = false;
                            if (releases.Count(x => plugin.Dependents.Contains(x.Name) && x.Install) <= 1)
                            {
                                item.Checked = false;
                            }
                        }
                    }
                }
            }

            if (release.Dependents.Count > 0)
            {
                if (releases.Count(x => release.Dependents.Contains(x.Name) && x.Install) > 0)
                {
                    e.Item.Checked = true;
                }
            }

            if (release.Name.Contains("BepInEx")) { e.Item.Checked = true; };
            release.Install = e.Item.Checked;
        }
        private void listViewMods_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listViewMods.SelectedItems.Count > 0)
            {
                //buttonModInfo.ForeColor = Color.White;
            }
            else
            {
                //buttonModInfo.ForeColor = Color.LightGray;
            }
        }
        private void buttonInstall_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Install();
            }).Start();
        }
        private void Install()
        {
            ChangeInstallButtonState(false);
            foreach (ReleaseInfo release in releases)
            {
                if (release.Install)
                {
                    byte[] file = DownloadFile(release.Link);
                    string fileName = Path.GetFileName(release.Link);

                    if (release.Link.Contains("thunderstore.io"))
                    {
                        string zipPath = Path.Combine(InstallDirectory, @"BepInEx\ModManger", $"{ release.Name}.zip");
                        if (!Directory.Exists(Path.GetDirectoryName(zipPath))) { Directory.CreateDirectory(Path.GetDirectoryName(zipPath)); }
                        File.WriteAllBytes(zipPath, file);

                        string pluginsDir = Path.Combine(InstallDirectory, @"BepInEx\plugins");
                        if (!Directory.Exists(pluginsDir)) { Directory.CreateDirectory(pluginsDir); }
                        string tempDir = Path.Combine(InstallDirectory, "temp");
                        if (!Directory.Exists(tempDir)) { Directory.CreateDirectory(tempDir); }
                        UnzipFile(file, tempDir);
                        MoveFoldersBack(tempDir, InstallDirectory);
                        MoveDllFiles(tempDir, pluginsDir);
                        Directory.Delete(tempDir, true);
                        DeleteUnwantedFiles(pluginsDir);
                    }
                    else
                    {
                        if (Path.GetExtension(fileName).Equals(".dll"))
                        {
                            string dir;
                            if (release.InstallLocation == null)
                            {
                                dir = Path.Combine(InstallDirectory, @"BepInEx\plugins", Regex.Replace(release.Name, @"\s+", string.Empty));
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }
                            }
                            else
                            {
                                dir = Path.Combine(InstallDirectory, release.InstallLocation);
                            }
                            File.WriteAllBytes(Path.Combine(dir, fileName), file);
                        }
                        else
                        {
                            UnzipFile(file, (release.InstallLocation != null) ? Path.Combine(InstallDirectory, release.InstallLocation) : InstallDirectory);
                        }
                    }
                }
            }
            ChangeInstallButtonState(true);

            this.Invoke((MethodInvoker)(() =>
            {
                buttonToggleMods.Enabled = true;
            }));
        }
        private void MoveDllFiles(string sourceDir, string targetDir)
        {
            foreach (string dllFile in Directory.GetFiles(sourceDir, "*.dll", SearchOption.AllDirectories))
            {
                string targetPath = Path.Combine(targetDir, Path.GetFileName(dllFile));
                if (File.Exists(targetPath)) { File.Delete(targetPath); }
                File.Move(dllFile, targetPath);
            }
        }
        private void MoveFoldersBack(string sourceDir, string targetDir)
        {
            string[] foldersToMove = { "plugins", "patchers" };

            foreach (var folder in foldersToMove)
            {
                string sourcePath = Path.Combine(sourceDir, folder);
                string destinationPath = Path.Combine(targetDir, "BepInEx", folder);

                if (Directory.Exists(sourcePath))
                {
                    if (!Directory.Exists(Path.Combine(targetDir, "BepInEx"))) { Directory.CreateDirectory(Path.Combine(targetDir, "BepInEx")); }
                    if (Directory.Exists(destinationPath)) { Directory.Delete(destinationPath, true); }
                    Directory.Move(sourcePath, destinationPath);
                }
            }
        }
        private void DeleteUnwantedFiles(string directory)
        {
            string[] unwantedFiles = { "README.md", "manifest.json", "icon.png", "CHANGELOG.md" };

            foreach (var unwantedFile in unwantedFiles)
            {
                string filePath = Path.Combine(directory, unwantedFile);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        private void ChangeInstallButtonState(bool enabled)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                buttonInstall.Enabled = enabled;
            }));
        }
        private byte[] DownloadFile(string url)
        {
            WebClient client = new WebClient();
            client.Proxy = null;
            return client.DownloadData(url);
        }
        private void UnzipFile(byte[] data, string directory)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (var unzip = new Unzip(ms))
                {
                    unzip.ExtractToDirectory(directory);
                }
            }
        }
        private void buttonModInfo_Click(object sender, EventArgs e)
        {
            OpenLinkFromRelease();
        }
        private void OpenLinkFromRelease()
        {
            if (listViewMods.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewMods.SelectedItems)
                {
                    ReleaseInfo release = (ReleaseInfo)item.Tag;
                    Process.Start(string.Format("https://github.com/{0}", release.GitPath));
                }
            }
        }
        private void buttonUninstallAll_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("You are about to delete all your mods (including any saved data in your plugins). This cannot be undone!\n\nAre you sure you wish to continue?", "Confirm Delete", MessageBoxButtons.YesNo);

            if (confirmResult == DialogResult.Yes)
            {
                var pluginsPath = Path.Combine(InstallDirectory, @"BepInEx\plugins");

                try
                {
                    foreach (var d in Directory.GetDirectories(pluginsPath)) { Directory.Delete(d, true); }
                    foreach (var f in Directory.GetFiles(pluginsPath)) { File.Delete(f); }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Something went wrong!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }
    }
}

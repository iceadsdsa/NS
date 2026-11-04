using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace NS
{
    public class EnvItem
    {
        public string Name;
        public string Language;
        public string Version = "";
        public string Path = "";
        public string Source = "";
        public string Uninstall = "";
        public string Url = "";
    }

    public static class Scanner
    {
        public static List<EnvItem> ScanAll(Action<string> progress)
        {
            var items = new List<EnvItem>();
            ScanRegistry(items, progress);
            ScanPath(items, progress);
            ScanDirs(items, progress);
            return Sort(Dedup(items));
        }

        static void Say(Action<string> p, string s) { if (p != null) p(s); }

        static void ScanRegistry(List<EnvItem> items, Action<string> progress)
        {
            Say(progress, L.T("扫描注册表…", "Scanning registry…"));
            RegistryHive[] hives = { RegistryHive.LocalMachine, RegistryHive.CurrentUser };
            RegistryView[] views = { RegistryView.Registry64, RegistryView.Registry32 };
            foreach (RegistryHive hive in hives)
                foreach (RegistryView view in views)
                {
                    RegistryKey baseKey = null, un = null;
                    try
                    {
                        baseKey = RegistryKey.OpenBaseKey(hive, view);
                        un = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                        if (un == null) continue;
                        foreach (string subName in un.GetSubKeyNames())
                        {
                            RegistryKey sub = null;
                            try
                            {
                                sub = un.OpenSubKey(subName);
                                if (sub == null) continue;
                                string display = sub.GetValue("DisplayName") as string;
                                Product p = Catalog.MatchName(display);
                                if (p == null) continue;
                                var it = new EnvItem();
                                it.Name = p.Name;
                                it.Language = p.Language;
                                it.Url = p.Url;
                                it.Version = (sub.GetValue("DisplayVersion") as string) ?? "";
                                it.Path = (sub.GetValue("InstallLocation") as string) ?? "";
                                it.Source = L.T("注册表: ", "Registry: ") + display;
                                string q = sub.GetValue("QuietUninstallString") as string;
                                string u = sub.GetValue("UninstallString") as string;
                                it.Uninstall = !string.IsNullOrEmpty(q) ? q : (u ?? "");
                                items.Add(it);
                            }
                            finally { if (sub != null) sub.Close(); }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (un != null) un.Close();
                        if (baseKey != null) baseKey.Close();
                    }
                }
        }

        static void ScanPath(List<EnvItem> items, Action<string> progress)
        {
            Say(progress, L.T("扫描 PATH…", "Scanning PATH…"));
            string[] parts =
            {
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "",
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "",
                Environment.GetEnvironmentVariable("PATH") ?? ""
            };
            var seen = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (string part in parts)
            {
                string[] segs = part.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string raw in segs)
                {
                    string dir = raw.Trim().Trim('"');
                    if (dir.Length == 0) continue;
                    try { dir = Environment.ExpandEnvironmentVariables(dir); } catch { }
                    if (seen.ContainsKey(dir)) continue;
                    seen[dir] = true;
                    string[] files;
                    try
                    {
                        if (!Directory.Exists(dir)) continue;
                        files = Directory.GetFiles(dir);
                    }
                    catch { continue; }
                    foreach (string f in files)
                    {
                        Product p = Catalog.MatchExe(Path.GetFileName(f));
                        if (p == null) continue;
                        var it = new EnvItem();
                        it.Name = p.Name;
                        it.Language = p.Language;
                        it.Url = p.Url;
                        it.Version = FileVer(f);
                        it.Path = dir;
                        it.Source = "PATH: " + f;
                        items.Add(it);
                    }
                }
            }
        }

        static void ScanDirs(List<EnvItem> items, Action<string> progress)
        {
            Say(progress, L.T("扫描磁盘目录…", "Scanning disk folders…"));
            var parents = new List<string>();
            try
            {
                foreach (DriveInfo d in DriveInfo.GetDrives())
                    if (d.DriveType == DriveType.Fixed && d.IsReady) parents.Add(d.RootDirectory.FullName);
            }
            catch { }
            Add(parents, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            Add(parents, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            Add(parents, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs"));
            Add(parents, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            foreach (string parent in parents)
            {
                bool deep = parent.EndsWith("\\");
                foreach (string dir in ChildDirs(parent))
                {
                    CheckDir(dir, items);
                    if (deep && !SkipSub(Path.GetFileName(dir.TrimEnd('\\'))))
                        foreach (string sub in ChildDirs(dir)) CheckDir(sub, items);
                }
            }
        }

        static void Add(List<string> l, string s)
        {
            if (!string.IsNullOrEmpty(s) && !l.Contains(s)) l.Add(s);
        }

        static List<string> ChildDirs(string parent)
        {
            var l = new List<string>();
            try { l.AddRange(Directory.GetDirectories(parent)); } catch { }
            return l;
        }

        static bool SkipSub(string name)
        {
            string n = name.ToLowerInvariant();
            return n == "windows" || n == "users" || n == "appdata" || n == "programdata"
                || n.StartsWith("program files") || n.StartsWith("$recycle") || n.StartsWith("system volume");
        }

        static void CheckDir(string dir, List<EnvItem> items)
        {
            string name = Path.GetFileName(dir.TrimEnd('\\'));
            if (name.StartsWith(".")) return;
            Product p = Catalog.MatchName(name);
            if (p == null) return;
            var it = new EnvItem();
            it.Name = p.Name;
            it.Language = p.Language;
            it.Url = p.Url;
            it.Path = dir;
            it.Source = L.T("目录: ", "Folder: ") + name;
            it.Version = FindVer(p, dir);
            items.Add(it);
        }

        static string FindVer(Product p, string dir)
        {
            if (p.Exes.Length == 0) return "未知";
            string[] subs = { "", "bin", "Scripts" };
            foreach (string sub in subs)
            {
                string d = sub.Length == 0 ? dir : Path.Combine(dir, sub);
                foreach (string exe in p.Exes)
                {
                    string f = Path.Combine(d, exe);
                    if (File.Exists(f)) return FileVer(f);
                }
            }
            return "";
        }

        static string FileVer(string f)
        {
            try
            {
                FileVersionInfo v = FileVersionInfo.GetVersionInfo(f);
                string s = v.ProductVersion;
                if (string.IsNullOrEmpty(s)) s = v.FileVersion;
                return string.IsNullOrEmpty(s) ? "" : s.Trim();
            }
            catch { return "未知"; }
        }

        public static List<EnvItem> Dedup(List<EnvItem> items)
        {
            var map = new Dictionary<string, EnvItem>(StringComparer.OrdinalIgnoreCase);
            var order = new List<EnvItem>();
            foreach (EnvItem it in items)
            {
                string loc = (it.Path ?? "").Trim().TrimEnd('\\');
                if (loc.Length == 0) loc = it.Source;
                string key = it.Name + "|" + loc;
                EnvItem old;
                if (map.TryGetValue(key, out old))
                {
                    if (string.IsNullOrEmpty(old.Uninstall)) old.Uninstall = it.Uninstall;
                    if (string.IsNullOrEmpty(old.Version)) old.Version = it.Version;
                    if (string.IsNullOrEmpty(old.Path)) old.Path = it.Path;
                    continue;
                }
                map[key] = it;
                order.Add(it);
            }
            return order;
        }

        public static List<EnvItem> Sort(List<EnvItem> items)
        {
            items.Sort(delegate(EnvItem a, EnvItem b)
            {
                int c = string.Compare(a.Language, b.Language, StringComparison.OrdinalIgnoreCase);
                if (c != 0) return c;
                c = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                if (c != 0) return c;
                return string.Compare(a.Version, b.Version, StringComparison.OrdinalIgnoreCase);
            });
            return items;
        }
    }
}

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

[assembly: AssemblyTitle("NS 开发环境管理器")]
[assembly: AssemblyDescription("扫描、定位并管理本机开发环境")]
[assembly: AssemblyProduct("NS")]
[assembly: AssemblyCompany("NS")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]

namespace NS
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--scan") { RunScanReport(); return; }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void RunScanReport()
        {
            var items = Scanner.ScanAll(null);
            var sb = new StringBuilder();
            sb.AppendLine("NS 扫描报告 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string lang = "";
            foreach (EnvItem it in items)
            {
                if (it.Language != lang) { lang = it.Language; sb.AppendLine(); sb.AppendLine("[" + lang + "]"); }
                sb.AppendLine("  " + it.Name + " | " + it.Version + " | " + it.Path + " | " + it.Source);
            }
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ns-scan.txt");
            try { File.WriteAllText(file, sb.ToString(), Encoding.UTF8); }
            catch
            {
                file = Path.Combine(Path.GetTempPath(), "ns-scan.txt");
                File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
            }
        }
    }
}

using System;
using System.Text.RegularExpressions;

namespace NS
{
    public class Product
    {
        public string Name;
        public string Language;
        public string Url;
        public string[] Patterns;
        public string[] Exes;
    }

    public static class Catalog
    {
        // 声明顺序 = 匹配优先级：具体产品必须排在泛化产品之前（如 Anaconda/PyCharm 在 Python 前）
        public static readonly Product[] Products = new Product[]
        {
            P("Anaconda", "Python", "https://www.anaconda.com/download", new string[] { "anaconda" }, new string[] { "conda.exe" }),
            P("Android Studio", "Kotlin", "https://developer.android.com/studio", new string[] { "android studio" }, new string[] { "studio64.exe", "studio.exe" }),
            P("Bun", "JavaScript", "https://bun.com/", new string[] { @"\bbun\b" }, new string[] { "bun.exe" }),
            P("CLion", "C/C++", "https://www.jetbrains.com/clion/download/", new string[] { "clion" }, new string[] { "clion64.exe" }),
            P("Dart SDK", "Dart", "https://dart.dev/get-dart", new string[] { "dart sdk", @"^dart\b" }, new string[] { "dart.exe" }),
            P("Deno", "JavaScript", "https://deno.com/", new string[] { @"^deno\b" }, new string[] { "deno.exe" }),
            P("Flutter SDK", "Dart", "https://docs.flutter.dev/get-started/install", new string[] { "flutter" }, new string[] { "flutter.exe" }),
            P("Git", "开发工具", "https://git-scm.com/downloads", new string[] { @"^git\b", "git for windows" }, new string[] { "git.exe" }),
            P("Go", "Go", "https://go.dev/dl/", new string[] { @"^go$", @"^go \d", "golang", "go programming language" }, new string[] { "go.exe" }),
            P("GoLand", "Go", "https://www.jetbrains.com/goland/download/", new string[] { "goland" }, new string[] { "goland64.exe" }),
            P("Gradle", "Java", "https://gradle.org/releases/", new string[] { @"^gradle" }, new string[] { "gradle.exe" }),
            P("IntelliJ IDEA", "Java", "https://www.jetbrains.com/idea/download/", new string[] { "intellij" }, new string[] { "idea64.exe" }),
            P("Java JDK", "Java", "https://www.oracle.com/java/technologies/downloads/", new string[] { "jdk", "java se", "java development", @"^java \d", @"java\(tm\)", "temurin", "adoptium", "openjdk", "zulu", "corretto" }, new string[] { "javac.exe" }),
            P("Eclipse IDE", "Java", "https://eclipse.org/downloads/", new string[] { "eclipse ide", @"^eclipse$" }, new string[] { "eclipse.exe" }),
            P("Kotlin", "Kotlin", "https://github.com/JetBrains/kotlin/releases", new string[] { "kotlin" }, new string[] { "kotlinc.exe" }),
            P("LLVM/Clang", "C/C++", "https://github.com/llvm/llvm-project/releases", new string[] { "llvm", "clang" }, new string[] { "clang.exe" }),
            P("Lua", "Lua", "https://www.lua.org/download.html", new string[] { @"^lua\b", @"\blua \d", "luajit" }, new string[] { "lua.exe" }),
            P("Miniconda", "Python", "https://docs.conda.io/en/latest/miniconda.html", new string[] { "miniconda" }, new string[0]),
            P("MinGW-w64", "C/C++", "https://www.mingw-w64.org/downloads/", new string[] { "mingw" }, new string[] { "gcc.exe" }),
            P(".NET SDK", "C#/.NET", "https://dotnet.microsoft.com/download", new string[] { @"\.net sdk", "dotnet" }, new string[] { "dotnet.exe" }),
            P("Node.js", "JavaScript", "https://nodejs.org/", new string[] { @"node\.js", "nodejs" }, new string[] { "node.exe" }),
            P("Perl", "Perl", "https://www.perl.org/get.html", new string[] { "perl" }, new string[] { "perl.exe" }),
            P("PHP", "PHP", "https://www.php.net/downloads.php", new string[] { @"^php\b", @"\bphp \d" }, new string[] { "php.exe" }),
            P("PhpStorm", "PHP", "https://www.jetbrains.com/phpstorm/download/", new string[] { "phpstorm" }, new string[] { "phpstorm64.exe" }),
            P("PyCharm", "Python", "https://www.jetbrains.com/pycharm/download/", new string[] { "pycharm" }, new string[] { "pycharm64.exe" }),
            P("PyPy", "Python", "https://www.pypy.org/download.html", new string[] { "pypy" }, new string[] { "pypy.exe" }),
            P("Python", "Python", "https://www.python.org/downloads/", new string[] { @"\bpython\b\s*\d", @"\bpython\b\s*\(", @"^python$" }, new string[] { "python.exe" }),
            P("R", "R", "https://cran.r-project.org/bin/windows/base/", new string[] { "r for windows", @"^r-?\d", "cran" }, new string[] { "R.exe" }),
            P("Rider", "C#/.NET", "https://www.jetbrains.com/rider/download/", new string[] { @"\brider\b" }, new string[] { "rider64.exe" }),
            P("Ruby", "Ruby", "https://www.ruby-lang.org/en/downloads/", new string[] { @"^ruby", "rubyinstaller", @"\bruby \d" }, new string[] { "ruby.exe" }),
            P("RubyMine", "Ruby", "https://www.jetbrains.com/rubymine/download/", new string[] { "rubymine" }, new string[] { "rubymine64.exe" }),
            P("Rust", "Rust", "https://rustup.rs/", new string[] { "rustup", @"\brust\b", "rustrover" }, new string[] { "rustc.exe", "cargo.exe" }),
            P("Scala", "Scala", "https://www.scala-lang.org/download/", new string[] { @"^scala\b", @"\bscala \d" }, new string[] { "scala.exe" }),
            P("Visual Studio Code", "开发工具", "https://code.visualstudio.com/", new string[] { "visual studio code", "vs code", "vscode" }, new string[] { "code.exe" }),
            P("Visual Studio", "C#/.NET", "https://visualstudio.microsoft.com/downloads/", new string[] { @"^(?:microsoft )?visual studio (?:community|professional|enterprise|ultimate|premium|express|test professional|team suite|build tools)(?: (?:19|20)\d{2})?(?: preview)?$", @"^(?:microsoft )?visual studio (?:19|20)\d{2}(?: preview)?$" }, new string[] { "devenv.exe" }),
            P("WebStorm", "JavaScript", "https://www.jetbrains.com/webstorm/download/", new string[] { "webstorm" }, new string[] { "webstorm64.exe" })
        };

        static Product P(string n, string l, string u, string[] p, string[] e)
        {
            return new Product { Name = n, Language = l, Url = u, Patterns = p, Exes = e };
        }

        public static Product MatchName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            for (int i = 0; i < Products.Length; i++)
                for (int j = 0; j < Products[i].Patterns.Length; j++)
                    if (Regex.IsMatch(name, Products[i].Patterns[j], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                        return Products[i];
            return null;
        }

        public static Product MatchExe(string file)
        {
            if (string.IsNullOrEmpty(file)) return null;
            for (int i = 0; i < Products.Length; i++)
                for (int j = 0; j < Products[i].Exes.Length; j++)
                    if (string.Compare(Products[i].Exes[j], file, StringComparison.OrdinalIgnoreCase) == 0)
                        return Products[i];
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace NS
{
    public class MainForm : Form
    {
        static readonly Color AppBackground = Color.FromArgb(245, 248, 247);
        static readonly Color Surface = Color.White;
        static readonly Color Sidebar = Color.FromArgb(22, 35, 31);
        static readonly Color SidebarActive = Color.FromArgb(35, 55, 48);
        static readonly Color Accent = Color.FromArgb(14, 124, 102);
        static readonly Color AccentSoft = Color.FromArgb(232, 246, 241);
        static readonly Color TextPrimary = Color.FromArgb(30, 43, 38);
        static readonly Color TextMuted = Color.FromArgb(103, 118, 112);
        static readonly Color Border = Color.FromArgb(221, 230, 226);
        static readonly Color SoftFill = Color.FromArgb(247, 250, 249);
        static readonly Color Blue = Color.FromArgb(67, 100, 175);

        readonly List<EnvItem> allItems = new List<EnvItem>();
        readonly List<Product> downloadItems = new List<Product>();
        readonly List<NavButton> navButtons = new List<NavButton>();
        readonly List<Panel> pages = new List<Panel>();

        TableLayoutPanel shellRoot;
        RoundedButton btnScan;
        RoundedButton btnManage;
        RoundedButton btnVisitSite;
        SearchBox envSearch;
        SearchBox downloadSearch;
        DataGridView envGrid;
        DataGridView downloadGrid;
        EmptyStatePanel envEmpty;
        EmptyStatePanel downloadEmpty;
        Label summaryCount;
        Label summaryLanguages;
        Label summaryLastScan;
        Label statusLabel;
        ContextMenuStrip manageMenu;
        DateTime? lastScan;
        bool scanning;

        public MainForm()
        {
            Text = L.T("NS ", "NS ");
            Width = 1200;
            Height = 780;
            MinimumSize = new Size(1000, 650);
            Icon = BuildAppIcon();
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppBackground;
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            AutoScaleMode = AutoScaleMode.Dpi;
            KeyPreview = true;
            DoubleBuffered = true;

            LoadDownloadCatalog();
            BuildShell();
            ShowPage(0);
            KeyDown += MainForm_KeyDown;
        }

        void BuildShell()
        {
            shellRoot = new TableLayoutPanel();
            shellRoot.Dock = DockStyle.Fill;
            shellRoot.ColumnCount = 2;
            shellRoot.RowCount = 1;
            shellRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 238F));
            shellRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            shellRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            shellRoot.Margin = Padding.Empty;
            shellRoot.Padding = Padding.Empty;
            shellRoot.BackColor = AppBackground;

            shellRoot.Controls.Add(BuildSidebar(), 0, 0);
            shellRoot.Controls.Add(BuildContentArea(), 1, 0);
            Controls.Add(shellRoot);
        }

        Control BuildSidebar()
        {
            var sidebar = new TableLayoutPanel();
            sidebar.Dock = DockStyle.Fill;
            sidebar.BackColor = Sidebar;
            sidebar.ColumnCount = 1;
            sidebar.RowCount = 4;
            sidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 184F));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            sidebar.Padding = Padding.Empty;
            sidebar.Margin = Padding.Empty;

            var brand = new Panel();
            brand.Dock = DockStyle.Fill;
            brand.Margin = Padding.Empty;
            brand.BackColor = Sidebar;

            var mark = new RoundedPanel();
            mark.CornerRadius = 8;
            mark.BackColor = Accent;
            mark.Location = new Point(18, 24);
            mark.Size = new Size(42, 42);
            var markText = MakeLabel("NS", 10.5F, FontStyle.Bold, Color.White);
            markText.AutoSize = false;
            markText.Dock = DockStyle.Fill;
            markText.TextAlign = ContentAlignment.MiddleCenter;
            mark.Controls.Add(markText);

            var brandText = new TableLayoutPanel();
            brandText.AutoSize = true;
            brandText.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            brandText.ColumnCount = 1;
            brandText.RowCount = 2;
            brandText.Location = new Point(72, 20);
            brandText.Margin = Padding.Empty;
            brandText.Padding = Padding.Empty;
            brandText.BackColor = Sidebar;
            brandText.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            brandText.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            brandText.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var logo = MakeLabel("NS", 16F, FontStyle.Bold, Color.White);
            logo.Margin = new Padding(0);
            var product = MakeLabel(L.T("开发环境管理器", "Dev Environment Manager"), 8F, FontStyle.Regular,
                Color.FromArgb(134, 157, 148));
            product.Margin = new Padding(0, 1, 0, 0);

            brandText.Controls.Add(logo, 0, 0);
            brandText.Controls.Add(product, 0, 1);
            brand.Controls.Add(mark);
            brand.Controls.Add(brandText);
            sidebar.Controls.Add(brand, 0, 0);

            var nav = new FlowLayoutPanel();
            nav.Dock = DockStyle.Fill;
            nav.FlowDirection = FlowDirection.TopDown;
            nav.WrapContents = false;
            nav.Padding = new Padding(16, 6, 16, 0);
            nav.Margin = Padding.Empty;
            nav.BackColor = Sidebar;
            string[] labels =
            {
                L.T("环境概览", "Overview"),
                L.T("下载中心", "Downloads"),
                L.T("关于", "About")
            };
            string[] glyphs = { "\uE80F", "\uE896", "\uE946" };
            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                var button = new NavButton();
                button.Text = labels[i];
                button.Glyph = glyphs[i];
                button.Width = 206;
                button.Margin = new Padding(0, 0, 0, 4);
                button.Click += delegate { ShowPage(index); };
                nav.Controls.Add(button);
                navButtons.Add(button);
            }
            nav.Resize += delegate
            {
                int width = Math.Max(120, nav.ClientSize.Width - nav.Padding.Horizontal);
                foreach (NavButton button in navButtons) button.Width = width;
            };
            sidebar.Controls.Add(nav, 0, 1);

            var spacer = new Panel();
            spacer.Dock = DockStyle.Fill;
            spacer.BackColor = Sidebar;
            sidebar.Controls.Add(spacer, 0, 2);

            var localCard = new Panel();
            localCard.Dock = DockStyle.Fill;
            localCard.BackColor = Sidebar;
            localCard.Margin = Padding.Empty;
            localCard.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (var pen = new Pen(Color.FromArgb(42, 62, 55)))
                    e.Graphics.DrawLine(pen, 18, 0, localCard.Width - 18, 0);
            };
            var localIcon = new IconBadge();
            localIcon.Glyph = "\uE72E";
            localIcon.FillColor = Color.FromArgb(34, 56, 49);
            localIcon.GlyphColor = Color.FromArgb(92, 205, 170);
            localIcon.Location = new Point(18, 24);
            localIcon.Size = new Size(32, 32);
            var localText = new TableLayoutPanel();
            localText.AutoSize = true;
            localText.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            localText.ColumnCount = 1;
            localText.RowCount = 2;
            localText.Location = new Point(60, 21);
            localText.Margin = Padding.Empty;
            localText.Padding = Padding.Empty;
            localText.BackColor = Sidebar;
            localText.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            localText.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            localText.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var localTitle = MakeLabel(L.T("仅在本机处理", "Processed locally"), 8.8F, FontStyle.Bold, Color.FromArgb(224, 233, 229));
            localTitle.Margin = Padding.Empty;
            var localSub = MakeLabel(L.T("不上传 · 不修改系统配置", "No uploads · No system changes"), 8F, FontStyle.Regular, Color.FromArgb(134, 157, 148));
            localSub.Margin = new Padding(0, 2, 0, 0);
            // 固定副标题宽度并允许自动换行，避免英文文案超出侧栏右边界。
            localSub.AutoSize = false;
            localSub.Size = new Size(160, 28);

            localText.Controls.Add(localTitle, 0, 0);
            localText.Controls.Add(localSub, 0, 1);
            localCard.Controls.Add(localIcon);
            localCard.Controls.Add(localText);
            sidebar.Controls.Add(localCard, 0, 3);

            return sidebar;
        }

        Control BuildContentArea()
        {
            var content = new Panel();
            content.Dock = DockStyle.Fill;
            content.BackColor = AppBackground;
            content.Margin = Padding.Empty;

            var pageHost = new Panel();
            pageHost.Dock = DockStyle.Fill;
            pageHost.BackColor = AppBackground;
            pageHost.Margin = Padding.Empty;

            pages.Add(BuildEnvironmentPage());
            pages.Add(BuildDownloadPage());
            pages.Add(BuildAboutPage());
            foreach (Panel page in pages)
            {
                page.Dock = DockStyle.Fill;
                page.Visible = false;
                pageHost.Controls.Add(page);
            }

            var footer = new Panel();
            footer.Dock = DockStyle.Bottom;
            footer.Height = 34;
            footer.BackColor = Color.FromArgb(251, 252, 252);
            footer.Padding = new Padding(24, 0, 24, 0);
            footer.Paint += delegate(object sender, PaintEventArgs e)
            {
                using (var pen = new Pen(Border))
                    e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };

            statusLabel = MakeLabel(L.T("就绪", "Ready"), 8.5F, FontStyle.Regular, TextMuted);
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            footer.Controls.Add(statusLabel);

            var footerVersion = MakeLabel(L.T("Ctrl+F 搜索  ·  F5 扫描", "Ctrl+F Search  ·  F5 Scan") + "  ·  v" + Application.ProductVersion,
                8.2F, FontStyle.Regular, Color.FromArgb(137, 150, 145));
            footerVersion.Dock = DockStyle.Right;
            footerVersion.Width = 240;
            footerVersion.TextAlign = ContentAlignment.MiddleRight;
            footer.Controls.Add(footerVersion);

            content.Controls.Add(pageHost);
            content.Controls.Add(footer);
            return content;
        }

        Panel BuildEnvironmentPage()
        {
            var page = MakePage();
            var layout = MakePageLayout(4);
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(BuildHeader(
                L.T("环境概览", "Overview"),
                L.T("检查本机开发工具、运行时与安装位置", "Check installed developer tools, runtimes and locations")), 0, 0);

            var toolbar = new TableLayoutPanel();
            toolbar.Dock = DockStyle.Fill;
            toolbar.Margin = new Padding(0, 0, 0, 6);
            toolbar.ColumnCount = 2;
            toolbar.RowCount = 1;
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            toolbar.BackColor = AppBackground;

            var actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.LeftToRight;
            actions.WrapContents = false;
            actions.Margin = Padding.Empty;
            actions.BackColor = AppBackground;

            btnScan = MakeButton(L.T("扫描环境", "Scan"), true, 112);
            btnScan.Click += delegate { StartScan(); };
            actions.Controls.Add(btnScan);

            btnManage = MakeButton(L.T("管理所选", "Manage"), false, 116);
            btnManage.Enabled = false;
            actions.Controls.Add(btnManage);

            var searchHost = new FlowLayoutPanel();
            searchHost.Dock = DockStyle.Fill;
            searchHost.FlowDirection = FlowDirection.RightToLeft;
            searchHost.WrapContents = false;
            searchHost.Margin = Padding.Empty;
            searchHost.Padding = new Padding(0, 2, 0, 0);
            searchHost.BackColor = AppBackground;
            envSearch = MakeSearchBox(L.T("搜索环境、语言或路径", "Search name, language or path"), 286);
            envSearch.TextChanged += delegate { ApplyEnvironmentFilter(); };
            searchHost.Controls.Add(envSearch);
            FitSearchBox(searchHost, envSearch, 286);

            toolbar.Controls.Add(actions, 0, 0);
            toolbar.Controls.Add(searchHost, 1, 0);
            layout.Controls.Add(toolbar, 0, 1);

            var summary = new BorderedPanel();
            summary.Dock = DockStyle.Fill;
            summary.Margin = new Padding(0, 0, 0, 10);
            summary.BackColor = Surface;
            summary.BorderColor = Border;
            summary.Padding = new Padding(1);

            var metrics = new TableLayoutPanel();
            metrics.Dock = DockStyle.Fill;
            metrics.ColumnCount = 3;
            metrics.RowCount = 1;
            metrics.Padding = Padding.Empty;
            metrics.Margin = Padding.Empty;
            metrics.BackColor = Surface;
            for (int i = 0; i < 3; i++)
                metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            summaryCount = BuildMetric(metrics, 0, L.T("已发现环境", "Environments"), "0");
            summaryLanguages = BuildMetric(metrics, 1, L.T("语言分类", "Languages"), "0");
            summaryLastScan = BuildMetric(metrics, 2, L.T("最近扫描", "Last scan"), L.T("尚未扫描", "Not scanned"));
            summary.Controls.Add(metrics);
            layout.Controls.Add(summary, 0, 2);

            var tableSurface = new BorderedPanel();
            tableSurface.Dock = DockStyle.Fill;
            tableSurface.BackColor = Surface;
            tableSurface.BorderColor = Border;
            tableSurface.Padding = new Padding(1);

            envGrid = MakeEnvironmentGrid();
            envEmpty = MakeEmptyState(L.T("尚未扫描环境", "No environments scanned yet"),
                L.T("点击“扫描环境”开始检查本机开发环境", "Click “Scan” to check this machine"),
                L.T("扫描环境", "Scan"), delegate { StartScan(); }, Accent);
            tableSurface.Controls.Add(envGrid);
            tableSurface.Controls.Add(envEmpty);
            envEmpty.BringToFront();
            layout.Controls.Add(tableSurface, 0, 3);

            buildManageMenu();
            envGrid.ContextMenuStrip = manageMenu;
            envGrid.SelectionChanged += delegate { UpdateEnvironmentSelection(); };
            envGrid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) OpenSelectedLocation();
            };
            btnManage.Click += delegate
            {
                manageMenu.Show(btnManage, new Point(0, btnManage.Height));
            };

            page.Controls.Add(layout);
            return page;
        }

        Panel BuildDownloadPage()
        {
            var page = MakePage();
            var layout = MakePageLayout(4);
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(BuildHeader(
                L.T("下载中心", "Downloads"),
                L.T("查找并访问开发环境的官方下载页面", "Find and open official download pages")), 0, 0);

            var toolbar = new TableLayoutPanel();
            toolbar.Dock = DockStyle.Fill;
            toolbar.Margin = new Padding(0, 0, 0, 6);
            toolbar.ColumnCount = 2;
            toolbar.RowCount = 1;
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            toolbar.BackColor = AppBackground;

            var actions = new FlowLayoutPanel();
            actions.Dock = DockStyle.Fill;
            actions.WrapContents = false;
            actions.Margin = Padding.Empty;
            actions.BackColor = AppBackground;
            btnVisitSite = MakeButton(L.T("访问官网", "Open Website"), true, 112);
            btnVisitSite.Enabled = false;
            btnVisitSite.Click += delegate { OpenSelectedProduct(); };
            actions.Controls.Add(btnVisitSite);

            var searchHost = new FlowLayoutPanel();
            searchHost.Dock = DockStyle.Fill;
            searchHost.FlowDirection = FlowDirection.RightToLeft;
            searchHost.WrapContents = false;
            searchHost.Padding = new Padding(0, 2, 0, 0);
            searchHost.Margin = Padding.Empty;
            searchHost.BackColor = AppBackground;
            downloadSearch = MakeSearchBox(L.T("搜索环境或语言", "Search name or language"), 286);
            downloadSearch.TextChanged += delegate { ApplyDownloadFilter(); };
            searchHost.Controls.Add(downloadSearch);
            FitSearchBox(searchHost, downloadSearch, 286);

            toolbar.Controls.Add(actions, 0, 0);
            toolbar.Controls.Add(searchHost, 1, 0);
            layout.Controls.Add(toolbar, 0, 1);

            var notice = new Panel();
            notice.Dock = DockStyle.Fill;
            notice.Margin = new Padding(0, 0, 0, 10);
            notice.BackColor = Color.FromArgb(238, 247, 245);
            notice.Padding = new Padding(14, 0, 14, 0);
            var noticeText = MakeLabel(
                L.T("所有链接均指向软件官方网站，NS 不提供或托管安装包。",
                    "All links point to official websites. NS does not provide or host installers."),
                9F, FontStyle.Regular, Color.FromArgb(35, 104, 93));
            noticeText.Dock = DockStyle.Fill;
            noticeText.TextAlign = ContentAlignment.MiddleLeft;
            notice.Controls.Add(noticeText);
            layout.Controls.Add(notice, 0, 2);

            var tableSurface = new BorderedPanel();
            tableSurface.Dock = DockStyle.Fill;
            tableSurface.BackColor = Surface;
            tableSurface.BorderColor = Border;
            tableSurface.Padding = new Padding(1);

            downloadGrid = MakeDownloadGrid();
            downloadEmpty = MakeEmptyState(L.T("没有符合条件的环境", "No matching environments"),
                L.T("请调整搜索关键字", "Try a different search"), null, null, Blue);
            tableSurface.Controls.Add(downloadGrid);
            tableSurface.Controls.Add(downloadEmpty);
            downloadEmpty.BringToFront();
            layout.Controls.Add(tableSurface, 0, 3);

            downloadGrid.SelectionChanged += delegate { UpdateDownloadSelection(); };
            downloadGrid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) OpenSelectedProduct();
            };
            downloadGrid.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter) OpenSelectedProduct();
            };

            page.Controls.Add(layout);
            return page;
        }

        Panel BuildAboutPage()
        {
            var page = MakePage();
            var layout = MakePageLayout(2);
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Controls.Add(BuildHeader(
                L.T("关于", "About"),
                L.T("产品信息与功能说明", "Product information and features")), 0, 0);

            var body = new FlowLayoutPanel();
            body.Dock = DockStyle.Fill;
            body.AutoScroll = true;
            body.FlowDirection = FlowDirection.TopDown;
            body.WrapContents = false;
            body.BackColor = AppBackground;
            body.Margin = Padding.Empty;
            body.Padding = new Padding(0);
            layout.Controls.Add(body, 0, 1);

            var product = MakeAboutSection(180);
            AddSectionTitle(product, L.T("产品信息", "Product"));
            AddSectionText(product,
                L.T("NS 开发环境管理器\n版本 " + Application.ProductVersion + "  ·  Windows 桌面应用",
                    "NS Dev Environment Manager\nVersion " + Application.ProductVersion + "  ·  Windows desktop app"));
            AddSectionText(product,
                L.T("扫描注册表、PATH 与常用安装目录，集中呈现开发环境和安装位置。",
                    "Scans the registry, PATH and common install folders to present dev environments and locations."));
            body.Controls.Add(product);

            var security = MakeAboutSection(190);
            AddSectionTitle(security, L.T("数据与操作", "Data & operations"));
            AddSectionText(security,
                L.T("扫描仅读取本机已安装环境的信息，不修改注册表、PATH 或环境变量。\n卸载前会要求确认，并调用软件登记的官方卸载程序；官网入口通过默认浏览器打开。",
                    "Scanning is read-only and never modifies the registry, PATH or environment variables.\nUninstall asks for confirmation and runs the registered official uninstaller; website links open in the default browser."));
            body.Controls.Add(security);

            Action resizeSections = delegate
            {
                int width = Math.Max(300, body.ClientSize.Width - 18);
                foreach (Control child in body.Controls)
                {
                    child.Width = width;
                    LayoutAboutSection(child as Panel);
                }
            };
            body.Resize += delegate { resizeSections(); };
            resizeSections();

            page.Controls.Add(layout);
            return page;
        }

        Panel MakePage()
        {
            var page = new Panel();
            page.BackColor = AppBackground;
            page.Margin = Padding.Empty;
            page.Padding = Padding.Empty;
            return page;
        }

        TableLayoutPanel MakePageLayout(int rows)
        {
            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = rows;
            layout.Padding = new Padding(26, 22, 26, 18);
            layout.Margin = Padding.Empty;
            layout.BackColor = AppBackground;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            return layout;
        }

        Control BuildHeader(string title, string subtitle)
        {
            var header = new TableLayoutPanel();
            header.Dock = DockStyle.Fill;
            header.AutoSize = true;
            header.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            header.ColumnCount = 1;
            header.RowCount = 2;
            header.Margin = Padding.Empty;
            header.Padding = new Padding(0, 0, 0, 20);
            header.BackColor = AppBackground;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            header.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var titleLabel = MakeLabel(title, 20F, FontStyle.Bold, TextPrimary);
            titleLabel.Margin = Padding.Empty;
            var subtitleLabel = MakeLabel(subtitle, 9F, FontStyle.Regular, TextMuted);
            subtitleLabel.Margin = new Padding(1, 3, 0, 0);
            header.Controls.Add(titleLabel, 0, 0);
            header.Controls.Add(subtitleLabel, 0, 1);
            return header;
        }

        Label BuildMetric(TableLayoutPanel host, int column, string caption, string value)
        {
            var cell = new TableLayoutPanel();
            cell.Dock = DockStyle.Fill;
            cell.Margin = new Padding(column == 0 ? 0 : 1, 0, 1, 0);
            cell.BackColor = Surface;
            cell.ColumnCount = 1;
            cell.RowCount = 2;
            cell.Padding = new Padding(14, 6, 10, 0);
            cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cell.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            cell.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var captionLabel = MakeLabel(caption, 8F, FontStyle.Regular, TextMuted);
            captionLabel.Margin = Padding.Empty;
            var valueLabel = MakeLabel(value, 13.5F, FontStyle.Bold, TextPrimary);
            valueLabel.Margin = new Padding(0, 1, 0, 0);
            cell.Controls.Add(captionLabel, 0, 0);
            cell.Controls.Add(valueLabel, 0, 1);
            host.Controls.Add(cell, column, 0);
            return valueLabel;
        }

        DataGridView MakeEnvironmentGrid()
        {
            var grid = MakeGrid();
            grid.Columns.Add(MakeColumn("language", L.T("语言", "Language"), 13F));
            grid.Columns.Add(MakeColumn("name", L.T("环境", "Environment"), 16F));
            grid.Columns.Add(MakeColumn("version", L.T("版本", "Version"), 14F));
            grid.Columns.Add(MakeColumn("path", L.T("安装位置", "Install path"), 35F));
            grid.Columns.Add(MakeColumn("source", L.T("识别来源", "Detected via"), 22F));
            return grid;
        }

        DataGridView MakeDownloadGrid()
        {
            var grid = MakeGrid();
            grid.Columns.Add(MakeColumn("downloadName", L.T("环境", "Environment"), 26F));
            grid.Columns.Add(MakeColumn("downloadLanguage", L.T("语言", "Language"), 18F));
            grid.Columns.Add(MakeColumn("downloadUrl", L.T("官方网站", "Official website"), 56F));
            return grid;
        }

        DataGridView MakeGrid()
        {
            var grid = new BufferedDataGridView();
            grid.Dock = DockStyle.Fill;
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToOrderColumns = false;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.BackgroundColor = Surface;
            grid.GridColor = Color.FromArgb(235, 239, 242);
            grid.ColumnHeadersHeight = 40;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.RowTemplate.Height = 38;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = AccentSoft;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(10, 0, 8, 0);
            grid.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            grid.AlternatingRowsDefaultCellStyle.BackColor = SoftFill;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 250);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(82, 96, 109);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold, GraphicsUnit.Point);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 8, 0);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 250);
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(82, 96, 109);
            return grid;
        }

        DataGridViewTextBoxColumn MakeColumn(string name, string header, float weight)
        {
            var column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = header;
            column.FillWeight = weight;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.Resizable = DataGridViewTriState.True;
            return column;
        }

        EmptyStatePanel MakeEmptyState(string title, string detail, string actionText, EventHandler action, Color tint)
        {
            var panel = new EmptyStatePanel("\uE8A5", title, detail, actionText, action, tint);
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Surface;
            return panel;
        }

        SearchBox MakeSearchBox(string cue, int width)
        {
            var box = new SearchBox();
            box.SetCueBanner(cue);
            box.Width = Math.Max(width, box.PreferredWidth);
            return box;
        }

        // 搜索框右对齐：可用宽度不足时收窄，避免超出工具栏单元格；同时不小于提示文字所需宽度。
        void FitSearchBox(FlowLayoutPanel host, SearchBox box, int preferredWidth)
        {
            EventHandler fit = delegate
            {
                int available = host.ClientSize.Width;
                if (available <= 0) return;
                box.Width = Math.Min(Math.Max(preferredWidth, box.PreferredWidth),
                    Math.Max(120, available));
            };
            host.Resize += fit;
            fit(null, EventArgs.Empty);
        }

        RoundedButton MakeButton(string text, bool primary, int width)
        {
            var button = new RoundedButton();
            button.Text = text;
            button.Width = width;
            button.ButtonStyle = primary ? AppButtonStyle.Primary : AppButtonStyle.Secondary;
            button.Margin = new Padding(0, 3, 8, 0);
            return button;
        }

        Label MakeLabel(string text, float size, FontStyle style, Color color)
        {
            var label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Font = new Font("Microsoft YaHei UI", size, style, GraphicsUnit.Point);
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            return label;
        }

        void buildManageMenu()
        {
            manageMenu = new ContextMenuStrip();
            manageMenu.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            manageMenu.ShowImageMargin = false;
            manageMenu.Items.Add(L.T("在资源管理器中显示", "Show in Explorer"), null, delegate { OpenSelectedLocation(); });
            manageMenu.Items.Add(L.T("访问官方网站", "Open official website"), null, delegate { OpenSelectedEnvironmentSite(); });
            manageMenu.Items.Add(new ToolStripSeparator());
            manageMenu.Items.Add(L.T("启动官方卸载", "Run official uninstaller"), null, delegate { UninstallSelected(); });
            manageMenu.Opening += delegate
            {
                EnvItem item = SelectedEnvironment();
                if (item == null)
                {
                    for (int i = 0; i < manageMenu.Items.Count; i++) manageMenu.Items[i].Enabled = false;
                    return;
                }
                manageMenu.Items[0].Enabled = !string.IsNullOrEmpty(item.Path) && Directory.Exists(item.Path);
                manageMenu.Items[1].Enabled = !string.IsNullOrEmpty(item.Url);
                manageMenu.Items[3].Enabled = !string.IsNullOrEmpty(item.Uninstall);
            };
        }

        void LoadDownloadCatalog()
        {
            downloadItems.AddRange(Catalog.Products);
            downloadItems.Sort(delegate(Product a, Product b)
            {
                int c = string.Compare(a.Language, b.Language, StringComparison.OrdinalIgnoreCase);
                if (c != 0) return c;
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
        }

        void StartScan()
        {
            if (scanning) return;
            scanning = true;
            btnScan.Enabled = false;
            btnScan.Text = L.T("正在扫描", "Scanning");
            envSearch.Enabled = false;
            manageMenu.Enabled = false;
            statusLabel.Text = L.T("正在扫描注册表、PATH 与常用安装目录...",
                "Scanning registry, PATH and common install folders...");

            var thread = new Thread(delegate()
            {
                List<EnvItem> items = null;
                string error = "";
                try { items = Scanner.ScanAll(SetStatus); }
                catch (Exception ex) { error = ex.Message; }

                SafeInvoke(delegate
                {
                    scanning = false;
                    btnScan.Enabled = true;
                    btnScan.Text = L.T("扫描环境", "Scan");
                    envSearch.Enabled = true;
                    manageMenu.Enabled = true;
                    if (error.Length > 0)
                    {
                        statusLabel.Text = L.T("扫描未完成", "Scan incomplete");
                        MessageBox.Show(this,
                            L.T("扫描未能完成。\n\n详细信息：", "Scan did not complete.\n\nDetails: ") + error,
                            L.T("NS · 扫描失败", "NS · Scan failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    allItems.Clear();
                    allItems.AddRange(items);
                    lastScan = DateTime.Now;
                    ApplyEnvironmentFilter();
                    statusLabel.Text = L.T("扫描完成，共发现 ", "Scan complete: ") + allItems.Count +
                        L.T(" 个环境", " environments");
                });
            });
            thread.IsBackground = true;
            thread.Start();
        }

        void SetStatus(string text)
        {
            SafeInvoke(delegate { statusLabel.Text = text; });
        }

        void SafeInvoke(Action action)
        {
            if (IsDisposed) return;
            try
            {
                if (InvokeRequired) BeginInvoke(action);
                else action();
            }
            catch (InvalidOperationException) { }
        }

        void ApplyEnvironmentFilter()
        {
            if (envGrid == null) return;
            string query = envSearch == null ? "" : envSearch.Text.Trim();
            envGrid.Rows.Clear();
            int shown = 0;
            foreach (EnvItem item in allItems)
            {
                if (!Matches(item, query)) continue;
                int index = envGrid.Rows.Add(
                    L.Cat(item.Language),
                    item.Name,
                    Disp(item.Version, "未知", "Unknown"),
                    Disp(item.Path, "未记录", "Not recorded"),
                    Disp(item.Source, "未知", "Unknown"));
                envGrid.Rows[index].Tag = item;
                shown++;
            }

            var languageSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EnvItem item in allItems) languageSet.Add(item.Language);

            summaryCount.Text = allItems.Count.ToString();
            summaryLanguages.Text = languageSet.Count.ToString();
            summaryLastScan.Text = lastScan.HasValue
                ? lastScan.Value.ToString("MM-dd HH:mm")
                : L.T("尚未扫描", "Not scanned");
            bool scanned = allItems.Count > 0;
            envEmpty.SetContent(
                scanned ? L.T("没有符合条件的环境", "No matching environments") : L.T("尚未扫描环境", "No environments scanned yet"),
                scanned ? L.T("请调整搜索关键字", "Try a different search") : L.T("点击“扫描环境”开始检查本机开发环境", "Click “Scan” to check this machine"));
            envEmpty.ShowAction(!scanned);
            envEmpty.Visible = shown == 0;
            if (shown == 0) envEmpty.BringToFront();
            UpdateEnvironmentSelection();
            if (allItems.Count > 0)
                statusLabel.Text = query.Length == 0
                    ? L.T("共发现 ", "Found ") + allItems.Count + L.T(" 个环境，按语言与名称排序", " environments, sorted by language and name")
                    : L.T("显示 ", "Showing ") + shown + " / " + allItems.Count + L.T(" 个环境", " environments");
        }

        void ApplyDownloadFilter()
        {
            if (downloadGrid == null) return;
            string query = downloadSearch.Text.Trim();
            downloadGrid.Rows.Clear();
            int shown = 0;
            foreach (Product product in downloadItems)
            {
                if (query.Length > 0 &&
                    product.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    product.Language.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                int index = downloadGrid.Rows.Add(product.Name, L.Cat(product.Language), product.Url);
                downloadGrid.Rows[index].Tag = product;
                shown++;
            }
            downloadEmpty.Visible = shown == 0;
            if (shown == 0) downloadEmpty.BringToFront();
            UpdateDownloadSelection();
            if (downloadSearch.Text.Length == 0)
                statusLabel.Text = L.T("已载入 ", "Loaded ") + downloadItems.Count + L.T(" 个官方下载入口", " official download entries");
            else
                statusLabel.Text = L.T("显示 ", "Showing ") + shown + " / " + downloadItems.Count + L.T(" 个官方下载入口", " official download entries");
        }

        static string Disp(string value, string zh, string en)
        {
            return string.IsNullOrEmpty(value) ? L.T(zh, en) : value;
        }

        static bool Matches(EnvItem item, string query)
        {
            if (query.Length == 0) return true;
            return Contains(item.Name, query) || Contains(item.Language, query) ||
                Contains(L.Cat(item.Language), query) ||
                Contains(item.Version, query) || Contains(item.Path, query) || Contains(item.Source, query);
        }

        static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void UpdateEnvironmentSelection()
        {
            EnvItem item = SelectedEnvironment();
            if (btnManage != null) btnManage.Enabled = item != null;
            if (item == null) return;
            statusLabel.Text = item.Name + "  ·  " + Disp(item.Version, "未知", "Unknown") + "  ·  " +
                (string.IsNullOrEmpty(item.Path)
                    ? L.T("安装位置未记录", "Install path not recorded")
                    : item.Path);
        }

        void UpdateDownloadSelection()
        {
            Product product = SelectedProduct();
            if (btnVisitSite != null) btnVisitSite.Enabled = product != null;
            if (product == null) return;
            statusLabel.Text = product.Name + "  ·  " + product.Url;
        }

        EnvItem SelectedEnvironment()
        {
            if (envGrid == null || envGrid.SelectedRows.Count == 0) return null;
            return envGrid.SelectedRows[0].Tag as EnvItem;
        }

        Product SelectedProduct()
        {
            if (downloadGrid == null || downloadGrid.SelectedRows.Count == 0) return null;
            return downloadGrid.SelectedRows[0].Tag as Product;
        }

        void OpenSelectedLocation()
        {
            EnvItem item = SelectedEnvironment();
            if (item == null) return;
            if (string.IsNullOrEmpty(item.Path) || !Directory.Exists(item.Path))
            {
                MessageBox.Show(this,
                    L.T("未找到该环境的安装目录。\n\n", "Install folder not found.\n\n") + item.Name,
                    L.T("NS · 安装位置", "NS · Install path"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + item.Path + "\"") { UseShellExecute = true });
                statusLabel.Text = L.T("已打开安装位置：", "Opened install path: ") + item.Path;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    L.T("无法打开安装目录。\n\n详细信息：", "Could not open install folder.\n\nDetails: ") + ex.Message,
                    L.T("NS · 打开失败", "NS · Open failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenSelectedEnvironmentSite()
        {
            EnvItem item = SelectedEnvironment();
            if (item == null) return;
            if (string.IsNullOrEmpty(item.Url))
            {
                MessageBox.Show(this,
                    L.T("该环境没有可用的官方网站。", "No official website for this environment."),
                    L.T("NS · 官网入口", "NS · Website"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            OpenUrl(item.Url);
        }

        void OpenSelectedProduct()
        {
            Product product = SelectedProduct();
            if (product == null) return;
            OpenUrl(product.Url);
        }

        void UninstallSelected()
        {
            EnvItem item = SelectedEnvironment();
            if (item == null) return;
            if (string.IsNullOrEmpty(item.Uninstall))
            {
                MessageBox.Show(this,
                    item.Name + L.T(" 未登记官方卸载命令。\n请前往“设置 > 应用”完成卸载。",
                        " has no registered uninstall command.\nUse Windows Settings > Apps to uninstall."),
                    L.T("NS · 缺少卸载信息", "NS · Missing uninstall info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(this,
                L.T("将启动 ", "Run the official uninstaller for ") + item.Name +
                L.T(" 的官方卸载程序：\n\n", "?\n\n") + item.Uninstall +
                L.T("\n\nNS 不会直接删除任何文件。是否继续？", "\n\nNS never deletes files directly. Continue?"),
                L.T("NS · 确认卸载", "NS · Confirm uninstall"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;

            try
            {
                string exe;
                string arguments;
                SplitCommand(item.Uninstall, out exe, out arguments);
                Process.Start(new ProcessStartInfo(exe, arguments) { UseShellExecute = true });
                statusLabel.Text = L.T("已启动官方卸载：", "Uninstaller started: ") + item.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    L.T("无法启动官方卸载程序。\n\n详细信息：", "Could not start the uninstaller.\n\nDetails: ") + ex.Message,
                    L.T("NS · 卸载失败", "NS · Uninstall failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void SplitCommand(string command, out string exe, out string arguments)
        {
            command = command.Trim();
            if (command.StartsWith("\""))
            {
                int end = command.IndexOf('"', 1);
                if (end > 0)
                {
                    exe = command.Substring(1, end - 1);
                    arguments = command.Substring(end + 1).Trim();
                    return;
                }
            }
            int space = command.IndexOf(' ');
            if (space > 0)
            {
                exe = command.Substring(0, space);
                arguments = command.Substring(space + 1).Trim();
            }
            else
            {
                exe = command;
                arguments = "";
            }
        }

        static void OpenUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return;
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(L.T("无法打开浏览器。\n\n详细信息：", "Could not open browser.\n\nDetails: ") + ex.Message,
                    L.T("NS · 打开失败", "NS · Open failed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void ShowPage(int index)
        {
            for (int i = 0; i < pages.Count; i++) pages[i].Visible = i == index;
            for (int i = 0; i < navButtons.Count; i++)
            {
                navButtons[i].Active = i == index;
                navButtons[i].Invalidate();
            }
            if (index == 1) ApplyDownloadFilter();
            else if (index == 0 && allItems.Count == 0 && statusLabel != null)
                statusLabel.Text = L.T("就绪，点击“扫描环境”开始检查", "Ready. Click “Scan” to check this machine");
        }

        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                ShowPage(0);
                if (!scanning) StartScan();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                SearchBox search = pages[0].Visible ? envSearch : downloadSearch;
                if (search != null && search.Enabled)
                {
                    search.Focus();
                    search.SelectAll();
                }
                e.Handled = true;
            }
        }

        static Icon BuildAppIcon()
        {
            using (var bmp = new Bitmap(32, 32))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    using (var path = UiGeometry.RoundedRect(new Rectangle(0, 0, 31, 31), 7))
                    using (var brush = new SolidBrush(Color.FromArgb(14, 124, 102)))
                        g.FillPath(brush, path);
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    using (var font = new Font("Segoe UI", 13F, FontStyle.Bold))
                        g.DrawString("NS", font, Brushes.White, new RectangleF(0, 0, 32, 32), sf);
                }
                using (var icon = Icon.FromHandle(bmp.GetHicon()))
                    return (Icon)icon.Clone();
            }
        }

        void AddSectionTitle(Panel section, string text)
        {
            var label = MakeLabel(text, 11F, FontStyle.Bold, TextPrimary);
            label.AutoSize = false;
            label.UseMnemonic = false;
            section.Controls.Add(label);
        }

        void AddSectionText(Panel section, string text)
        {
            var label = MakeLabel(text, 9F, FontStyle.Regular, TextMuted);
            label.AutoSize = false;
            label.UseMnemonic = false;
            section.Controls.Add(label);
        }

        void LayoutAboutSection(Panel section)
        {
            if (section == null || section.IsDisposed) return;

            // 所有文字按顺序自上而下排列，高度由实际字体度量决定，避免高 DPI 或系统字体放大时重叠、裁切。
            int width = Math.Max(240, section.ClientSize.Width - 40);
            int y = 15;
            int index = 0;
            foreach (Control control in section.Controls)
            {
                Label label = control as Label;
                if (label == null) continue;

                Size measured = TextRenderer.MeasureText(
                    label.Text,
                    label.Font,
                    new Size(width, 10000),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                label.SetBounds(20, y, width, Math.Max(20, measured.Height + 6));
                y = label.Bottom + (index == 0 ? 12 : 10);
                index++;
            }

            section.Height = Math.Max(110, y + 4);
        }

        Panel MakeAboutSection(int height)
        {
            var section = new BorderedPanel();
            section.Height = height;
            section.Width = 760;
            section.Margin = new Padding(0, 0, 0, 12);
            section.BackColor = Surface;
            section.BorderColor = Border;
            section.Padding = new Padding(1);
            return section;
        }

        class BorderedPanel : Panel
        {
            public Color BorderColor = Border;

            public BorderedPanel()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (var pen = new Pen(BorderColor))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        class BufferedDataGridView : DataGridView
        {
            public BufferedDataGridView()
            {
                DoubleBuffered = true;
            }
        }
    }
}

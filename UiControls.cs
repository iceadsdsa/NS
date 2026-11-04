using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NS
{
    internal enum AppButtonStyle
    {
        Primary,
        Secondary,
        Quiet
    }

    internal class RoundedPanel : Panel
    {
        public int CornerRadius = 8;
        public Color BorderColor = Color.Transparent;
        public int BorderWidth = 0;

        public RoundedPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            using (GraphicsPath path = UiGeometry.RoundedRect(rect, CornerRadius))
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
                if (BorderWidth > 0)
                {
                    using (var pen = new Pen(BorderColor, BorderWidth))
                        e.Graphics.DrawPath(pen, path);
                }
            }
        }
    }

    internal class RoundedButton : Button
    {
        public AppButtonStyle ButtonStyle = AppButtonStyle.Secondary;
        public string Glyph = "";
        public Color PrimaryColor = Color.FromArgb(14, 124, 102);
        public Color PrimaryHoverColor = Color.FromArgb(11, 104, 85);
        public Color PrimaryPressedColor = Color.FromArgb(9, 86, 70);
        public Color SecondaryBorderColor = Color.FromArgb(218, 227, 223);
        public Color SecondaryHoverColor = Color.FromArgb(244, 248, 246);
        public Color TextColor = Color.FromArgb(31, 43, 39);
        public Color MutedColor = Color.FromArgb(107, 121, 116);

        bool hovering;
        bool pressing;

        public RoundedButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
            Height = 38;
            MinimumSize = new Size(80, 34);
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            TabStop = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            pressing = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left) pressing = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            pressing = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            Color background;
            Color border = Color.Transparent;
            Color foreground;

            if (!Enabled)
            {
                background = ButtonStyle == AppButtonStyle.Primary
                    ? Color.FromArgb(186, 207, 200)
                    : Color.FromArgb(244, 247, 246);
                foreground = ButtonStyle == AppButtonStyle.Primary
                    ? Color.FromArgb(248, 251, 250)
                    : Color.FromArgb(157, 169, 164);
                if (ButtonStyle == AppButtonStyle.Secondary)
                    border = Color.FromArgb(230, 236, 233);
            }
            else if (ButtonStyle == AppButtonStyle.Primary)
            {
                background = pressing ? PrimaryPressedColor : hovering ? PrimaryHoverColor : PrimaryColor;
                foreground = Color.White;
            }
            else if (ButtonStyle == AppButtonStyle.Quiet)
            {
                background = pressing ? Color.FromArgb(231, 238, 235) : hovering ? SecondaryHoverColor : Color.Transparent;
                foreground = TextColor;
            }
            else
            {
                background = pressing ? Color.FromArgb(235, 242, 239) : hovering ? SecondaryHoverColor : Color.White;
                border = SecondaryBorderColor;
                foreground = TextColor;
            }

            using (GraphicsPath path = UiGeometry.RoundedRect(rect, 7))
            {
                if (background.A > 0)
                {
                    using (var brush = new SolidBrush(background))
                        pevent.Graphics.FillPath(brush, path);
                }
                if (border.A > 0)
                {
                    using (var pen = new Pen(border))
                        pevent.Graphics.DrawPath(pen, path);
                }
            }

            bool hasGlyph = !string.IsNullOrEmpty(Glyph);
            Font glyphFont = new Font("Segoe MDL2 Assets", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            Font textFont = Font;
            Size glyphSize = hasGlyph ? TextRenderer.MeasureText(Glyph, glyphFont) : Size.Empty;
            Size textSize = TextRenderer.MeasureText(Text, textFont);
            int gap = hasGlyph && Text.Length > 0 ? 8 : 0;
            int totalWidth = glyphSize.Width + gap + textSize.Width;
            int startX = Math.Max(8, (ClientSize.Width - totalWidth) / 2);
            int centerY = ClientSize.Height / 2;

            try
            {
                if (hasGlyph)
                {
                    TextRenderer.DrawText(pevent.Graphics, Glyph, glyphFont,
                        new Rectangle(startX, 0, glyphSize.Width, ClientSize.Height),
                        foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    startX += glyphSize.Width + gap;
                }
                if (Text.Length > 0)
                {
                    TextRenderer.DrawText(pevent.Graphics, Text, textFont,
                        new Rectangle(startX, 0, Math.Max(0, ClientSize.Width - startX - 8), ClientSize.Height),
                        foreground, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding |
                        TextFormatFlags.EndEllipsis);
                }
            }
            finally
            {
                glyphFont.Dispose();
            }
        }
    }

    internal class NavButton : Button
    {
        public bool Active;
        public string Glyph = "";
        public Color IdleBackground = Color.FromArgb(22, 35, 31);
        public Color ActiveBackground = Color.FromArgb(35, 55, 48);
        public Color IdleTextColor = Color.FromArgb(190, 204, 198);
        public Color ActiveTextColor = Color.White;
        public Color AccentColor = Color.FromArgb(78, 196, 162);

        bool hovering;

        public NavButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
            Height = 44;
            Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            TextAlign = ContentAlignment.MiddleLeft;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Color background = Active
                ? ActiveBackground
                : hovering ? Color.FromArgb(29, 45, 40) : IdleBackground;
            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            using (GraphicsPath path = UiGeometry.RoundedRect(rect, 7))
            using (var brush = new SolidBrush(background))
                pevent.Graphics.FillPath(brush, path);

            if (Active)
            {
                using (var brush = new SolidBrush(AccentColor))
                    pevent.Graphics.FillRectangle(brush, 0, 12, 3, ClientSize.Height - 24);
            }

            Font glyphFont = new Font("Segoe MDL2 Assets", 12F, FontStyle.Regular, GraphicsUnit.Point);
            try
            {
                TextRenderer.DrawText(pevent.Graphics, Glyph, glyphFont,
                    new Rectangle(15, 0, 22, ClientSize.Height),
                    Active ? AccentColor : Color.FromArgb(125, 151, 142),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(pevent.Graphics, Text, Font,
                    new Rectangle(48, 0, Math.Max(0, ClientSize.Width - 58), ClientSize.Height),
                    Active ? ActiveTextColor : IdleTextColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
            finally
            {
                glyphFont.Dispose();
            }
        }
    }

    internal class IconBadge : Control
    {
        public string Glyph = "";
        public Color FillColor = Color.FromArgb(232, 246, 241);
        public Color GlyphColor = Color.FromArgb(14, 124, 102);
        public int CornerRadius = 8;

        public IconBadge()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(38, 38);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            using (GraphicsPath path = UiGeometry.RoundedRect(rect, CornerRadius))
            using (var brush = new SolidBrush(FillColor))
                e.Graphics.FillPath(brush, path);

            using (var font = new Font("Segoe MDL2 Assets", 13F, FontStyle.Regular, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(e.Graphics, Glyph, font, ClientRectangle, GlyphColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    internal class SearchBox : UserControl
    {
        readonly TextBox input;
        string cue = "";
        bool focused;

        public SearchBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.White;
            Height = 38;
            Width = 286;
            Padding = Padding.Empty;
            Margin = Padding.Empty;

            input = new TextBox();
            input.BorderStyle = BorderStyle.None;
            input.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            input.ForeColor = Color.FromArgb(31, 43, 39);
            input.BackColor = Color.White;
            input.Location = new Point(36, 10);
            input.TextChanged += delegate
            {
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            };
            input.GotFocus += delegate
            {
                focused = true;
                Invalidate();
            };
            input.LostFocus += delegate
            {
                focused = false;
                Invalidate();
            };
            input.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                OnKeyDown(e);
            };
            Controls.Add(input);
            LayoutInput();
        }

        public int PreferredWidth
        {
            get
            {
                // 图标 36px + 右侧留白 18px，保证提示文字（cue banner）完整显示。
                return TextRenderer.MeasureText(cue, input.Font).Width + 54;
            }
        }

        public override string Text
        {
            get { return input.Text; }
            set
            {
                input.Text = value ?? "";
                Invalidate();
            }
        }

        public void SetCueBanner(string text)
        {
            cue = text ?? "";
            if (input.IsHandleCreated) ApplyCue();
            else input.HandleCreated += delegate { ApplyCue(); };
            Width = Math.Max(Width, PreferredWidth);
            LayoutInput();
        }

        void ApplyCue()
        {
            User32.SendMessage(input.Handle, User32.EM_SETCUEBANNER, new IntPtr(1), cue);
        }

        void LayoutInput()
        {
            if (input == null) return;
            int height = input.PreferredHeight;
            input.SetBounds(36, Math.Max(6, (ClientSize.Height - height) / 2),
                Math.Max(30, ClientSize.Width - 48), height);
        }

        public new void Focus()
        {
            input.Focus();
        }

        public void SelectAll()
        {
            input.SelectAll();
        }

        public bool InputEnabled
        {
            get { return input.Enabled; }
            set { input.Enabled = value; }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            input.Enabled = Enabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutInput();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Rectangle rect = ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            Color fill = Enabled ? (focused ? Color.FromArgb(252, 254, 253) : Color.White)
                : Color.FromArgb(246, 248, 247);
            Color border = focused ? Color.FromArgb(14, 124, 102)
                : Color.FromArgb(218, 227, 223);

            using (GraphicsPath path = UiGeometry.RoundedRect(rect, 7))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border, focused ? 1.5F : 1F))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }

            using (var glyphFont = new Font("Segoe MDL2 Assets", 11F, FontStyle.Regular, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(e.Graphics, "\uE721", glyphFont,
                    new Rectangle(13, 0, 18, ClientSize.Height),
                    Enabled ? Color.FromArgb(111, 128, 121) : Color.FromArgb(170, 181, 176),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    internal class EmptyStatePanel : Panel
    {
        readonly Label titleLabel;
        readonly Label detailLabel;
        readonly RoundedButton actionButton;
        string glyph;
        Color glyphColor;

        public EmptyStatePanel(string glyphText, string title, string detail,
            string actionText, EventHandler action, Color tint)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            glyph = glyphText;
            glyphColor = tint;

            titleLabel = new Label();
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
            titleLabel.ForeColor = Color.FromArgb(31, 43, 39);
            titleLabel.BackColor = Color.Transparent;
            titleLabel.Text = title;
            Controls.Add(titleLabel);

            detailLabel = new Label();
            detailLabel.AutoSize = false;
            detailLabel.TextAlign = ContentAlignment.TopCenter;
            detailLabel.Font = new Font("Microsoft YaHei UI", 8.8F, FontStyle.Regular, GraphicsUnit.Point);
            detailLabel.ForeColor = Color.FromArgb(107, 121, 116);
            detailLabel.BackColor = Color.Transparent;
            detailLabel.Text = detail;
            Controls.Add(detailLabel);

            actionButton = new RoundedButton();
            actionButton.ButtonStyle = AppButtonStyle.Primary;
            actionButton.Text = actionText;
            actionButton.Width = 128;
            actionButton.Height = 36;
            actionButton.Visible = action != null;
            if (action != null) actionButton.Click += action;
            Controls.Add(actionButton);
        }

        public void SetContent(string title, string detail)
        {
            titleLabel.Text = title;
            detailLabel.Text = detail;
            Relayout();
        }

        public void ShowAction(bool visible)
        {
            actionButton.Visible = visible;
            Relayout();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Relayout();
        }

        void Relayout()
        {
            if (titleLabel == null) return;
            int centerX = ClientSize.Width / 2;
            int centerY = ClientSize.Height / 2;
            int blockHeight = actionButton.Visible ? 155 : 112;
            int top = Math.Max(18, centerY - blockHeight / 2);
            titleLabel.Location = new Point(centerX - titleLabel.Width / 2, top + 62);
            detailLabel.SetBounds(centerX - Math.Min(270, ClientSize.Width / 2 - 20), top + 88,
                Math.Min(540, ClientSize.Width - 40), 38);
            actionButton.Location = new Point(centerX - actionButton.Width / 2, top + 128);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int centerX = ClientSize.Width / 2;
            int centerY = ClientSize.Height / 2;
            int blockHeight = actionButton.Visible ? 155 : 112;
            int top = Math.Max(18, centerY - blockHeight / 2);
            Rectangle iconRect = new Rectangle(centerX - 24, top, 48, 48);
            using (var brush = new SolidBrush(Color.FromArgb(238, 247, 243)))
                e.Graphics.FillEllipse(brush, iconRect);
            using (var font = new Font("Segoe MDL2 Assets", 19F, FontStyle.Regular, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(e.Graphics, glyph, font, iconRect, glyphColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    internal static class UiGeometry
    {
        public static GraphicsPath RoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            int diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(rect.Width, rect.Height)));
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal static class User32
    {
        public const int EM_SETCUEBANNER = 0x1501;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
    }
}

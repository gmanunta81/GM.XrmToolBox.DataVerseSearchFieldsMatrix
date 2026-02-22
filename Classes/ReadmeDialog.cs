using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace GM.XrmToolBox.DataVerseSearchFieldsMatrix
{
    /// <summary>
    /// Simple readme viewer dialog with a vertical toolbar on the right
    /// to scroll up/down quickly.
    /// </summary>
    internal sealed class ReadmeDialog : Form
    {
        private readonly RichTextBox _rtb;
        private readonly ToolStrip _toolStrip;
        private readonly ToolStripButton _btnUp;
        private readonly ToolStripButton _btnDown;
        private readonly ToolStripButton _btnTop;
        private readonly ToolStripButton _btnBottom;
        private readonly ToolStripButton _btnCopy;

        private const int EM_LINESCROLL = 0x00B6;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public ReadmeDialog(string title, string content)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            Width = 980;
            Height = 720;
            MinimizeBox = false;
            MaximizeBox = true;
            ShowIcon = false;

            _rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                WordWrap = false,
                DetectUrls = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Font = new System.Drawing.Font("Consolas", 10f),
                Text = content ?? string.Empty
            };

            _toolStrip = new ToolStrip
            {
                Dock = DockStyle.Right,
                GripStyle = ToolStripGripStyle.Hidden,
                LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow
            };

            _btnTop = new ToolStripButton("Top");
            _btnUp = new ToolStripButton("▲");
            _btnDown = new ToolStripButton("▼");
            _btnBottom = new ToolStripButton("Bottom");
            _btnCopy = new ToolStripButton("Copy");

            _btnTop.Click += (s, e) => ScrollToTop();
            _btnBottom.Click += (s, e) => ScrollToBottom();
            _btnUp.Click += (s, e) => ScrollLines(-8);
            _btnDown.Click += (s, e) => ScrollLines(+8);
            _btnCopy.Click += (s, e) =>
            {
                try { Clipboard.SetText(_rtb.Text); }
                catch { /* ignore clipboard errors */ }
            };

            _toolStrip.Items.Add(_btnTop);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnUp);
            _toolStrip.Items.Add(_btnDown);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnBottom);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnCopy);

            Controls.Add(_rtb);
            Controls.Add(_toolStrip);

            // Ensure we start at the top.
            Shown += (s, e) => ScrollToTop();
        }

        private void ScrollLines(int deltaLines)
        {
            // EM_LINESCROLL scrolls the RichTextBox by a number of lines.
            SendMessage(_rtb.Handle, EM_LINESCROLL, IntPtr.Zero, (IntPtr)deltaLines);
        }

        private void ScrollToTop()
        {
            _rtb.SelectionStart = 0;
            _rtb.SelectionLength = 0;
            _rtb.ScrollToCaret();
        }

        private void ScrollToBottom()
        {
            _rtb.SelectionStart = _rtb.TextLength;
            _rtb.SelectionLength = 0;
            _rtb.ScrollToCaret();
        }
    }
}
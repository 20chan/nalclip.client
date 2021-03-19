using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nalclip.cllient {
    public partial class Form1 : Form {
        [DllImport("User32.dll")]
        protected static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        IntPtr next;
        public Form1() {
            InitializeComponent();
            next = SetClipboardViewer(Handle);
        }

        private void OnCopy() {
            var value = new Clip { content = Clipboard.GetText() };
            Task.Run(async () => {
                var client = new HttpClient();
                var content = new StringContent(
                    JsonConvert.SerializeObject(value),
                    Encoding.UTF8,
                    "application/json"
                );
                System.Diagnostics.Debug.WriteLine($"before ${JsonConvert.SerializeObject(value)}");
                var resp = await client.PostAsync("https://clip.0ch.me/api/clip", content);
                System.Diagnostics.Debug.WriteLine($"after ${resp.StatusCode}");
                var a = await resp.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"after ${a}");

            });
        }

        protected override void WndProc(ref Message m) {
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            switch (m.Msg) {
                case WM_DRAWCLIPBOARD:
                    OnCopy();
                    SendMessage(next, m.Msg, m.WParam, m.LParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (m.WParam == next) {
                        next = m.LParam;
                    } else {
                        SendMessage(next, m.Msg, m.WParam, m.LParam);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }

    struct Clip {
        public string content;
    }
}

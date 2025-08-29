using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace LudvigUninstaller
{
    public partial class Form1 : Form
    {
        private ListView listView;
        private Button btnUninstall;
        private List<UninstallEntry> programs = new List<UninstallEntry>();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }

        public Form1()
        {
            InitializeComponent(); // Designer может быть пустым, но partial остаётся
            this.Text = "Ludvig Uninstaller";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создаём ListView
            listView = new ListView
            {
                Dock = DockStyle.Top,
                Height = 400,
                View = View.Details,
                FullRowSelect = true
            };
            listView.Columns.Add("Program Name", 250);
            listView.Columns.Add("Install Location", 350);
            this.Controls.Add(listView);

            // Создаём кнопку
            btnUninstall = new Button
            {
                Text = "Uninstall Selected",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnUninstall.Click += BtnUninstall_Click;
            this.Controls.Add(btnUninstall);

            LoadPrograms();
        }

        private void LoadPrograms()
        {
            programs.Clear();
            listView.Items.Clear();

            string[] registryKeys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (string keyPath in registryKeys)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key == null) continue;

                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                        {
                            string displayName = subKey?.GetValue("DisplayName") as string;
                            string uninstallString = subKey?.GetValue("UninstallString") as string;
                            string installLocation = subKey?.GetValue("InstallLocation") as string;

                            if (!string.IsNullOrEmpty(displayName))
                            {
                                var entry = new UninstallEntry
                                {
                                    Name = displayName,
                                    UninstallString = uninstallString,
                                    InstallLocation = installLocation
                                };
                                programs.Add(entry);

                                var item = new ListViewItem(entry.Name);
                                item.SubItems.Add(entry.InstallLocation ?? "");
                                listView.Items.Add(item);
                            }
                        }
                    }
                }
            }
        }

        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            if (listView.SelectedIndices.Count == 0) return;

            int index = listView.SelectedIndices[0];
            var entry = programs[index];

            if (!string.IsNullOrEmpty(entry.UninstallString))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c " + entry.UninstallString,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("This program has no uninstall command.");
            }
        }
    }

    public class UninstallEntry
    {
        public string Name { get; set; }
        public string UninstallString { get; set; }
        public string InstallLocation { get; set; }
    }
}

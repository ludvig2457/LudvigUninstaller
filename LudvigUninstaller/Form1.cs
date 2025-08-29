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
        private Button btnRefresh;
        private TextBox searchBox;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Panel topPanel;
        private Button btnDarkTheme;
        private Button btnLightTheme;

        private List<UninstallEntry> programs = new List<UninstallEntry>();
        private string currentTheme = "dark";

        private Color darkBg = ColorTranslator.FromHtml("#1e1e1e");
        private Color darkControl = ColorTranslator.FromHtml("#333333");
        private Color lightBg = Color.White;
        private Color lightControl = ColorTranslator.FromHtml("#f0f0f0");

        private ContextMenuStrip listContextMenu;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }

        public Form1()
        {
            InitializeComponent();

            this.Text = "Ludvig Uninstaller";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = darkBg;

            // Верхняя панель
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = darkControl
            };
            this.Controls.Add(topPanel);

            // Поиск
            searchBox = new TextBox
            {
                Width = 250,
                Height = 30,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                BackColor = darkControl,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "🔍 Поиск программы..."
            };
            searchBox.GotFocus += RemovePlaceholder;
            searchBox.LostFocus += SetPlaceholder;
            searchBox.TextChanged += SearchBox_TextChanged;
            searchBox.Location = new Point(10, 5);
            topPanel.Controls.Add(searchBox);

            // Кнопки смены темы
            btnDarkTheme = new Button
            {
                Text = "Dark",
                Location = new Point(270, 5),
                Size = new Size(60, 30),
                BackColor = darkControl,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDarkTheme.FlatAppearance.BorderSize = 0;
            btnDarkTheme.Click += (s, e) => { currentTheme = "dark"; ApplyTheme(); };
            topPanel.Controls.Add(btnDarkTheme);

            btnLightTheme = new Button
            {
                Text = "Light",
                Location = new Point(340, 5),
                Size = new Size(60, 30),
                BackColor = lightControl,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            btnLightTheme.FlatAppearance.BorderSize = 0;
            btnLightTheme.Click += (s, e) => { currentTheme = "light"; ApplyTheme(); };
            topPanel.Controls.Add(btnLightTheme);

            // Кнопка обновления списка
            btnRefresh = new Button
            {
                Text = "⟳ Refresh",
                Location = new Point(410, 5),
                Size = new Size(80, 30),
                BackColor = darkControl,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => RefreshPrograms();
            topPanel.Controls.Add(btnRefresh);

            // ListView
            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BackColor = darkControl,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                BorderStyle = BorderStyle.None
            };
            listView.Columns.Add("Program Name", 300);
            listView.Columns.Add("Install Location", 400);
            listView.DoubleClick += BtnUninstall_Click;
            listView.ColumnClick += ListView_ColumnClick;
            this.Controls.Add(listView);

            // Включаем двойную буферизацию
            typeof(ListView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, listView, new object[] { true });

            // Контекстное меню
            listContextMenu = new ContextMenuStrip();
            listContextMenu.Items.Add("Open Install Folder").Click += (s, e) => OpenInstallFolder();
            listContextMenu.Items.Add("Copy Install Path").Click += (s, e) => CopyInstallPath();
            listView.ContextMenuStrip = listContextMenu;

            // Кнопка удаления
            btnUninstall = new Button
            {
                Text = "🗑 Uninstall Selected",
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = ColorTranslator.FromHtml("#E74C3C"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.MouseEnter += (s, e) => btnUninstall.BackColor = ColorTranslator.FromHtml("#FF5555");
            btnUninstall.MouseLeave += (s, e) => btnUninstall.BackColor = ColorTranslator.FromHtml("#E74C3C");
            btnUninstall.Click += BtnUninstall_Click;
            this.Controls.Add(btnUninstall);

            // Статус-бар
            statusStrip = new StatusStrip
            {
                BackColor = darkControl,
                ForeColor = Color.White
            };
            statusLabel = new ToolStripStatusLabel("Ready");
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);

            // Загружаем программы
            LoadPrograms();

            // Применяем текущую тему
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            if (currentTheme == "dark")
            {
                this.BackColor = darkBg;
                topPanel.BackColor = darkControl;
                searchBox.BackColor = darkControl;
                searchBox.ForeColor = searchBox.Text == "🔍 Поиск программы..." ? Color.Gray : Color.White;
                listView.BackColor = darkControl;
                listView.ForeColor = Color.White;
                statusStrip.BackColor = darkControl;
                statusStrip.ForeColor = Color.White;
                btnUninstall.BackColor = ColorTranslator.FromHtml("#E74C3C");
                btnRefresh.BackColor = darkControl;
                btnRefresh.ForeColor = Color.White;
            }
            else
            {
                this.BackColor = lightBg;
                topPanel.BackColor = lightControl;
                searchBox.BackColor = lightControl;
                searchBox.ForeColor = searchBox.Text == "🔍 Поиск программы..." ? Color.Gray : Color.Black;
                listView.BackColor = lightControl;
                listView.ForeColor = Color.Black;
                statusStrip.BackColor = lightControl;
                statusStrip.ForeColor = Color.Black;
                btnUninstall.BackColor = Color.Red;
                btnRefresh.BackColor = lightControl;
                btnRefresh.ForeColor = Color.Black;
            }
        }

        private void SetPlaceholder(object sender = null, EventArgs e = null)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "🔍 Поиск программы...";
                searchBox.ForeColor = Color.Gray;
            }
        }

        private void RemovePlaceholder(object sender, EventArgs e)
        {
            if (searchBox.Text == "🔍 Поиск программы...")
            {
                searchBox.Text = "";
                searchBox.ForeColor = currentTheme == "dark" ? Color.White : Color.Black;
            }
        }

        private void LoadPrograms()
        {
            programs.Clear();

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
                                programs.Add(new UninstallEntry
                                {
                                    Name = displayName,
                                    UninstallString = uninstallString,
                                    InstallLocation = installLocation
                                });
                            }
                        }
                    }
                }
            }

            UpdateListView();
        }

        private void RefreshPrograms()
        {
            LoadPrograms();
            searchBox.Text = "";
        }

        private void UpdateListView(string filter = "")
        {
            listView.BeginUpdate();
            listView.Items.Clear();

            foreach (var entry in programs)
            {
                if (string.IsNullOrEmpty(filter) ||
                    entry.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (!string.IsNullOrEmpty(entry.InstallLocation) &&
                     entry.InstallLocation.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var item = new ListViewItem(entry.Name);
                    item.SubItems.Add(entry.InstallLocation ?? "");
                    listView.Items.Add(item);
                }
            }

            listView.EndUpdate();
            statusLabel.Text = $"Найдено программ: {listView.Items.Count}";
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (searchBox.ForeColor == Color.Gray) return;
            UpdateListView(searchBox.Text);
        }

        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0) return;

            var selectedItem = listView.SelectedItems[0];
            var entry = programs.Find(p => p.Name == selectedItem.Text);

            if (entry == null || string.IsNullOrEmpty(entry.UninstallString))
            {
                MessageBox.Show("This program has no uninstall command.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Вы уверены, что хотите удалить \"{entry.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = entry.UninstallString,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Сортировка колонок
        private int sortColumn = -1;
        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != sortColumn)
            {
                sortColumn = e.Column;
                listView.Sorting = SortOrder.Ascending;
            }
            else
            {
                listView.Sorting = listView.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            listView.ListViewItemSorter = new ListViewItemComparer(e.Column, listView.Sorting);
            listView.Sort();
        }

        // Контекстное меню
        private void OpenInstallFolder()
        {
            if (listView.SelectedItems.Count == 0) return;
            var entry = programs.Find(p => p.Name == listView.SelectedItems[0].Text);
            if (!string.IsNullOrEmpty(entry?.InstallLocation))
                Process.Start("explorer.exe", entry.InstallLocation);
        }

        private void CopyInstallPath()
        {
            if (listView.SelectedItems.Count == 0) return;
            var entry = programs.Find(p => p.Name == listView.SelectedItems[0].Text);
            if (!string.IsNullOrEmpty(entry?.InstallLocation))
                Clipboard.SetText(entry.InstallLocation);
        }
    }

    public class UninstallEntry
    {
        public string Name { get; set; }
        public string UninstallString { get; set; }
        public string InstallLocation { get; set; }
    }

    // Класс для сортировки ListView
    public class ListViewItemComparer : System.Collections.IComparer
    {
        private int col;
        private SortOrder order;
        public ListViewItemComparer(int column, SortOrder order)
        {
            col = column;
            this.order = order;
        }
        public int Compare(object x, object y)
        {
            int returnVal = String.Compare(
                ((ListViewItem)x).SubItems[col].Text,
                ((ListViewItem)y).SubItems[col].Text);
            if (order == SortOrder.Descending) returnVal *= -1;
            return returnVal;
        }
    }
}

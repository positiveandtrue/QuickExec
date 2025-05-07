using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using IWshRuntimeLibrary;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;

namespace QuickExecutables
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += DragProgram;
            this.DragDrop += CreateButton;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public class ButtonData
        {
            public string TargetPath { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string IconPath { get; set; } // Optional: If you want to save the icon
        }

        private void SaveButtonData(List<ButtonData> buttonDataList)
        {
            string json = JsonConvert.SerializeObject(buttonDataList, Formatting.Indented);
            System.IO.File.WriteAllText("buttons.json", json);
        }

        // Load button data from the JSON file
        private List<ButtonData> LoadButtonData()
        {
            if (System.IO.File.Exists("buttons.json"))
            {
                string json = System.IO.File.ReadAllText("buttons.json");
                return JsonConvert.DeserializeObject<List<ButtonData>>(json);
            }
            return new List<ButtonData>();
        }


        private void startProgram_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sc2 = new ProcessStartInfo();
            sc2.FileName = @"G:\EasyBuilderProv61001217\EasyBuilder Pro.exe";

            try
            {
                Process.Start(sc2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start the process: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<ButtonData> loadedButtonData = LoadButtonData();

            foreach (var buttonData in loadedButtonData)
            {
                Button dynamicButton = new Button();
                dynamicButton.Size = new Size(64, 64);
                dynamicButton.Tag = buttonData.TargetPath;
                dynamicButton.Location = new Point(buttonData.X, buttonData.Y);

                // Load icon (you can optionally save the icon file path if you want to optimize this)
                Icon icon = Icon.ExtractAssociatedIcon(buttonData.TargetPath);
                dynamicButton.Image = icon?.ToBitmap();
                dynamicButton.ImageAlign = ContentAlignment.MiddleCenter;

                dynamicButton.Click += (s, args) =>
                {
                    string path = (string)((Button)s).Tag;
                    if (System.IO.File.Exists(path))
                        Process.Start(path);
                };

                // Tooltip
                ToolTip tt = new ToolTip();
                tt.SetToolTip(dynamicButton, buttonData.TargetPath);

                // Add the button to the form
                this.Controls.Add(dynamicButton);
            }
        }


        private void DragProgram(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private List<ButtonData> buttonDataList = new List<ButtonData>();

        private void CreateButton(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;

            string filePath = files[0];
            string ext = Path.GetExtension(filePath).ToLower();
            string targetPath = "";

            if (ext == ".lnk")
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(filePath);
                targetPath = shortcut.TargetPath;
            }
            else if (ext == ".exe")
            {
                targetPath = filePath;
            }
            else
            {
                MessageBox.Show("Only .lnk and .exe files are supported.");
                return;
            }

            if (System.IO.File.Exists(targetPath))
            {
                // Check if button already exists for this target path
                if (this.Controls.OfType<Button>().Any(b => (string)b.Tag == targetPath))
                {
                    return; // Skip creating a new button if one already exists
                }

                // Extract icon
                Icon icon = Icon.ExtractAssociatedIcon(targetPath);

                // Create button dynamically
                Button dynamicButton = new Button();
                dynamicButton.Size = new Size(64, 64);
                dynamicButton.Image = icon?.ToBitmap();
                dynamicButton.ImageAlign = ContentAlignment.MiddleCenter;
                dynamicButton.Tag = targetPath;
                dynamicButton.Location = new Point(10 + this.Controls.Count * 70, 10);

                dynamicButton.Click += (s, args) =>
                {
                    string path = (string)((Button)s).Tag;
                    if (System.IO.File.Exists(path))
                        Process.Start(path);
                };

                // Save button data
                buttonDataList.Add(new ButtonData
                {
                    TargetPath = targetPath,
                    X = dynamicButton.Location.X,
                    Y = dynamicButton.Location.Y
                });

                // Tooltip
                ToolTip tt = new ToolTip();
                tt.SetToolTip(dynamicButton, targetPath);

                // Add the button to the form
                this.Controls.Add(dynamicButton);
            }
            else
            {
                MessageBox.Show("The target path does not exist.");
            }

            // Save the button data to a file after creating the button
            SaveButtonData(buttonDataList);
        }



    }
}
    


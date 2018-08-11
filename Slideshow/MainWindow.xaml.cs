using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Slideshow
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly List<string> imageExtensions = new List<string> { ".JPG", ".jpg", ".JPE", ".BMP", ".GIF", ".PNG" };

        private string rootPath;

        private List<String> paths;
        private List<String> relativePaths;

        private JsonManager jm;
        private int currentIndex;
        private ImageManager im;

        public MainWindow()
        {
            InitializeComponent();
            im = new ImageManager();

            // Farbe der TitleTextBox setzen
            TitleTextBox.Foreground = Brushes.White;
        }

        private void ChooseDirectory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            // Vorauswahl
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog.SelectedPath = "D:\\Bilder\\03_Suedamerika";

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Pfade auslesen
                rootPath = folderBrowserDialog.SelectedPath;

                // jsonFile einlesen
                jm = new JsonManager(rootPath);
                jm.ReadFile();

                Console.WriteLine(rootPath);
                paths = Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories).Where(path => imageExtensions.Contains(System.IO.Path.GetExtension(path))).ToList();
                currentIndex = 0;
                
                relativePaths = paths.Select(path => path.Replace(rootPath, "").Trim('\\').Replace('\\', '/')).ToList();
                
                // erstes Bild anzeigen
                ShowImage();
                this.Background = Brushes.Black;

                ChooseDirectoryButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowNextImage()
        {
            int indexBefore = currentIndex;
            currentIndex = currentIndex >= paths.Count - 1 ? 0 : ++currentIndex;
            if(!ShowImage())
            {
                ShowNextImage();
            }
        }

        private void ShowPreviousImage()
        {
            int indexBefore = currentIndex;
            currentIndex = currentIndex <= 0 ? paths.Count - 1 : --currentIndex;
            if(!ShowImage())
            {
                ShowPreviousImage();
            }
        }

        private bool ShowImage()
        {
            if(jm.GetDisabled(relativePaths[currentIndex]))
            {
                return false;
            }
            image.Source = im.Get(paths[currentIndex]);

            TitleTextBox.Text = jm.GetTitle(relativePaths[currentIndex]);

            string d = relativePaths[currentIndex].Split('/').First();
            DatumLabel.Content = d.Split('-')[2] + "." + d.Split('-')[1] + "." + d.Split('-')[0];

            // Nächstes Bild im Hintergrund laden
            if (currentIndex < paths.Count - 1)
			{
				im.Preload(paths[currentIndex + 1]);
			}

            return true;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Right:
                    // Nächstes Bild anzeigen
                    ShowNextImage();
                    break;
                case Key.Left:
                    // Letztes Bild anzeigen
                    ShowPreviousImage();
                    break;
                case Key.S:
                    if(jm != null && e.OriginalSource != TitleTextBox)
                    {
                        // Speichern und Nachricht ausgeben
                        //(wenn s nicht in TextBox geschrieben wurde)
                        jm.SaveToFile(true);
                    }
                    break;
                case Key.T:
                    if (jm != null && e.OriginalSource != TitleTextBox && jm != null)
                    {
                        // Titel setzen
                        e.Handled = true;
                        EnableTitleTextBox();
                        TitleTextBox.Focus();
                        TitleTextBox.SelectAll();
                    }
                    break;
                case Key.D:
                    jm.SetDisabled(relativePaths[currentIndex], !jm.GetDisabled(relativePaths[currentIndex]));
                    break;
                case Key.F5:
                    // Vollbild-Modus
                    SetFullscreenMode(true);
                    break;
                case Key.Escape:
                    // Vollbild-Modus beenden
                    SetFullscreenMode(false);
                    break;
                case Key.Oem1: // Ü
                    if (jm != null && e.OriginalSource != TitleTextBox && jm != null)
                    {
                        // Titel übernehmen
                        e.Handled = true;
                        int indexLastImageWithTitle = currentIndex - 1;
                        while(String.IsNullOrWhiteSpace(jm.GetTitle(relativePaths[indexLastImageWithTitle])))
                        {
                            indexLastImageWithTitle--;
                            Console.WriteLine("___ " + currentIndex + ":" + indexLastImageWithTitle);
                        }
                        Console.WriteLine("indexLast: " + indexLastImageWithTitle + ", " + jm.GetTitle(relativePaths[indexLastImageWithTitle]));
                        for (int i = currentIndex; i >= indexLastImageWithTitle; i--)
                        {
                            jm.SaveTitle(relativePaths[i], jm.GetTitle(relativePaths[indexLastImageWithTitle]));
                            Console.WriteLine("do: " + currentIndex + ":" + indexLastImageWithTitle);
                        }

                        TitleTextBox.Text = jm.GetTitle(relativePaths[currentIndex]);
                    }
                    break;
            }
        }

        private void TitleTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    string text = TitleTextBox.Text;
                    DisableTitleTextBox();
                    jm.SaveTitle(relativePaths[currentIndex], text);
                    break;
                case Key.Escape:
                    TitleTextBox.Text = jm.GetTitle(relativePaths[currentIndex]);
                    DisableTitleTextBox();
                    break;
            }
        }

        private void EnableTitleTextBox()
        {
            TitleTextBox.IsReadOnly = false;
            TitleTextBox.IsEnabled = true;
        }

        private void DisableTitleTextBox()
        {
            TitleTextBox.IsReadOnly = true;
            TitleTextBox.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Fenster wird geschossen
            if(jm != null && jm.UnsavedChanges)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Willst du deine Einstellungen speichern?", "Einstellungen speichern", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    jm.SaveToFile(false);
                }
            }
        }


        private void SetFullscreenMode(bool fullscreenMode)
        {
            if (fullscreenMode)
            {
                Visibility = Visibility.Collapsed;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                Topmost = true;
                Visibility = Visibility.Visible;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                Topmost = false;
            }
        }
    }
}

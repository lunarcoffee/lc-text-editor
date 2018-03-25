using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.Win32;

namespace WPFTest {
     /// <summary>
     /// Manages the MainWindow.xaml's interactivity,
     /// as well as the storage of persistent data.
     /// </summary>
    public partial class MainWindow {
        // current open file in editor and the program's data
        private static string _currentFile;
        private static string _currentFileShort = " No file open.";
        private static Dictionary<string, bool> _appData;
        public MainWindow() {
            InitializeComponent();
            // check if the data file exists; if it doesn't, make a new one
            if (!File.Exists("lcte-data.txt"))
                File.Create(".\\lcte-data.txt");
            // get the list of values from the data file
            var appDataStrings = !File.ReadAllText(".\\lcte-data.txt").Contains(",")
                                     ? "False,False,True".Split(',').Select(bool.Parse).ToArray()
                                     : File.ReadAllText(".\\lcte-data.txt").Split(',').Select(bool.Parse).ToArray();
            // initialize the program's data dictionary
            _appData = new Dictionary<string, bool> {
                {"mono_font", appDataStrings[0]},
                {"dark_theme", appDataStrings[1]},
                {"wrap_text", appDataStrings[2]}
            };
            // load settings from the program's data
            LoadSettingsOnOpen();
        }
        // required for new custom kb shortcuts
        private void AlwaysCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }
        // disable save when no file is open
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _currentFile != null;
        }
        // shows about dialog
        private void AboutDialog_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show(
                "Simple but functional text editor made using C# and the WPF framework. " 
                    + "This is LunarCoffee's first GUI Windows program.", 
                "About",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        // override the closing to display dialog to confirm program exit
        protected override void OnClosing(CancelEventArgs e) {
            CloseWithConfirmDialog();
        }
        // shows dialog confirming program exit
        private void ExitProgram_OnClick(object sender, RoutedEventArgs e) {
            CloseWithConfirmDialog();
        }
        // manages current row/char status bar item
        private void TextArea_OnSelectionChanged(object sender, RoutedEventArgs e) {
            var row = TextArea.GetLineIndexFromCharacterIndex(TextArea.CaretIndex);
            var col = TextArea.CaretIndex - TextArea.GetCharacterIndexFromLineIndex(row);
            CurrentCaretLocation.Text = $"{row + 1}:{col + 1} ";
        }
        // makes a new file
        private void NewFile_OnClick(object sender, RoutedEventArgs e) {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true) {
                // create the file and clear the text area
                File.Create(saveFileDialog.FileName);
                TextArea.Text = "";
                // show shortened file path if the full name is too long
                CurrentFile.Text = " " + (saveFileDialog.FileName.Length < 84
                                       ? saveFileDialog.FileName
                                       : saveFileDialog.FileName.Substring(0, 64) 
                                           + "..."
                                           + saveFileDialog.FileName.Split('\\').Last());
                _currentFileShort = CurrentFile.Text;
                // set current file variable
                _currentFile = saveFileDialog.FileName;
            }
        }
        // opens a file
        private void OpenFile_OnClick(object sender, RoutedEventArgs e) {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                // "open" the file
                TextArea.Text = File.ReadAllText(openFileDialog.FileName);
                // show shortened file path if the full name is too long
                CurrentFile.Text = " " + (openFileDialog.FileName.Length < 84
                                       ? openFileDialog.FileName
                                       : openFileDialog.FileName.Substring(0, 64) 
                                           + "..."
                                           + openFileDialog.FileName.Split('\\').Last());
                _currentFileShort = CurrentFile.Text;
                // set current file variable
                _currentFile = openFileDialog.FileName;
            }
        }
        // saves the currently open file
        private void SaveFile_OnClick(object sender, RoutedEventArgs e) {
            // checking for no open file handled by savecommand_canexecute method
            try {
                File.WriteAllText(_currentFile, TextArea.Text);
                // inform the user that the file was saved
                ShowSnackbar("File successfully saved.");
            } catch {
                // inform the user of the error
                ShowSnackbar("An error occured. The file was not saved.");
            }
        }
        // same as above method, but allows the user to select a file name to save with
        private void SaveAsFile_OnClick(object sender, RoutedEventArgs e) {
            try {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true) {
                    File.WriteAllText(saveFileDialog.FileName, TextArea.Text);
                    // inform the user that the file was saved
                    ShowSnackbar("File successfully saved.");
                } else {
                    // inform the user that the file was not saved
                    ShowSnackbar("Did not save file.");
                }
            } catch {
                // inform the user of the error
                ShowSnackbar("An error occured. The file was not saved.");
            }
        }
        // "closes" a file
        private void CloseFile_OnClick(object sender, RoutedEventArgs e) {
            TextArea.Text = "";
            CurrentFile.Text = " No file open.";
            _currentFile = null;
            _currentFileShort = CurrentFile.Text;
        }
        // changes font to monospaced font
        private void FontMonospaced_OnClick(object sender = null, RoutedEventArgs e = null) {
            TextArea.FontFamily = new FontFamily("Consolas");
            // update the program's persistent data
            _appData["mono_font"] = true;
            UpdateAppDataFile();
        }
        // changes font to default font
        private void FontDefault_OnClick(object sender, RoutedEventArgs e) {
            TextArea.FontFamily = SystemFonts.MessageFontFamily;
            // update the program's persistent data
            _appData["mono_font"] = false;
            UpdateAppDataFile();
        }
        // switches to dark theme
        private void DarkTheme_OnCheck(object sender = null, RoutedEventArgs e = null) {
            // add every menu item of every menu item because wpf is inefficient sometimes
            var submenus = new [] {
                AppSubmenu1, AppSubmenu2, AppSubmenu3, AppSubmenu4, AppSubmenu5, AppSubmenu6, AppSubmenu7, 
                AppSubmenu8, AppSubmenu9, AppSubmenu10, AppSubmenu11, AppSubmenu12, AppSubmenu13, AppSubmenu14,
                AppSubmenu15, AppSubmenu16
            };
            // change basically everything to it's opposite color
            AppWindow.Background = Brushes.Black;
            AppWindow.Foreground = Brushes.White;
            AppMenu.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#262626");
            AppMenu.Foreground = Brushes.White;
            TextArea.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#181818");
            TextArea.Foreground = Brushes.White;
            AppStatusBar.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#262626");
            AppStatusBar.Foreground = Brushes.White;
            // including all the menu items of the menu items
            foreach (var menu in submenus)
                menu.Foreground = Brushes.Black;
            // update the program's persistent data
            _appData["dark_theme"] = true;
            UpdateAppDataFile();
        }
        // switches to light theme
        private void DarkTheme_OnUncheck(object sender, RoutedEventArgs e) {
            // change everything back to its original color
            AppWindow.Background = SystemColors.ControlBrush;
            AppWindow.Foreground = Brushes.Black;
            AppMenu.Background = Brushes.White;
            AppMenu.Foreground = Brushes.Black;
            TextArea.Background = Brushes.White;
            TextArea.Foreground = Brushes.Black;
            AppStatusBar.Background = Brushes.White;
            AppStatusBar.Foreground = Brushes.Black;
            // update the program's persistent data
            _appData["dark_theme"] = false;
            UpdateAppDataFile();
        }
        // turns on the text wrapping of the text area
        private void WrapText_OnCheck(object sender, RoutedEventArgs e) {
            TextArea.TextWrapping = TextWrapping.Wrap;
            // update the program's persistent data
            _appData["wrap_text"] = true;
            UpdateAppDataFile();
        }
        // turns off the text wrapping of the text area
        private void WrapText_OnUncheck(object sender = null, RoutedEventArgs e = null) {
            TextArea.TextWrapping = TextWrapping.NoWrap;
            // update the program's persistent data
            _appData["wrap_text"] = false;
            UpdateAppDataFile();
        }
        // shows an error message in the status bar
        private async void ShowSnackbar(string message) {
            // change the text for 3 seconds, then revert to the original text stored in temp
            CurrentFile.Text = " " + message;
            await Task.Delay(2000);
            CurrentFile.Text = _currentFileShort;
        }
        // update the program's data file
        private void UpdateAppDataFile() {
            var toWrite = string.Join(",", _appData.Values.ToList().Select(i => i.ToString()));
            File.WriteAllText(".\\lcte-data.txt", toWrite);
        }
        // load the program's data (to be called in the constructor)
        private void LoadSettingsOnOpen() {
            // load dark theme setting (default is false)
            if (_appData["dark_theme"]) {
                DarkTheme_OnCheck();
                AppSubmenu15.IsChecked = true;
            }
            // load monospaced font setting (default is regular (true))
            if (_appData["mono_font"])
                FontMonospaced_OnClick();
            // load wrap text setting (default is true)
            if (!_appData["wrap_text"]) {
                WrapText_OnUncheck();
                AppSubmenu16.IsChecked = false;
            } else {
                AppSubmenu16.IsChecked = true;
            }
        }
        // closes program with a confirmation dialog
        private void CloseWithConfirmDialog() {
            var exitConfirm = MessageBox.Show(
                "Are you sure you want to exit?",
                "Exit",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (exitConfirm == MessageBoxResult.OK) {
                Application.Current.Shutdown();
            }
        }
    }
}

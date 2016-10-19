using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using QMora.FamilyPhotos.Properties;

namespace QMora.FamilyPhotos
{
    /// <summary>
    /// Interaction logic for ConfigureDialog.xaml
    /// </summary>
    public partial class ConfigureDialog : Window
    {
        public ConfigureDialog()
        {
            InitializeComponent();
        }

        public void InitFields()
        {
            TxtDirInput.Text = Settings.Default.Directory_Input;
            TxtDirBackup.Text = Settings.Default.Directory_Backup;
            TxtDirOutput.Text = Settings.Default.Directory_Output;
            TxtFileData.Text = Settings.Default.File_ExcelData;

            TxtDrawingHeightPercentage.Text = Settings.Default.Draw_Box_Height_Percentage.ToString();
            TxtDrawingWidthPercentage.Text = Settings.Default.Draw_Box_Width_Percentage.ToString();

            TxtDrawingMarginFromLeftPercentage.Text = Settings.Default.Margin_FromLeft_Percentage.ToString();
            TxtDrawingMarginFromBottomPercentage.Text = Settings.Default.Margin_FromBotton_Percentage.ToString();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var isValid = ValidateInputs();
            if (!isValid) return;

            Settings.Default.Directory_Input = TxtDirInput.Text.Trim();
            Settings.Default.Directory_Backup = TxtDirBackup.Text.Trim();
            Settings.Default.Directory_Output = TxtDirOutput.Text.Trim();

            Settings.Default.File_ExcelData = TxtFileData.Text.Trim();

            Settings.Default.Draw_Box_Height_Percentage = int.Parse(TxtDrawingHeightPercentage.Text.Trim());
            Settings.Default.Draw_Box_Width_Percentage = int.Parse(TxtDrawingWidthPercentage.Text.Trim());

            Settings.Default.Margin_FromLeft_Percentage = int.Parse(TxtDrawingMarginFromLeftPercentage.Text.Trim());
            Settings.Default.Margin_FromBotton_Percentage = int.Parse(TxtDrawingMarginFromBottomPercentage.Text.Trim());
            
            Settings.Default.Save();

            MessageBox.Show("Data saved successfully");

            DialogResult = true;
            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            MessageBox.Show("No data saved");
        }

        private bool ValidateInputs()
        {
            var isValidInput = ValidateFolder(TxtDirInput.Text.Trim(), "Input");
            var isValidBackup = ValidateFolder(TxtDirBackup.Text.Trim(), "Backup");
            var isValidOutput = ValidateFolder(TxtDirOutput.Text.Trim(), "Output");

            var isValidData = ValidateFile(TxtFileData.Text.Trim(), "Excel data");

            var isValidHeight = ValidateInteger(TxtDrawingHeightPercentage.Text.Trim(), "Drawing Box Height %");
            var isValidWidth = ValidateInteger(TxtDrawingWidthPercentage.Text.Trim(), "Drawing Box Width %");

            var isValidMarginFromLeft = ValidateInteger(TxtDrawingMarginFromLeftPercentage.Text.Trim(), "Drawing Box Margin From Left %");
            var isValidMarginFromBottom = ValidateInteger(TxtDrawingMarginFromBottomPercentage.Text.Trim(), "Drawing Box Margin From Bottom %");

            return isValidInput & isValidBackup & isValidOutput &
                   isValidData &
                   isValidHeight & isValidWidth &
                   isValidMarginFromLeft & isValidMarginFromBottom;
        }

        private bool ValidateInteger(string intString, string name)
        {
            int value;
            var isValid = int.TryParse(intString, out value);

            if (!isValid)
            {
                MessageBox.Show(string.Format("{0} is invalid", name));
                return false;
            }

            return true;
        }

        private bool ValidateFile(string file, string name)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                MessageBox.Show(string.Format("{0} file empty", name));
                return false;
            }

            var isFileExists = File.Exists(file);
            if (!isFileExists)
            {
                MessageBox.Show(string.Format("{0} file - not exits", name));
                return false;
            }

            return true;
        }

        private bool ValidateFolder(string folder, string name)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                MessageBox.Show(string.Format("{0} folder empty", name));
                return false;
            }

            var isDirectoryExists = Directory.Exists(folder);
            if (!isDirectoryExists)
            {
                MessageBox.Show(string.Format("{0} folder - not exits", name));
                return false;
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using QMora.FamilyPhotos.Common;
using QMora.FamilyPhotos.Data;
using QMora.FamilyPhotos.Properties;
using Path = System.Windows.Shapes.Path;

namespace QMora.FamilyPhotos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Queue<string> _inputFilesQueue = new Queue<string>();

        ParticipantDetailProvider participantDetailProvider = new ParticipantDetailProvider();

        private InputFilesListener inputFileListener;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayParticipants(new ParticipantRecord());
        }

        private void BtnConfigure_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ConfigureDialog();
            dialog.InitFields();
            var result = dialog.ShowDialog();
            if (result.GetValueOrDefault())
            {
                DisplayParticipants(new ParticipantRecord());
            }
        }

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            CreateFoldersIfNotExists();
            BtnStart.Visibility = Visibility.Collapsed;
            BtnStop.Visibility = Visibility.Visible;

            UpdatePendingFilesList();

            var binDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            var inputDirectoryFullPath = System.IO.Path.Combine(binDir, Properties.Settings.Default.Directory_Input);
            inputFileListener = new InputFilesListener(inputDirectoryFullPath);
            inputFileListener.FileRecivedEvent += inputFileListener_FileRecivedEvent;
            inputFileListener.Start();
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            BtnStart.Visibility = Visibility.Visible;
            BtnStop.Visibility = Visibility.Collapsed;

            if (inputFileListener != null)
            {
                inputFileListener.Stop();
            }
        }

        private void inputFileListener_FileRecivedEvent(object sender, InputFileEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(UpdatePendingFilesList);
        }

        private void BtnAssignParticipant_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListParticipants.SelectedItem == null)
            {
                return;
            }

            if (ImgPhoto.Visibility == Visibility.Visible)
            {
                AssignSelectedParticipantToCurrentImage(false, false);
            }
        }

        private void BtnSkipParticipant_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListParticipants.SelectedItem == null)
            {
                return;
            }

            if (ImgPhoto.Visibility == Visibility.Visible)
            {
                AssignSelectedParticipantToCurrentImage(true, false);
            }
        }
        private void BtnSkiipParticipantNoRename_OnClick(object sender, RoutedEventArgs e)
        {
            if (ListParticipants.SelectedItem == null)
            {
                return;
            }

            if (ImgPhoto.Visibility == Visibility.Visible)
            {
                AssignSelectedParticipantToCurrentImage(true, true);
            }
        }

        #region Search Section

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            var searchCriteria = new ParticipantRecord();
            searchCriteria.No = TxtSearchNo.Text.Trim();
            searchCriteria.Name = TxtSearchName.Text.Trim();
            searchCriteria.GraduatedYear = TxtSearchYear.Text.Trim();
            searchCriteria.Faculty = TxtSearchFaculty.Text.Trim();

            DisplayParticipants(searchCriteria);
        }

        private void BtnClearSearch_OnClick(object sender, RoutedEventArgs e)
        {
            TxtSearchNo.Text = string.Empty;
            TxtSearchName.Text = string.Empty;
            TxtSearchYear.Text = string.Empty;
            TxtSearchFaculty.Text = string.Empty;

            DisplayParticipants(new ParticipantRecord());
        }

        private void TxtSerachInput_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }
            BtnSearch_OnClick(sender, e);
        }

        #endregion

        private void CreateFoldersIfNotExists()
        {
            CreateFolderIfNotExists(Properties.Settings.Default.Directory_Input);
            CreateFolderIfNotExists(Properties.Settings.Default.Directory_Backup);
            CreateFolderIfNotExists(System.IO.Path.Combine(Properties.Settings.Default.Directory_Backup, "Temp"));
            CreateFolderIfNotExists(Properties.Settings.Default.Directory_Output);
        }

        private void CreateFolderIfNotExists(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                MessageBox.Show(String.Format("Created folder {0}", folder));
            }
        }

        private void DisplayParticipants(ParticipantRecord searchRecord)
        {
            try
            {
                ListParticipants.ItemsSource = participantDetailProvider.GetAll(searchRecord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdatePendingFilesList()
        {
            _inputFilesQueue.Clear();

            var files = Directory.GetFiles(Settings.Default.Directory_Input);
            
            foreach (var file in files)
            {
                _inputFilesQueue.Enqueue(file);
            }

            //if no file displayed, display the first file
            DisplayFirstPendingFiles();
        }

        private void DisplayFirstPendingFiles()
        {
            if (_inputFilesQueue.Count > 0)
            {
                if (ImgPhoto.Visibility == Visibility.Collapsed)
                {
                    var file = _inputFilesQueue.Peek();

                    try
                    {
                        BitmapImage logo = new BitmapImage();
                        logo.BeginInit();
                        logo.CacheOption = BitmapCacheOption.OnLoad;
                        logo.UriSource = new Uri(file);
                        logo.EndInit();

                        ImgPhoto.Source = logo;
                        ImgPhoto.Visibility = Visibility.Visible;

                        LabelCurrentFileName.Content = file;
                        LabelCurrentFileName.Visibility = Visibility.Visible;
                    }
                    catch (Exception)
                    {
                        //no message
                    }
                }
            }

            LabelPendingFiles.Content = string.Format("{0} files in queue", _inputFilesQueue.Count);
            LabelPendingFiles.Visibility = Visibility.Visible;
        }

        private void AssignSelectedParticipantToCurrentImage(bool skipTextAssign, bool skipFileRename)
        {
            var inputFile = _inputFilesQueue.Dequeue();

            var filename = new FileInfo(inputFile).Name;

            var selectedParticipant = (ParticipantRecord) ListParticipants.SelectedItem;

            var bitmap = (BitmapImage) ImgPhoto.Source;
            bitmap.UriSource = null;

            ImgPhoto.Source = null;
            ImgPhoto.UpdateLayout();
            ImgPhoto.Visibility = Visibility.Collapsed;

            LabelCurrentFileName.Visibility = Visibility.Collapsed;

            var imageProcessor = new ImageProcessor();
            imageProcessor.ProcessImage(filename, inputFile, selectedParticipant, skipTextAssign, skipFileRename);

            var stringBuilder = new StringBuilder();

            if (!skipTextAssign)
            {
                stringBuilder.AppendFormat("Image {0} text is assigned to {1}", filename, selectedParticipant.Name);
            }
            else
            {
                stringBuilder.AppendFormat("No image {0} text is assigned", filename);
            }

            if (!skipFileRename)
            {
                stringBuilder.Append("\nFile renamed");

            }
            else
            {
                stringBuilder.Append("\nFile not renamed");
            }

            MessageBox.Show(stringBuilder.ToString());
            
            DisplayFirstPendingFiles();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QMora.FamilyPhotos.Data;


namespace QMora.FamilyPhotos.Common
{
    public class ImageProcessor
    {
        public void ProcessImage(string filename, string fullPathName, ParticipantRecord participant, bool skipTextAssign, bool skipFileRename)
        {
            //Copy the file to both backup and temp
            var fileInfo = new FileInfo(fullPathName);
            

            var backupFileFullPath = Path.Combine(Properties.Settings.Default.Directory_Backup, filename);
            var workingFileFullPath = Path.Combine(Path.Combine(Properties.Settings.Default.Directory_Backup, "Temp"), filename);

            var nextAvailableOutputFileName = filename;
            if (!skipFileRename)
            {
                nextAvailableOutputFileName = FindNextAvailableFileName(Properties.Settings.Default.Directory_Output,
                string.Format("{0}-{1}", participant.Name, participant.GraduatedYear), fileInfo.Extension);
            }
            var outputFileFullPath = Path.Combine(Properties.Settings.Default.Directory_Output, nextAvailableOutputFileName);

            File.Copy(fullPathName, backupFileFullPath, true);
            File.Copy(fullPathName, workingFileFullPath, true);

            File.Delete(fullPathName);

            if (skipTextAssign)
            {
                File.Copy(backupFileFullPath, outputFileFullPath);

                return;
            }

            Bitmap bitmap = (Bitmap) Image.FromFile(workingFileFullPath);

            var boxHeight = bitmap.Height * Properties.Settings.Default.Draw_Box_Height_Percentage / 100;
            var boxWidth = bitmap.Width * Properties.Settings.Default.Draw_Box_Width_Percentage / 100;

            var boxTopPointX = bitmap.Width* Properties.Settings.Default.Margin_FromLeft_Percentage/100;
            var boxTopPointY = bitmap.Height - boxHeight - bitmap.Height * Properties.Settings.Default.Margin_FromBotton_Percentage / 100;

           //var text = string.Format("{0}\n{1} - {2}", participant.Name, participant.Faculty, participant.GraduatedYear);
            var text = string.Format("{0}\n{1} ", participant.Name, participant.GraduatedYear);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (Font font = FindFont(graphics, text, new Size(boxWidth, boxHeight), Properties.Settings.Default.Draw_Font ))
                {
                    graphics.FillRectangle(new SolidBrush(Properties.Settings.Default.Draw_BackGround), boxTopPointX, boxTopPointY, boxWidth, boxHeight);
                    graphics.DrawString(text, font, new SolidBrush(Properties.Settings.Default.Draw_FontColor), new PointF(boxTopPointX, boxTopPointY));
                    
                }
            }

            bitmap.Save(outputFileFullPath);
        }

        private Font FindFont(System.Drawing.Graphics g, string longString, Size Room, Font PreferedFont)
        {
            //you should perform some scale functions!!!
            SizeF RealSize = g.MeasureString(longString, PreferedFont);
            float HeightScaleRatio = Room.Height / RealSize.Height;
            float WidthScaleRatio = Room.Width / RealSize.Width;
            float ScaleRatio = (HeightScaleRatio < WidthScaleRatio) ? ScaleRatio = HeightScaleRatio : ScaleRatio = WidthScaleRatio;
            float ScaleFontSize = PreferedFont.Size * ScaleRatio;
            return new Font(PreferedFont.FontFamily, ScaleFontSize);
        }

        private string FindNextAvailableFileName(string folderName, string fileNamePart, string fileExtension)
        {
            var existingFiles = Directory.GetFiles(folderName, string.Format("{0}*.{1}", fileNamePart, fileExtension));

            if (!existingFiles.Any())
            {
                return string.Format("{0}.{1}", fileNamePart, fileExtension);
            }

            var count = existingFiles.Count();

            return string.Format("{0} ({1}).{2}", fileNamePart, count, fileExtension);
        }
    }
}

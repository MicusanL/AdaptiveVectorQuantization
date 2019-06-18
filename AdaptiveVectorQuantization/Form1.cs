using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdaptiveVectorQuantization
{
    public partial class FormAVQ : Form
    {

        private FastImage originalImage;

        public FormAVQ()
        {
            InitializeComponent();
        }

        public static string InputFile { get; set; }
        internal AVQ AvqCompression { get; set; } = null;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            InputFile = openFileDialog.FileName;

            try
            {
                panelOriginalImage.BackgroundImage = new Bitmap(InputFile);
            }
            catch (Exception)
            {
                Console.WriteLine("File {0} not found", InputFile);
            }
            
            
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            buttonLoad.Enabled = false;

            if (InputFile != null)
            {
                AvqCompression = new AVQ(InputFile);
                bool drawBorder = checkBoxDrawBorder.Checked;
                bool CompressedFileFormat = checkBoxCompressedFileFormat.Checked;
                int.TryParse(textBoxThreshold.Text, out int threshold);
                int.TryParse(textBoxDictionarySize.Text, out int dictionarySize);
               
             

                originalImage = AvqCompression.StartCompression(threshold, dictionarySize, drawBorder, CompressedFileFormat);
                panelDestination.BackgroundImage = null;
                panelDestination.BackgroundImage = originalImage.GetBitMap();
            }
            else
            {
                MessageBox.Show("You need to choose an image!");
            }


            buttonStart.Enabled = true;
            buttonLoad.Enabled = true;
        }

        private void buttonShannon_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"D:\Facultate\Licenta\Shannon.jar");
        }
    }
}

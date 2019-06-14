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

        public string SSourceFileName { get; set; }
        internal AVQ AvqCompression { get; set; } = null;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            SSourceFileName = openFileDialog.FileName;

            try
            {
                panelOriginalImage.BackgroundImage = new Bitmap(SSourceFileName);
            }
            catch (Exception)
            {
                Console.WriteLine("File {0} not found", SSourceFileName);
            }
            
            
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;
            buttonLoad.Enabled = false;

            if (SSourceFileName != null)
            {
                AvqCompression = new AVQ(SSourceFileName);
                bool drawBorder = checkBoxDrawBorder.Checked;
                int.TryParse(textBoxThreshold.Text, out int threshold);
                int.TryParse(textBoxDictionarySize.Text, out int dictionarySize);
               
             

                originalImage = AvqCompression.StartCompression(threshold, dictionarySize, drawBorder);
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
    }
}

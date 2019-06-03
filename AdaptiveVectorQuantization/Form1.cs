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

        internal AVQ AvqCompression { get; set; } = null;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            string sSourceFileName = openFileDialog.FileName;
            panelOriginalImage.BackgroundImage = new Bitmap(sSourceFileName);
            
            AvqCompression = new AVQ(sSourceFileName);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (AvqCompression != null)
            {
                if (int.TryParse(textBoxThreshold.Text, out int threshold))
                {
                    originalImage = AvqCompression.TestDictionar(threshold);
                }
                else
                {
                    originalImage = AvqCompression.TestDictionar(0);
                }

                panelDestination.BackgroundImage = null;
                panelDestination.BackgroundImage = originalImage.GetBitMap();
            }
            else
            {
                MessageBox.Show("You need to choose an image!");
            }
        }
    }
}

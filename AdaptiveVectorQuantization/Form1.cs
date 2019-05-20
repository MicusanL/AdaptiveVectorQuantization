using System;
using System.Drawing;
using System.Windows.Forms;

namespace AdaptiveVectorQuantization
{
    public partial class FormAVQ : Form
    {

        private FastImage originalImage;
        
        private AVQ avqCompression = null;

        public FormAVQ()
        {
            InitializeComponent();
        }

        internal AVQ AvqCompression { get => avqCompression; set => avqCompression = value; }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            string sSourceFileName = openFileDialog.FileName;
            panelOriginalImage.BackgroundImage = new Bitmap(sSourceFileName);
            
            avqCompression = new AVQ(sSourceFileName);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {

            originalImage = avqCompression.TestDictionar();
            panelDestination.BackgroundImage = null;
            panelDestination.BackgroundImage = originalImage.GetBitMap();
        }
    }
}

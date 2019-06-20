﻿using System;
using System.Drawing;
using System.IO;
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
        public static string InputFileComp { get; set; }
        internal AVQ AvqCompression { get; set; } = null;

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG)|*.BMP;*.JPG|All files (*.*)|*.*";
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
            invertFormAcces();

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


            invertFormAcces();
        }

        private void buttonShannon_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"D:\Facultate\Licenta\Shannon.jar");
        }
        private void invertFormAcces()
        {
            buttonStart.Enabled = !buttonStart.Enabled;
            buttonLoad.Enabled = !buttonLoad.Enabled;
            buttonDecode.Enabled = !buttonDecode.Enabled;
            buttonShannon.Enabled = !buttonShannon.Enabled;
        }

        private void buttonDecode_Click(object sender, EventArgs e)
        {

            invertFormAcces();
            openFileDialog.Filter = "AVQ Files(*.AVQ)|*.AVQ|All files (*.*)|*.*";
            openFileDialog.ShowDialog();
            InputFileComp = openFileDialog.FileName;

            if (InputFileComp != null)
            {
                AvqCompression = new AVQ();

                originalImage = AvqCompression.StartDeCompression(InputFileComp);
                Bitmap bitmap = originalImage.GetBitMap();

                string[] output = InputFileComp.Split('.');
                bitmap.Save(output[0] + "-decoded.bmp");
            }
            else
            {
                MessageBox.Show("You need to choose an image!");
            }

            invertFormAcces();
        }
    }
}

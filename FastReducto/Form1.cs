using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastReducto
{
    public partial class FastReducto : Form
    {
        Octree Octree, StepOctree;
        DirectBitmap OriginalImage, ReductedImage, StepReductedImage, GrayImage;
        int ColorsNumber;

        public FastReducto()
        {
            InitializeComponent();
            LoadForm();
        }

        private void LoadForm()
        {
            ColorsNumber = 16;
            LoadImage(true);
            textBox1.Text = ColorsNumber.ToString();
            GrayScaleComboBox.SelectedIndex = 0;
            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void ReductoButtom_Click(object sender, EventArgs e)
        {
            ReductoButton.Enabled = false;
            int number;
            //wczytanie ilości kolorów wynikowych
            if (textBox1.Text != "" && int.TryParse(textBox1.Text, out number))
                ColorsNumber = number;
            //odpalenie progress bara
            progressBar1.Visible = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            DirectBitmap origin;
            if (GrayScaleCheckBox.Checked)
                origin = GrayImage;
            else
                origin = OriginalImage;
            //wczytanie do drzewa oryginalnego obrazka
            LoadColorPalette(ColorsNumber, origin);
            //redukcja
            Octree.ReductTree(ColorsNumber);
            //generacja obrazków
            GenerateReductedImage(origin);

            backgroundWorker1.ReportProgress(100);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            ReductoButton.Enabled = true;
        }

        private void LoadColorPalette(int colors_count, DirectBitmap origin)
        {
            Octree = new Octree();
            StepOctree = new Octree();
            for (int i = 0; i < origin.Width; i++)
            {
                for (int j = 0; j < origin.Height; j++)
                {
                    Color c = origin.GetPixel(i, j);
                    Octree.AddColor(c);
                    StepOctree.AddAndReductColor(c, colors_count);
                }
                backgroundWorker1.ReportProgress((66 * i) / origin.Width);
            }
        }

        private void GenerateReductedImage(DirectBitmap origin)
        {
            pictureBox2.BackgroundImage = null;
            pictureBox3.BackgroundImage = null;

            ReductedImage?.Dispose();
            StepReductedImage?.Dispose();
            ReductedImage = new DirectBitmap(origin.Width, origin.Height);
            StepReductedImage = new DirectBitmap(origin.Width, origin.Height);
            int v = progressBar1.Value;
            for (int i = 0; i < origin.Width; i++)
            {
                for (int j = 0; j < origin.Height; j++)
                {
                    ReductedImage.SetPixel(i, j, Octree.TranslateColor(origin.GetPixel(i, j)));
                    StepReductedImage.SetPixel(i, j, StepOctree.TranslateColor(origin.GetPixel(i, j)));
                }
                backgroundWorker1.ReportProgress(66 + (33 * i) / origin.Width);
            }
            //Parallel.For(0, ReductedImage.Width,
            //i =>
            //{
            //    for(int j=0;j<ReductedImage.Height;j++)
            //    {
            //        ReductedImage.SetPixel(i, j, Octree.TranslateColor(OriginalImage.GetPixel(i, j)));
            //        StepReductedImage.SetPixel(i, j, StepOctree.TranslateColor(OriginalImage.GetPixel(i, j)));
            //    }
            //});
            pictureBox2.BackgroundImage = ReductedImage.Bitmap;
            pictureBox3.BackgroundImage = StepReductedImage.Bitmap;
        }

        private void GrayScaleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GrayScaleCheckBox.Checked)
            {
                MakePictureGrayAgain(GrayScaleComboBox.SelectedIndex);
                pictureBox1.BackgroundImage = GrayImage.Bitmap;
            }
        }

        private void MakePictureGrayAgain(int method)
        {
            GrayImage?.Dispose();
            GrayImage = new DirectBitmap(OriginalImage.Bitmap);
            switch(method)
            {
                case 0:
                    GrayImage.LightnessGray();
                    break;
                case 1:
                    GrayImage.AverageGray();
                    break;
                case 2:
                    GrayImage.LuminosityGray();
                    break;
                default:
                    MessageBox.Show("Akuku");
                    break;
            }
        }

        private void LoadImage(bool def = false)
        {
            Bitmap file = null;
            if (def)
            {
                file = new Bitmap(Properties.Resources.Lenna__test_image_);
            }
            else
            {
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    file = new Bitmap(openFileDialog1.FileName);
                }
                openFileDialog1.Dispose();
            }
            if(file!=null)
            {
                OriginalImage?.Dispose();
                OriginalImage = new DirectBitmap(file);

                //for (int i = 0; i < file.Width; i++)
                //    for (int j = 0; j < file.Height; j++)
                //        OriginalImage.SetPixel(i, j, file.GetPixel(i, j));


                pictureBox1.BackgroundImage = OriginalImage.Bitmap;
            }
        }

        private void GrayScaleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            GrayScaleComboBox.Visible = !GrayScaleComboBox.Visible;
            if (!GrayScaleCheckBox.Checked)
                pictureBox1.BackgroundImage = OriginalImage.Bitmap;
            else
            {
                MakePictureGrayAgain(GrayScaleComboBox.SelectedIndex);
                pictureBox1.BackgroundImage = GrayImage.Bitmap;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            LoadImage();
        }
    }
}

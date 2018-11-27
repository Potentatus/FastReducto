using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastReducto
{
    public partial class Form1 : Form
    {
        Octree Octree;
        Bitmap OriginalImage, ReductedImage;
        int ColorsNumber;

        public Form1()
        {
            InitializeComponent();
            LoadForm();
        }

        private void LoadForm()
        {
            Octree = new Octree();
            ColorsNumber = 16;
            LoadImage(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int number;
            //wczytanie ilości kolorów wynikowych
            if (textBox1.Text != "" && int.TryParse(textBox1.Text, out number))
                ColorsNumber = number;
            //wczytanie do drzewa oryginalnego obrazka
            LoadColorPalette();
            //redukcja
            Octree.ReductTree(ColorsNumber);

            GenerateReductedImage();
        }

        private void LoadColorPalette()
        {
            Octree = new Octree();
            for (int i = 0; i < OriginalImage.Width; i++)
                for (int j = 0; j < OriginalImage.Height; j++)
                {
                    Octree.AddColor(OriginalImage.GetPixel(i, j));
                }
        }

        private void GenerateReductedImage()
        {
            ReductedImage = new Bitmap(OriginalImage);
            for(int i=0;i<ReductedImage.Width;i++)
                for(int j=0;j<ReductedImage.Height;j++)
                {
                    ReductedImage.SetPixel(i, j, Octree.TranslateColor(OriginalImage.GetPixel(i, j)));
                }
            pictureBox2.Image = ReductedImage;
        }

        private void LoadImage(bool def = false)
        {
            if (def)
                OriginalImage = new Bitmap(Properties.Resources.Lenna__test_image_);
            else
            {
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OriginalImage = new Bitmap(openFileDialog1.FileName);
                }
                openFileDialog1.Dispose();
            }
            pictureBox1.Image = OriginalImage;
        }
    }
}

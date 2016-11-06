using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ProcessingImageSDK;

namespace CIPP
{
    public partial class ViewMotionForm : Form
    {
        public ViewMotionForm(Motion motion)
        {
            InitializeComponent();

            PictureBox p = null;
            for (int index = 0; index < motion.imageNumber; index++)
            {
                p = new PictureBox();
                p.Size = new Size(motion.imageList[index].getSizeX(), motion.imageList[index].getSizeY());
                p.BackgroundImage = motion.imageList[index].getBitmap(ProcessingImageBitmapType.AlphaColor);

                if (index < motion.imageNumber - 1)
                {
                    Bitmap b = new Bitmap(p.Size.Width, p.Size.Height);
                    Graphics g = Graphics.FromImage(b);
                    g.Clear(Color.Transparent);

                    Pen pen1 = new Pen(Color.Red);
                    Pen pen2 = new Pen(Color.Blue);

                    int y = motion.searchDistance + motion.blockSize / 2;
                    for (int i = 0; i < motion.vectors[index].GetLength(0); i++)
                    {
                        int x = motion.searchDistance + motion.blockSize / 2;
                        for (int j = 0; j < motion.vectors[index].GetLength(1); j++)
                        {
                            if (motion.vectors[index][i, j].x != 0 || motion.vectors[index][i, j].y != 0)
                            {
                                g.DrawLine(pen1, new Point(x, y), new Point(x + motion.vectors[index][i, j].x, y + motion.vectors[index][i, j].y));
                                g.DrawEllipse(pen2, x + motion.vectors[index][i, j].x - 1, y + motion.vectors[index][i, j].y - 1, 2, 2);
                            }
                            x += motion.blockSize;
                        }
                        y += motion.blockSize;
                    }
                    p.Image = b;
                }
                imagesFlowLayoutPanel.Controls.Add(p);
            }
        }
    }
}
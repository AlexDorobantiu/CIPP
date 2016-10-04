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
    public partial class ViewImageForm : Form
    {
        ProcessingImage pi;
        public bool loaded;
        public ViewImageForm(ProcessingImage image)
        {
            loaded = false;
            InitializeComponent();
            pi = image;
            piBox.Width = pi.getSizeX();
            piBox.Height = pi.getSizeY();
            if (pi.grayscale)
            {
                redCheckBox.Checked = false; redCheckBox.Enabled = false;
                greenCheckBox.Checked = false; greenCheckBox.Enabled = false;
                blueCheckBox.Checked = false; blueCheckBox.Enabled = false;
                luminanceCheckButton.Checked = false; luminanceCheckButton.Enabled = false;
                grayCheckButton.Enabled = true; grayCheckButton.Checked = true;
            }
            redrawImage();
            loaded = true;
        }

        private void redrawImage() 
        {
            piBox.Image = null;

            if (alphaCheckBox.Checked)
            {
                if (redCheckBox.Checked)
                {
                    if (greenCheckBox.Checked)
                    {
                        if (blueCheckBox.Checked)
                        {
                            piBox.Image = pi.getBitmap(BitmapType.AlphaColor);
                            return;
                        }
                        piBox.Image = pi.getBitmap(BitmapType.AlphaRedGreen);
                        return;
                    }
                    if (blueCheckBox.Checked)
                    {
                        piBox.Image = pi.getBitmap(BitmapType.AlphaRedBlue);
                        return;
                    }
                    piBox.Image = pi.getBitmap(BitmapType.AlphaRed);
                    return;
                }
                if (greenCheckBox.Checked)
                {
                    if (blueCheckBox.Checked)
                    {
                        piBox.Image = pi.getBitmap(BitmapType.AlphaGreenBlue);
                        return;
                    }
                    piBox.Image = pi.getBitmap(BitmapType.AlphaGreen);
                    return;
                }
                if (blueCheckBox.Checked)
                {
                    piBox.Image = pi.getBitmap(BitmapType.AlphaBlue);
                    return;
                }
                if (grayCheckButton.Checked)
                {
                    piBox.Image = pi.getBitmap(BitmapType.AlphaGray);
                    return;
                }
                if (luminanceCheckButton.Checked)
                {
                    piBox.Image = pi.getBitmap(BitmapType.AlphaLuminance);
                    return;
                }
                piBox.Image = pi.getBitmap(BitmapType.Alpha);
                return;
            }
            if (redCheckBox.Checked)
            {
                if (greenCheckBox.Checked)
                {
                    if (blueCheckBox.Checked)
                    {
                        piBox.Image = pi.getBitmap(BitmapType.Color);
                        return;
                    }
                    piBox.Image = pi.getBitmap(BitmapType.RedGreen);
                    return;
                }
                if (blueCheckBox.Checked)
                {
                    piBox.Image = pi.getBitmap(BitmapType.RedBlue);
                    return;
                }
                piBox.Image = pi.getBitmap(BitmapType.Red);
                return;
            }
            if (greenCheckBox.Checked)
            {
                if (blueCheckBox.Checked)
                {
                    piBox.Image = pi.getBitmap(BitmapType.GreenBlue);
                    return;
                }
                piBox.Image = pi.getBitmap(BitmapType.Green);
                return;
            }
            if (blueCheckBox.Checked)
            {
                piBox.Image = pi.getBitmap(BitmapType.Blue);
                return;
            }
            if (grayCheckButton.Checked)
            {
                piBox.Image = pi.getBitmap(BitmapType.Gray);
                return;
            }
            if (luminanceCheckButton.Checked)
            {
                piBox.Image = pi.getBitmap(BitmapType.Luminance);
                return;
            }
        }

        private void checkerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) return;
            try
            {
                if (checkerCheckBox.Checked) this.piBox.BackgroundImage = new Bitmap("checkers.png");
                else
                {
                    this.piBox.BackgroundImage = null;
                    GC.Collect();
                }
            }
            catch { }
        }

        private void grayCheckButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) return;
            if (grayCheckButton.Checked)
            {
                redCheckBox.Checked = false;
                greenCheckBox.Checked = false;
                blueCheckBox.Checked = false;
                luminanceCheckButton.Checked = false;
            }
            redrawImage();
        }

        private void luminanceCheckButton_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) return;
            if (luminanceCheckButton.Checked)
            {
                redCheckBox.Checked = false;
                greenCheckBox.Checked = false;
                blueCheckBox.Checked = false;
                grayCheckButton.Checked = false;
            }
            redrawImage();
        }

        private void alphaCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            redrawImage();
        }

        private void redCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) return;
            if (redCheckBox.Checked)
            {
                grayCheckButton.Checked = false;
                luminanceCheckButton.Checked = false;
            }
            redrawImage();
        }

        private void greenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) return;
            if (greenCheckBox.Checked)
            {
                grayCheckButton.Checked = false;
                luminanceCheckButton.Checked = false;
            }
            redrawImage();
        }

        private void blueCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) return;
            if (blueCheckBox.Checked)
            {
                grayCheckButton.Checked = false;
                luminanceCheckButton.Checked = false;
            }
            redrawImage();
        }

        private void ViewImageForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            pi = null;
            GC.Collect();
        }
    }
}
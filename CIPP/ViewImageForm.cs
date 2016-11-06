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
                            piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaColor);
                            return;
                        }
                        piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaRedGreen);
                        return;
                    }
                    if (blueCheckBox.Checked)
                    {
                        piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaRedBlue);
                        return;
                    }
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaRed);
                    return;
                }
                if (greenCheckBox.Checked)
                {
                    if (blueCheckBox.Checked)
                    {
                        piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaGreenBlue);
                        return;
                    }
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaGreen);
                    return;
                }
                if (blueCheckBox.Checked)
                {
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaBlue);
                    return;
                }
                if (grayCheckButton.Checked)
                {
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaGray);
                    return;
                }
                if (luminanceCheckButton.Checked)
                {
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.AlphaLuminance);
                    return;
                }
                piBox.Image = pi.getBitmap(ProcessingImageBitmapType.Alpha);
                return;
            }
            if (redCheckBox.Checked)
            {
                if (greenCheckBox.Checked)
                {
                    if (blueCheckBox.Checked)
                    {
                        piBox.Image = pi.getBitmap(ProcessingImageBitmapType.Color);
                        return;
                    }
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.RedGreen);
                    return;
                }
                if (blueCheckBox.Checked)
                {
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.RedBlue);
                    return;
                }
                piBox.Image = pi.getBitmap(ProcessingImageBitmapType.Red);
                return;
            }
            if (greenCheckBox.Checked)
            {
                if (blueCheckBox.Checked)
                {
                    piBox.Image = pi.getBitmap(ProcessingImageBitmapType.GreenBlue);
                    return;
                }
                piBox.Image = pi.getBitmap(ProcessingImageBitmapType.Green);
                return;
            }
            if (blueCheckBox.Checked)
            {
                piBox.Image = pi.getBitmap(ProcessingImageBitmapType.Blue);
                return;
            }
            if (grayCheckButton.Checked)
            {
                piBox.Image = pi.getBitmap(ProcessingImageBitmapType.Gray);
                return;
            }
            if (luminanceCheckButton.Checked)
            {
                piBox.Image = pi.getBitmap(ProcessingImageBitmapType.Luminance);
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
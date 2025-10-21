using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Monopoly
{
    public partial class Game : Form
    {
        public Game()
        {
            InitializeComponent();
            LoadForm();
            RotateLabel(label3, -90f);

        }
        private void LoadForm()
        {

            if (Forms.isOnline)
            {

                pcbChat.Visible = true;
            }
            else
            {
                pcbChat.Visible = false;
            }
        }

        private void Game_FormClosing(object sender, FormClosingEventArgs e)
        {
            Forms.Dispose();
        }

        private void pcbReturn_Click(object sender, EventArgs e)
        {
            this.Hide();
            Forms.menu.Show();
        }

        private void pcbReturn_MouseEnter(object sender, EventArgs e)
        {
            pcbReturn.Size = new Size(110, 110);
        }

        private void pcbReturn_MouseLeave(object sender, EventArgs e)
        {
            pcbReturn.Size = new Size(100, 100);
        }

        private void pcbSave_MouseEnter(object sender, EventArgs e)
        {
            pcbSave.Size = new Size(110, 110);
        }

        private void pcbSave_MouseLeave(object sender, EventArgs e)
        {
            pcbSave.Size = new Size(100, 100);
        }

        private void pcbChat_MouseHover(object sender, EventArgs e)
        {
            pcbChat.Size = new Size(110, 110);
        }

        private void pcbChat_MouseEnter(object sender, EventArgs e)
        {
            pcbChat.Size = new Size(110, 110);
        }

        private void pcbChat_MouseLeave(object sender, EventArgs e)
        {
            pcbChat.Size = new Size(100, 100);
        }

        private void pcbInfo_DragEnter(object sender, DragEventArgs e)
        {
            pcbInfo.Size = new Size(110, 110);
        }

        private void pcbInfo_MouseEnter(object sender, EventArgs e)
        {
            pcbInfo.Size = new Size(110, 110);
        }

        private void pcbInfo_MouseLeave(object sender, EventArgs e)
        {
            pcbInfo.Size = new Size(100, 100);
        }
        public void RotateLabel(Label lbl, float angle)
        {
            lbl.Visible = false;
            PictureBox rotated = new PictureBox
            {
                Width = lbl.Width,
                Height = lbl.Height,
                Location = lbl.Location,
                BackColor = lbl.BackColor,
                Parent = lbl.Parent
            };

            rotated.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                e.Graphics.TranslateTransform(rotated.Width / 2f, rotated.Height / 2f);
                e.Graphics.RotateTransform(angle);
                e.Graphics.TranslateTransform(-rotated.Width / 2f, -rotated.Height / 2f);
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                using (Brush b = new SolidBrush(lbl.ForeColor))
                {
                    e.Graphics.DrawString(lbl.Text, lbl.Font, b,
                        new RectangleF(0, 0, rotated.Width, rotated.Height), format);
                }

                e.Graphics.ResetTransform();
            };
            rotated.Invalidate();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}

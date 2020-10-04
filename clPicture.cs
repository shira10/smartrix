using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smartrix
{
    public partial class clPicture : PictureBox
    {
        public int[] vecValues = new int[9];
        public int rowInSavepic;

        public clPicture()
        {
            InitializeComponent();
        }
        
        public clPicture(int r)
        {
            InitializeComponent();
            setValues(r);
        }

        public void setValues(int r)
        {
            rowInSavepic = r;
            for (int i = 0; i < 9; i++)
            {
                vecValues[i] = clGlobal.savePic[r, i];
            }
        }

        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // clPicture
            // 
            this.MouseLeave += new System.EventHandler(this.clPicture_MouseLeave);
            this.Click += new System.EventHandler(this.clPicture_Click);
            this.MouseEnter += new System.EventHandler(this.clPicture_MouseEnter);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        private void clPicture_Click(object sender, EventArgs e)
        {
            if (((clPicture)sender).Name.StartsWith("player"))
            {
                if (((PictureBox)sender).BackgroundImage != null)
                {
                    ((clPicture)this.Parent.Parent.Controls["SelectedCard"]).BackgroundImage = ((clPicture)sender).BackgroundImage;
                    ((clPicture)this.Parent.Parent.Controls["SelectedCard"]).setValues(Convert.ToInt32(((clPicture)sender).rowInSavepic));
                }
            }
        }

        private void clPicture_MouseEnter(object sender, EventArgs e)
        {
            if (((clPicture)sender).Name.StartsWith("player"))
            {
                if (((PictureBox)sender).BackgroundImage != null)
                    ((PictureBox)sender).BorderStyle = BorderStyle.FixedSingle;
            }
        }

        private void clPicture_MouseLeave(object sender, EventArgs e)
        {
            if (((clPicture)sender).Name.StartsWith("player"))
            {
                if (((PictureBox)sender).BackgroundImage != null)
                    ((PictureBox)sender).BorderStyle = BorderStyle.Fixed3D;
            }
        }

        public void RotateVecValues()
        {
            int k;
            int[] tempVec = new int[9];
            //שמירת המטריצה במטריצה זמנית
            for (k = 0; k < 9; k++)
                tempVec[k] = ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[k];

            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[0] = tempVec[6];
            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[1] = tempVec[3];
            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[2] = tempVec[0];
            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[3] = tempVec[7];
            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[5] = tempVec[1];
            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[6] = tempVec[8];
            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[7] = tempVec[5];
            ((clPicture)this.Parent.Controls["SelectedCard"]).vecValues[8] = tempVec[2];

        }
    }
}

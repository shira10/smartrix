using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;

namespace Smartrix
{
    public delegate void dlPassTurn();
    public partial class clPicBoard : PictureBox
    {
        public dlPassTurn dlPass;
        public Boolean placeFlag = false, rotateFlag = true;
        public int[][] matValues = new int[3][];
        int picMis;
        Random rnd = new Random();
        Timer t = new Timer();
        int stopShow = 0;

        public Timer T
        {
            get { return t; }
            set { t = value; }
        }

        Image myBackGround = null;

        public clPicBoard()
        {
            InitializeComponent();

            for (int i = 0; i < 3; i++)
            {
                matValues[i] = new int[3];
            }
        }

        private void InitializeComponent()
        {
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // clPicBoard
            // 
            this.MouseLeave += new System.EventHandler(this.clPicBoard_MouseLeave);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.clPicBoard_MouseClick);
            this.MouseEnter += new System.EventHandler(this.clPicBoard_MouseEnter);
            this.t.Tick += new System.EventHandler(this.timer_Tick);
            this.t.Interval = 100;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        void placingCard(object sender, EventArgs e)
        //Occurs after click on a picturebox on the board
        {
            int[,] cardValues = new int[3, 3];
            //int[] cardValues = {1,2,3,4,5,6,7,8,9};

            //find card's location (using pictureBox Tag)
            int i, j;
            i = Convert.ToInt32(((clPicBoard)sender).Tag) / 9;
            j = Convert.ToInt32(((clPicBoard)sender).Tag) % 9;

            //Check if place is empty
            if (((clPicBoard)sender).BackgroundImage != ((clPicture)this.Parent.Controls["SelectedCard"]).BackgroundImage) //מקום תפוס
            {
                DialogOK FullDialog = new DialogOK("המקום שבחרת תפוס", "מהלך לא חוקי");
                FullDialog.ShowDialog();
            }

            else
            {
                //בדיקה שהקלף משיק לפחות לקלף אחד על הלוח
                if (checkTouching(i, j) == false) //אין השקה
                {
                    DialogOK TouchDialog = new DialogOK("אין השקה", "מהלך לא חוקי");
                    TouchDialog.ShowDialog();
                    ((clPicBoard)sender).BackgroundImage = null; //ריקון המשבצת
                }
                else // נמצאה השקה - המשך בדיקה - התאמה
                {
                    //בדיקה שהקלף שהונח לא יוצא מגבולות מסגרת של 
                    //5X5
                    if (checkBorder(i, j) == false) //נמצאה חריגה מהמסגרת
                    {
                        DialogOK BorderDialog = new DialogOK("חריגה מגבולות הלוח 5*5", "מהלך לא חוקי");
                        BorderDialog.ShowDialog();
                        ((clPicBoard)sender).BackgroundImage = null; //ריקון המשבצת
                    }
                    else //הקלף הונח בגבולות המסגרת
                    {
                        //בדיקת התאמה של הקלף ל"מסגרת"
                        int[][] matNew = new int[3][];
                        for (int q = 0; q < 3; matNew[q++] = new int[3]) ;
                        int p = 0;
                        for (int s = 0; s < 3; s++)
                            for (int r = 0; r < 3; r++)
                                matNew[s][r] = ((clPicture)Parent.Controls["SelectedCard"]).vecValues[p++];


                        if (checkAllNeighbours(i, j, matNew) == false) //אין התאמה
                        {
                            DialogOK MatchDialog = new DialogOK("אין התאמה", "מהלך לא חוקי");
                            MatchDialog.ShowDialog();
                            ((clPicBoard)sender).BackgroundImage = null; //ריקון המשבצת
                        }
                        else  //נמצאה התאמה - הנחת קלף
                        {
                            placeFlag = true; //סימון המקום כתפוס
                            rotateFlag = false; //סימון המקום כלא ניתן לסיבוב
                            clGlobal.boardStatus[i, j] = 1;

                            //בדיקת רצף לפי הקלף שהונח: טור, שורה, אלכסון
                            if (checkSequence(i, j, 0) == true)
                                ((frmMainScreen)this.Parent).timerDelay.Enabled = true;

                            //החלפת קלף
                            ((frmMainScreen)Parent).AddCardToPnl();

                            //העברת תור למחשב
                            dlPass();

                        }
                    }
                }
            }
        }

        public bool checkBorder(int iMatrix, int jMatrix)
        {
            // בדיקת כל משבצת תפוסה האם עבר את התווח 5 על 5 מול המיקום להנחה 
            int i, j;
            for (i = 0; i < 9; i++)
            {
                for (j = 0; j < 9; j++)
                    if (clGlobal.boardStatus[i, j] == 1)
                    {
                        if (Math.Abs(jMatrix - j) >= 5)
                            return false;
                        if (Math.Abs(iMatrix - i) >= 5)
                            return false;
                    }
            }

            return true;
        }

        public bool checkSequence(int iMatrix, int jMatrix, int id)
        {
            //הפונקציה מקבלת מיקום המטריצה של הקלף שהונח ובודקת האם נוצר רצף
            //id = 1 -> computer, id = 0 -> player


            int Scores = 0;
            int cnt, tzover;
            int i, j;
            List<clPicBoard> listPic = new List<clPicBoard>();
            List<clPicBoard> listTemp = new List<clPicBoard>();
            //--- Check row sequence --- 
            cnt = 1; tzover = 0;
            //right
            for (j = jMatrix + 1; j < 9 && ((clPicBoard)this.Parent.Controls[iMatrix.ToString() + j.ToString()]).BackgroundImage != null; j++)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[iMatrix.ToString() + j.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[iMatrix.ToString() + j.ToString()]);
            }
            //left
            for (j = jMatrix - 1; j >= 0 && ((clPicBoard)this.Parent.Controls[iMatrix.ToString() + j.ToString()]).BackgroundImage != null; j--)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[iMatrix.ToString() + j.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[iMatrix.ToString() + j.ToString()]);
            }
            if (cnt >= 5) //נמצא רצף שורה
            {
                Scores += tzover;
                foreach (var pic in listTemp)
                    listPic.Add(pic);
            }
            //--- Check column sequence --- 
            cnt = 1; tzover = 0;
            listTemp.Clear();
            //up
            for (i = iMatrix + 1; i < 9 && ((clPicBoard)this.Parent.Controls[i.ToString() + jMatrix.ToString()]).BackgroundImage != null; i++)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[i.ToString() + jMatrix.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[i.ToString() + jMatrix.ToString()]);
            }
            //down
            for (i = iMatrix - 1; i >= 0 && ((clPicBoard)this.Parent.Controls[i.ToString() + jMatrix.ToString()]).BackgroundImage != null; i--)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[i.ToString() + jMatrix.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[i.ToString() + jMatrix.ToString()]);
            }
            if (cnt >= 5) //נמצא רצף עמודה
            {
                Scores += tzover;
                foreach (var pic in listTemp)
                    listPic.Add(pic);
            }
            //--- Check MainDiagonal sequence --- 
            cnt = 1; tzover = 0;
            listTemp.Clear();
            //down
            for (i = iMatrix + 1, j = jMatrix + 1; i < 9 && j < 9 && ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).BackgroundImage != null; i++, j++)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]);
            }
            //up
            for (i = iMatrix - 1, j = jMatrix - 1; i >= 0 && j >= 0 && ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).BackgroundImage != null; i--, j--)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]);
            }
            if (cnt >= 5) //נמצא רצף אלכסון ראשי
            {
                Scores += tzover;
                foreach (var pic in listTemp)
                    listPic.Add(pic);
            }

            //--- Check SecondDiagonal sequence --- 
            cnt = 1; tzover = 0;
            listTemp.Clear();
            //up
            for (i = iMatrix - 1, j = jMatrix + 1; i >= 0 && j < 9 && ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).BackgroundImage != null; i--, j++)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]);
            }
            //down
            for (i = iMatrix + 1, j = jMatrix - 1; i < 9 && j >= 0 && ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).BackgroundImage != null; i++, j--)
            {
                cnt++;
                tzover += ((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]).matValues[1][1];
                listTemp.Add((clPicBoard)this.Parent.Controls[i.ToString() + j.ToString()]);
            }
            if (cnt >= 5) //נמצא רצף אלכסון משני
            {
                Scores += tzover;
                foreach (var pic in listTemp)
                    listPic.Add(pic);
            }
            //------------------------------------------------------
            //אם נמצא רצף כלשהו - הוספת ערך הקלף הנוכחי
            if (Scores > 0)
            {
                Scores += ((clPicBoard)this.Parent.Controls[iMatrix.ToString() + jMatrix.ToString()]).matValues[1][1];
                listPic.Add((clPicBoard)this.Parent.Controls[iMatrix.ToString() + jMatrix.ToString()]);
                //MessageBox.Show("Scores: " + Scores.ToString());
                foreach (var pic in listPic) //הפעלת הבהוב ומחיקה של הרצף מהלוח
                    pic.T.Enabled = true;
                ((frmMainScreen)this.Parent).timer1.Enabled = true;

                if (id == 1) //computer
                {
                    ((frmMainScreen)this.Parent).lblScoresComputer.Text = (clGlobal.computerScores + Scores).ToString();
                    clGlobal.computerScores += Scores;
                }
                else //(id == 0) player
                {
                    ((frmMainScreen)this.Parent).lblScoresPlayer.Text = (clGlobal.playerScores + Scores).ToString();
                    clGlobal.playerScores += Scores;
                }
                return true;
            }
            return false;

        }

        private bool checkTouching(int iMatrix, int jMatrix)
        {
            //שמירת ערכי נקודות המסגרת המשיקות לקלף
            int[] iArr = { iMatrix - 1, iMatrix + 1, iMatrix, iMatrix, iMatrix - 1, iMatrix - 1, iMatrix + 1, iMatrix + 1 };
            int[] jArr = { jMatrix, jMatrix, jMatrix - 1, jMatrix + 1, jMatrix - 1, jMatrix + 1, jMatrix + 1, jMatrix - 1 };

            //בדיקת השקה לקלף K
            for (int k = 0; k < 8; k++)
                try
                {
                    if (((clPicBoard)this.Parent.Controls[iArr[k].ToString() + jArr[k].ToString()]).BackgroundImage != null)
                        return true;
                }
                catch { };

            return false;
        }

        public void saveCardValues(int picNum)
        {
            for (int i = 0, k = 0; i < 3; i++)
                for (int j = 0; j < 3; j++, k++)
                    matValues[i][j] = clGlobal.savePic[picNum, k];
        }

        public void saveCardValues(int[] vecValues)
        {
            for (int i = 0, k = 0; i < 3; i++)
                for (int j = 0; j < 3; j++, k++)
                    matValues[i][j] = vecValues[k];
        }

        public Boolean checkAllNeighbours(int iMatrix, int jMatrix, int[][] tempMat)
        {
            //העתקת הערכים למטריצה זמנית שהתמונה הנבחרת 
            /*   int k = 0;
               int[,] tempMat = new int[3,3];
               for (int i = 0; i < 3; i++)
                   for(int j = 0; j<3; j++)                
                       tempMat[i, j] = vecValues[k++];*/

            //בדיקת שכנים שורות למעלה למטה
            try //up
            {
                if (checkRow(0, 2, iMatrix - 1, jMatrix) == false)
                    return false;
            }
            catch { };
            try //down
            {
                if (checkRow(2, 0, iMatrix + 1, jMatrix) == false)
                    return false;
            }
            catch { };
            //בדיקת שכנים עמודות ימין שמאל
            try //left
            {
                if (checkCol(0, 2, iMatrix, jMatrix - 1) == false)
                    return false;
            }
            catch { };
            try //right
            {
                if (checkCol(2, 0, iMatrix, jMatrix + 1) == false)
                    return false;
            }
            catch { };
            //בדיקת אלכסונים
            try //up-left
            {
                if (((clPicBoard)this.Parent.Controls[(iMatrix - 1).ToString() + (jMatrix - 1).ToString()]).BackgroundImage != null)
                    if (tempMat[0][0] != ((clPicBoard)this.Parent.Controls[(iMatrix - 1).ToString() + (jMatrix - 1).ToString()]).getDiag(2, 2))
                        return false;
            }
            catch { };
            try //up-right
            {
                if (((clPicBoard)this.Parent.Controls[(iMatrix - 1).ToString() + (jMatrix + 1).ToString()]).BackgroundImage != null)
                    if (tempMat[0][2] != ((clPicBoard)this.Parent.Controls[(iMatrix - 1).ToString() + (jMatrix + 1).ToString()]).getDiag(2, 0))
                        return false;
            }
            catch { };
            try //down-right
            {
                if (((clPicBoard)this.Parent.Controls[(iMatrix + 1).ToString() + (jMatrix + 1).ToString()]).BackgroundImage != null)
                    if (tempMat[2][2] != ((clPicBoard)this.Parent.Controls[(iMatrix + 1).ToString() + (jMatrix + 1).ToString()]).getDiag(0, 0))
                        return false;
            }
            catch { };
            try //down-left
            {
                if (((clPicBoard)this.Parent.Controls[(iMatrix + 1).ToString() + (jMatrix - 1).ToString()]).BackgroundImage != null)
                    if (tempMat[2][0] != ((clPicBoard)this.Parent.Controls[(iMatrix + 1).ToString() + (jMatrix - 1).ToString()]).getDiag(0, 2))
                        return false;
            }
            catch { };

            return true;
        }

        private void clPicBoard_MouseLeave(object sender, EventArgs e)
        {
            if (placeFlag == false)
            {
                //אחרי מעבר עכבר בלי קליק -בלי מיקום
                ((clPicBoard)sender).BackgroundImage = null; //ריקון המשבצת
            }
        }

        private void clPicBoard_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                if (((clPicBoard)sender).BackgroundImage == null) //אם ריקה- תמקם תמונה
                {
                    ((clPicBoard)sender).BackgroundImage = ((clPicture)this.Parent.Controls["SelectedCard"]).BackgroundImage;
                    //מילוי מטריצה
                    //((clPicBoard)sender).saveCardValues(((clPicture)this.Parent.Controls["SelectedCard"]).rowInSavepic); //שמירת ערכי הקלף במטריצה של ה-PICBOARD
                    ((clPicBoard)sender).saveCardValues(((clPicture)this.Parent.Controls["SelectedCard"]).vecValues); //שמירת ערכי הקלף במטריצה של ה-PICBOARD
                    ((clPicBoard)sender).picMis = ((clPicture)this.Parent.Controls["SelectedCard"]).rowInSavepic; //שמירת מספר התמונה של הקלף
                }
                else
                    placeFlag = true;
            }
            catch { }
        }

        private void clPicBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.ToString() == "Left")  //Left click -> Placing Card
            {
                //Check and place card
                placingCard(sender, (EventArgs)e);
            }
            else                                //Right click -> Rotate Image
            {
                if (rotateFlag == true) //סיבוב רק אם הקלף לא מקובע
                {
                    ((clPicBoard)sender).BackgroundImage = utilities.RotateImage(((PictureBox)sender).BackgroundImage, 90); //Rotate Sender PicBoard
                    ((clPicture)this.Parent.Controls["SelectedCard"]).BackgroundImage = ((clPicBoard)sender).BackgroundImage; //Rotate SelectedCard
                    //סיבוב ערכי המטריצה של הקלף
                    ((clPicBoard)sender).RotateMatValues();//Rotate Sender PicBoard matValues
                    ((clPicture)this.Parent.Controls["SelectedCard"]).RotateVecValues();//Rotate SelectedCard vecValues
                }
            }

        }

        public void RotateMatValues()
        {
            int i, j;
            int[][] tempMat = new int[3][];
            //שמירת המטריצה במטריצה זמנית
            for (i = 0; i < 3; i++)
            {
                tempMat[i] = new int[3];
                for (j = 0; j < 3; j++)
                    tempMat[i][j] = matValues[i][j];
            }

            matValues[0][0] = tempMat[2][0];
            matValues[0][1] = tempMat[1][0];
            matValues[0][2] = tempMat[0][0];
            matValues[1][0] = tempMat[2][1];
            matValues[1][2] = tempMat[0][1];
            matValues[2][0] = tempMat[2][2];
            matValues[2][1] = tempMat[1][2];
            matValues[2][2] = tempMat[0][2];
        }

        Boolean checkRow(int myRow, int neighbourRow, int ine, int jne)
        {
            if (((clPicBoard)this.Parent.Controls[ine.ToString() + jne.ToString()]).BackgroundImage != null)
                return compareArrays(((clPicBoard)this.Parent.Controls[ine.ToString() + jne.ToString()]).getRow(neighbourRow), matValues[myRow]);
            else //empty picBoard
                return true;
        }

        Boolean checkCol(int myCol, int neighbourCol, int ine, int jne)
        {
            //Get MyCol Values
            int[] ColValues = new int[3];
            for (int i = 0; i < 3; i++)
                ColValues[i] = matValues[i][myCol];
            //get neighbourCol values and compare with myCol values
            if (((clPicBoard)this.Parent.Controls[ine.ToString() + jne.ToString()]).BackgroundImage != null)
                return compareArrays(((clPicBoard)this.Parent.Controls[ine.ToString() + jne.ToString()]).getCol(neighbourCol), ColValues);
            else //empty picBoard
                return true;
        }

        int[] getRow(int row)
        {
            return matValues[row];
        }

        int[] getCol(int col)
        {
            int[] vec = new int[3];
            for (int i = 0; i < 3; i++)
                vec[i] = matValues[i][col];
            return vec;
        }

        int getDiag(int row, int col)
        {
            return matValues[row][col];
        }

        bool compareArrays(int[] vec1, int[] vec2)
        {
            for (int i = 0; i < 3; i++)
                if (vec1[i] != vec2[i])
                    return false;
            return true;

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //יצירת הבהוב לאחר מציאת רצף
            if (stopShow == 6)
            {
                this.BackgroundImage = null;
                stopShow = 0;
                t.Enabled = false;
                placeFlag = false;
                rotateFlag = true;
                clGlobal.boardStatus[Convert.ToInt32(this.Tag) / 9, Convert.ToInt32(this.Tag) % 9] = 0;
                myBackGround = null;
                //בדיקה אם הלוח ריק לגמרי -> הגרלת קלף אמצעי חדש
                if (IsBoardEmpty())
                {
                    randomMiddleCard();
                }
            }
            else
            {
                stopShow++;
                if (myBackGround == null)

                    myBackGround = this.BackgroundImage;
                if (this.BackgroundImage == null)
                    this.BackgroundImage = myBackGround;

                else
                    this.BackgroundImage = null;
            }
        }

        private Boolean IsBoardEmpty()
        {
            Boolean fullFlag = true;
            for (int k = 0; k < 9; k++)
                for (int l = 0; l < 9; l++)
                    if (clGlobal.boardStatus[k, l] == 1)
                        fullFlag = false;
            return fullFlag;
        }

        private void randomMiddleCard()
        {
            //board is empty -> random new middle card
            int r;
            Random rnd = new Random();
            if (checkAllCardsInUse() == true) //אין אפשרות להגריל קלף אמצעי חדש -> סיום משחק
                GameOver();
            else
            {
                //random card that wasn't used
                do
                {
                    r = rnd.Next(clGlobal.cardsNum);
                } while (clGlobal.usedCards[r] == true);

                ((clPicBoard)this.Parent.Controls["44"]).BackgroundImage = Image.FromFile("..\\..\\cards\\card (" + r + ").jpg");
                clGlobal.usedCards[r] = true;
                ((clPicBoard)this.Parent.Controls["44"]).saveCardValues(r);
                ((clPicBoard)this.Parent.Controls["44"]).rotateFlag = false;
                clGlobal.boardStatus[4, 4] = 1;
            }

        }

        public bool checkAllCardsInUse()
        {
            for (int i = 0; i < clGlobal.cardsNum; i++)
                if (clGlobal.usedCards[i] == false)
                    return false; // נמצא כרטיס שאינו בשימוש

            return true; //כל הכרטיסים בשימוש - סיום משחק
        }

        private void GameOver()
        {
            ((frmMainScreen)Parent).Hide();
            if (clGlobal.computerScores > clGlobal.playerScores) //Computer won
            {
                frmGameOver GameOver = new frmGameOver("fail.jpg");
                GameOver.Show();
            }
            else
            {
                frmGameOver GameOver = new frmGameOver("win.jpg");
                GameOver.Show();
            }
        }



    }
}

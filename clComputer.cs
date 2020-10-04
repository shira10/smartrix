using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Smartrix
{
    public delegate void dlAddtoPanelCom(int x);
    public delegate void dlAnotherAddtoPanelCom();
    public delegate void dlPnlColor();

    struct checkPoint
    {
        public int i;
        public int j;
        public int lenght;
        public int tzover;
        public int numOfcard;
        public int numOfRotate;

        private int playerMaxLength;

        public int PlayerMaxLength
        {
            get { return playerMaxLength; }
            set { playerMaxLength = value; }
        }
    }

    class clComputer
    {
        public dlAddtoPanelCom dl;
        public dlAnotherAddtoPanelCom dlA;
        public dlPnlColor dlPnlColor;


        clPicBoard[,] board;
        clPicture[] computerCards;
        clPicture[] playerCards;
        List<checkPoint> listCheck = new List<checkPoint>();
        List<checkPoint> listPlayer = new List<checkPoint>();
        List<checkPoint> listCompNext = new List<checkPoint>();

        public clComputer(clPicBoard[,] board, clPicture[] computerCards, clPicture[] playerCards)
        {
            copyData(board, computerCards, playerCards);
        }

        public void copyData(clPicBoard[,] board, clPicture[] computerCards, clPicture[] playerCards)
        {
            this.board = board;
            this.computerCards = computerCards;
            this.playerCards = playerCards;
        }


        public void checkStatus()
        {   // ריצה על כל השכנים של הכרטיסים המופיעים על הלוח תוך בדיקה איזה רצף יצור כל אחד

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    if (clGlobal.boardStatus[i, j] == 1)
                    {
                        checkSequence(i, j - 1, listCheck);
                        checkSequence(i, j + 1, listCheck);
                        checkSequence(i - 1, j, listCheck);
                        checkSequence(i + 1, j, listCheck);
                        checkSequence(i + 1, j - 1, listCheck);
                        checkSequence(i + 1, j + 1, listCheck);
                        checkSequence(i - 1, j - 1, listCheck);
                        checkSequence(i - 1, j + 1, listCheck);
                    }
                }

            checkPoint temp = new checkPoint();
            temp.tzover = 0;
            temp.PlayerMaxLength = 5;
            // ריצה על הנחת הכרטיסים על האורך המקסימלי
            while (searchPlaceForTheMaxLength(ref temp, false) != 2 && listCheck.Count != 0) ;

        }

        private int searchPlaceForTheMaxLength(ref checkPoint temp, Boolean notFoundGoodLength)
        {
            //מציאת האורך המקסימלי ברשימת המשבצות הנתונה
            int maxLeng = 0;
            foreach (var l in listCheck)
                if (l.lenght > maxLeng)
                    maxLeng = l.lenght;
            if (maxLeng == 0) //אין קלפים להניח -> הוספת קלף לפאנל
                dlA();

            //ריצה על המשבצות הנותנות את האורך הארוך
            for (int i = 0; i < listCheck.Count; )

                if (listCheck[i].lenght == maxLeng)
                {
                    listCheck[i] = checkAllNeighboursForAllList(listCheck[i], -1); //חיפוש קלף מתאים למיקום הנוכחי מקלפי המחשב
                    if (listCheck[i].numOfcard == -1) // לא נמצא קלף מתאים למיקום הנוכחי
                        listCheck.Remove(listCheck[i]);
                    else
                    {
                        //בדיקה מה יהיה המהלך הבא של השחקן - מהו האורך המקסימלי שיכול ליצור אם יונח הקלף הזה
                        clGlobal.boardStatus[listCheck[i].i, listCheck[i].j] = 1; //סימון המקום כתפוס-מצב הלוח אם יתבצע מהלך זה
                        checkPoint res = checkPlayerStatus(); //אורך מהלך השחקן

                        if (res.lenght < 5)
                        {
                            //אם האורך המקסימלי 4 ולשחקן אין להשלים ל-5 -> בדיקה אם למחשב יהיה להשלים ל-5 במהלך הבא
                            //אם כן -יונח, אם לא - לא יונח
                            if (checkFourInLine(res.i, res.j) == true)
                            {
                                bool compNext = CompNextMovementLength(listCheck[i].numOfcard); //בדיקה אם המחשב יוכל להשלים רצף במהלך הבא
                                clGlobal.boardStatus[listCheck[i].i, listCheck[i].j] = 0;
                                if (compNext == true)
                                {
                                    if (listCheck[i].tzover > temp.tzover)
                                    {
                                        temp = listCheck[i];
                                        temp.PlayerMaxLength = res.lenght;
                                    }
                                }
                                else
                                    listCheck.Remove(listCheck[i]);                                
                            }

                            else
                            {
                                if (res.lenght < temp.PlayerMaxLength) //בדיקה אם הניקוד המצטבר של הקלף הנוכחי גבוה יותר
                                {
                                    temp = listCheck[i];
                                    temp.PlayerMaxLength = res.lenght;
                                }
                                else if (res.lenght == temp.PlayerMaxLength)
                                    if (listCheck[i].tzover > temp.tzover)
                                    {
                                        temp = listCheck[i];
                                        temp.PlayerMaxLength = res.lenght;
                                    }
                                clGlobal.boardStatus[listCheck[i].i, listCheck[i].j] = 0;
                            }


                        }
                        else //res==5
                        {
                            clGlobal.boardStatus[listCheck[i].i, listCheck[i].j] = 0;
                            listCheck.Remove(listCheck[i]);
                            i--; //חזרה לאותו מקום ברשימה
                        }

                        i++;
                    }
                }
                else
                    i++;
            //הנחת הכרטיס
            //if (temp.tzover > 0)
            if (temp.lenght > 0)
            {
                board[temp.i, temp.j].placeFlag = true; //סימון המקום כתפוס
                board[temp.i, temp.j].rotateFlag = false; //סימון המקום כלא ניתן לסיבוב
                clGlobal.boardStatus[temp.i, temp.j] = 1;
                //הבהוב והורדת הכרטיסים בעת יצירת חמישיה
                Image b = computerCards[temp.numOfcard].BackgroundImage;

                board[temp.i, temp.j].saveCardValues(computerCards[temp.numOfcard].rowInSavepic);

                if (temp.numOfRotate != 0)
                {
                    for (int k = 0; k < temp.numOfRotate; k++)
                    {
                        board[temp.i, temp.j].RotateMatValues();
                        b = utilities.RotateImage(b, 90); //Rotate Sender PicBoard  
                    }
                    temp.numOfRotate = 0;
                }
                board[temp.i, temp.j].BackgroundImage = b;

                dlPnlColor();

                if (maxLeng == 5)
                    board[temp.i, temp.j].checkSequence(temp.i, temp.j, 1);
                dl(temp.numOfcard);

                //צבירת נקודות של הרצף שנוצר
                //getComputerScores(temp.i, temp.j);

                listCheck.Clear();
            }
            else
            {
                if (listCheck.Count == 0) //אין קלפים להניח -> הוספת קלף לפאנל
                    dlA();
            }

            return (maxLeng);
        }

        private bool checkFourInLine(int x, int y)
        {
            int iStart = 0, jStart = 0;
            int cntRow, cntCol, cntMainDiag, cntSecondDiag;
            int row, col;
            bool flag;

            clGlobal.boardStatus[x, y] = 1; //סימון המקום של מהלך השחקן כתפוס

            //מציאת השורה של מקום תחילת הלוח 
            for (row = 0, flag = false; row < 9 && !flag; row++)
                for (col = 0; col < 9 && !flag; col++)
                    if (clGlobal.boardStatus[row, col] == 1)
                    {
                        iStart = row;
                        flag = true;
                    }
            //מציאת העמודה של מקום תחילת הלוח
            for (col = 0, flag = false; col < 9 && !flag; col++)
                for (row = 0; row < 9 && !flag; row++)
                    if (clGlobal.boardStatus[row, col] == 1)
                    {
                        jStart = col;
                        flag = true;
                    }

            for (int i = iStart; i < iStart + 5; i++)
            {
                cntRow = cntCol = cntMainDiag = cntSecondDiag = 0;
                for (int j = jStart; j < jStart + 5; j++)
                {
                    try
                    {
                        if (clGlobal.boardStatus[i, j] == 1)
                            cntRow++;
                    }
                    catch { };
                    try
                    {
                        if (clGlobal.boardStatus[j, i] == 1)
                            cntCol++;
                    }
                    catch { };
                    if (i == j)
                        try
                        {
                            if (clGlobal.boardStatus[i, j] == 1)
                                cntMainDiag++;
                        }
                        catch { };
                    if (i + j == 4)
                        try
                        {
                            if (clGlobal.boardStatus[i, j] == 1)
                                cntSecondDiag++;
                        }
                        catch { };
                }
                if (cntRow == 4 || cntCol == 4 || cntMainDiag == 4 || cntSecondDiag == 4)
                {
                    clGlobal.boardStatus[x, y] = 0; //סימון המקום של מהלך השחקן כפנוי
                    return (true);
                }

            }
            clGlobal.boardStatus[x, y] = 0; //סימון המקום של מהלך השחקן כפנוי
            return (false);
        }

        public checkPoint checkAllNeighboursForAllList(checkPoint l, int cardNum)
        {
            //ריצה על הקלפים של המחשב 
            //הפונקציה מחזירה את הקלף הכי מתאים למקום 

            //cardNum = הוא מספר הכרטיס בפאנל הכרטיסים של המחשב שאיננו רוצים לבדוק, כיוון שזהו הקלף שיונח במהלך זה
            //cardNum = -1 -> אם רוצים לבדוק את כל הקלפים ערכו 

            int j = 0;
            int max = 0;
            int maxi = -1;
            int rotate = 0;
            int maxr = 0;
            for (int i = 0; i < clGlobal.computerCardsNum; i++)
            {
                if (cardNum != i)
                {
                    //l.numOfRotate = 0;
                    board[l.i, l.j].saveCardValues(computerCards[i].rowInSavepic);
                    if (board[l.i, l.j].checkBorder(l.i, l.j) == true)
                    {
                        for (j = 0, rotate = 0; j < 3 && board[l.i, l.j].checkAllNeighbours(l.i, l.j, board[l.i, l.j].matValues) == false; rotate++, j++)
                            board[l.i, l.j].RotateMatValues();
                        if (board[l.i, l.j].checkAllNeighbours(l.i, l.j, board[l.i, l.j].matValues) == true)
                            if (board[l.i, l.j].matValues[1][1] > max)//כאן צריך להוסיף בדיקה לגבי הכרטיס האפס
                            {
                                max = board[l.i, l.j].matValues[1][1];
                                maxi = i;
                                maxr = rotate;
                            }
                    }
                }
            }
            //לא נמצא קלף מתאים למקום
            if (maxi == -1)
                l.numOfcard = -1;


            l.tzover += max;
            l.numOfcard = maxi;
            l.numOfRotate = maxr;

            return (l);
            //    board[l.i, l.j].BackgroundImage = computerCards[l.numOfcard].BackgroundImage;

        }

        private void checkSequence(int iMatrix, int jMatrix, List<checkPoint> l)
        {
            try
            {
                List<checkPoint> listTemp = new List<checkPoint>();
                if (clGlobal.boardStatus[iMatrix, jMatrix] == 0)
                {
                    int cnt, tzover;
                    int i, j;

                    //--- Check row sequence --- 
                    cnt = 1; tzover = 0;
                    //right
                    for (j = jMatrix + 1; j < 9 && clGlobal.boardStatus[iMatrix, j] == 1; j++)
                    {
                        cnt++;
                        tzover += board[iMatrix, j].matValues[1][1];
                    }
                    //left
                    for (j = jMatrix - 1; j >= 0 && clGlobal.boardStatus[iMatrix, j] == 1; j--)
                    {
                        cnt++;
                        tzover += board[iMatrix, j].matValues[1][1];
                    }

                    //הוספה לליסט את המשבצת שנבדקה
                    if (cnt != 1)
                    {
                        addToList(iMatrix, jMatrix, cnt, tzover, listTemp);
                    }

                    //--- Check column sequence --- 
                    cnt = 1; tzover = 0;

                    //up
                    for (i = iMatrix + 1; i < 9 && clGlobal.boardStatus[i, jMatrix] == 1; i++)
                    {
                        cnt++;
                        tzover += board[i, jMatrix].matValues[1][1];

                    }
                    //down
                    for (i = iMatrix - 1; i >= 0 && clGlobal.boardStatus[i, jMatrix] == 1; i--)
                    {
                        cnt++;
                        tzover += board[i, jMatrix].matValues[1][1];

                    }
                    if (cnt != 1) //נמצא רצף עמודה
                    {
                        addToList(iMatrix, jMatrix, cnt, tzover, listTemp);
                    }
                    //--- Check MainDiagonal sequence --- 
                    cnt = 1; tzover = 0;

                    //down
                    for (i = iMatrix + 1, j = jMatrix + 1; i < 9 && j < 9 && clGlobal.boardStatus[i, j] == 1; i++, j++)
                    {
                        cnt++;
                        tzover += board[i, j].matValues[1][1];

                    }
                    //up
                    for (i = iMatrix - 1, j = jMatrix - 1; i >= 0 && j >= 0 && clGlobal.boardStatus[i, j] == 1; i--, j--)
                    {
                        cnt++;
                        tzover += board[i, j].matValues[1][1];

                    }
                    if (cnt != 1) //נמצא רצף אלכסון ראשי
                    {
                        addToList(iMatrix, jMatrix, cnt, tzover, listTemp);
                    }

                    //--- Check SecondDiagonal sequence --- 
                    cnt = 1; tzover = 0;

                    //up
                    for (i = iMatrix - 1, j = jMatrix + 1; i >= 0 && j < 9 && clGlobal.boardStatus[i, j] == 1; i--, j++)
                    {
                        cnt++;
                        tzover += board[i, j].matValues[1][1];

                    }
                    //down
                    for (i = iMatrix + 1, j = jMatrix - 1; i < 9 && j >= 0 && clGlobal.boardStatus[i, j] == 1; i++, j--)
                    {
                        cnt++;
                        tzover += board[i, j].matValues[1][1];

                    }
                    if (cnt != 1) //נמצא רצף אלכסון משני
                    {
                        addToList(iMatrix, jMatrix, cnt, tzover, listTemp);
                    }
                    int sum = 0;
                    int maxLen = 0;
                    foreach (var c in listTemp)
                    {
                        sum += c.tzover;
                        if (c.lenght > maxLen)
                            maxLen = c.lenght;
                    }
                    addToList(iMatrix, jMatrix, maxLen, sum, l);

                }

            }
            catch { }
        }

        private void addToList(int iMatrix, int jMatrix, int cnt, int tzover, List<checkPoint> listTemp)
        {
            checkPoint c = new checkPoint();
            c.i = iMatrix;
            c.j = jMatrix;
            c.lenght = cnt;
            c.tzover = tzover;

            listTemp.Add(c);
        }

        public bool CompNextMovementLength(int numOfCard)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    if (clGlobal.boardStatus[i, j] == 1)
                    {
                        checkSequence(i, j - 1, listCompNext);
                        checkSequence(i, j + 1, listCompNext);
                        checkSequence(i - 1, j, listCompNext);
                        checkSequence(i + 1, j, listCompNext);
                        checkSequence(i + 1, j - 1, listCompNext);
                        checkSequence(i + 1, j + 1, listCompNext);
                        checkSequence(i - 1, j - 1, listCompNext);
                        checkSequence(i - 1, j + 1, listCompNext);
                    }
                }

            for (int i = 0; i < listCompNext.Count; i++)

                if (listCompNext[i].lenght == 5)
                {
                    listCompNext[i] = checkAllNeighboursForAllList(listCompNext[i], numOfCard); //חיפוש קלף מתאים למיקום הנוכחי מקלפי המחשב
                    if (listCompNext[i].numOfcard != -1) // נמצא קלף מתאים למיקום הנוכחי
                    {
                        listCompNext.Clear();
                        return (true); //נמצא קלף להשלים את הרצף במהלך הבא של המחשב
                    }
                }


            listCompNext.Clear();
            return (false); //לא נמצא
        }


        public checkPoint checkPlayerStatus()
        {   // ריצה על כל השכנים של הכרטיסים המופיעים על הלוח תוך בדיקה איזה רצף יצור כל אחד             
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    if (clGlobal.boardStatus[i, j] == 1)
                    {
                        checkSequence(i, j - 1, listPlayer);
                        checkSequence(i, j + 1, listPlayer);
                        checkSequence(i - 1, j, listPlayer);
                        checkSequence(i + 1, j, listPlayer);
                        checkSequence(i + 1, j - 1, listPlayer);
                        checkSequence(i + 1, j + 1, listPlayer);
                        checkSequence(i - 1, j - 1, listPlayer);
                        checkSequence(i - 1, j + 1, listPlayer);
                    }
                }

            checkPoint temp = new checkPoint();
            temp.tzover = 0;
            temp.lenght = 0;
            //  (!=4)ריצה על הנחת הכרטיסים על האורך המקסימלי
            while (searchPlacePlayer(ref temp, false) != 2 && listPlayer.Count != 0) ;
            //במקרה שלא מצא להניח שום דבר שינסה לחפש רצף של ארבע
            if (listPlayer.Count != 0)
                searchPlacePlayer(ref temp, true);

            return (temp);
        }

        private int searchPlacePlayer(ref checkPoint temp, Boolean notFoundGoodLength)
        {
            //מציאת האורך המקסימלי ברשימת המשבצות הנתונה
            int maxLeng = 0;
            foreach (var l in listPlayer)
                if (l.lenght > maxLeng && ((l.lenght != 4 && notFoundGoodLength == false) || notFoundGoodLength == true))
                    maxLeng = l.lenght;
            if (maxLeng == 0)
                maxLeng = 4;
            //ריצה על המשבצות הנותנות את האורך הארוך
            for (int i = 0; i < listPlayer.Count; )

                if (listPlayer[i].lenght == maxLeng)
                {
                    listPlayer[i] = checkAllNeighboursForAllPlayerList(listPlayer[i]); //חיפוש קלף מתאים למיקום הנוכחי מקלפי המחשב
                    if (listPlayer[i].numOfcard == -1) // לא נמצא קלף מתאים למיקום הנוכחי
                        listPlayer.Remove(listPlayer[i]);
                    else
                    {
                        if (listPlayer[i].tzover > temp.tzover) //בדיקה אם הניקוד המצטבר של הקלף הנוכחי גבוה יותר
                            temp = listPlayer[i];
                        i++;
                    }
                }
                else
                    i++;
            if (temp.lenght != 0)
                listPlayer.Clear();
            return (temp.lenght); //החזרת האורך הארוך ביותר שיכול השקחן ליצור אם יונח הקלף
        }

        public checkPoint checkAllNeighboursForAllPlayerList(checkPoint l)
        {
            //ריצה על הקלפים של המחשב 
            //הפונקציה מחזירה את הקלף הכי מתאים למקום 
            int j = 0;
            int max = 0;
            int maxi = -1;
            int rotate = 0;
            int maxr = 0;
            for (int i = 0; i < clGlobal.playerCardsNum; i++)
            {
                //l.numOfRotate = 0;
                board[l.i, l.j].saveCardValues(playerCards[i].rowInSavepic);

                if (board[l.i, l.j].checkBorder(l.i, l.j) == true)
                {
                    for (j = 0, rotate = 0; j < 3 && board[l.i, l.j].checkAllNeighbours(l.i, l.j, board[l.i, l.j].matValues) == false; rotate++, j++)
                        board[l.i, l.j].RotateMatValues();
                    if (j < 3)
                        if (board[l.i, l.j].matValues[1][1] > max)//כאן צריך להוסיף בדיקה לגבי הכרטיס האפס
                        {
                            max = board[l.i, l.j].matValues[1][1];
                            maxi = i;
                            maxr = rotate;
                        }
                }
            }
            //לא נמצא קלף מתאים למקום
            if (maxi == -1)
                l.numOfcard = -1;

            l.tzover += max;
            l.numOfcard = maxi;
            l.numOfRotate = maxr;

            return (l);
            //    board[l.i, l.j].BackgroundImage = computerCards[l.numOfcard].BackgroundImage;

        }



    }
}
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

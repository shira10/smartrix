using System;
using System.Collections.Generic;
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
        
        bool compareArrays(int[] vec1, int[] vec2)
        {
            for (int i = 0; i < 3; i++)
                if (vec1[i] != vec2[i])
                    return false;
            return true;

        }

    }
}

clspicboard

public bool checkAllCardsInUse()
        {
            for (int i = 0; i < clGlobal.cardsNum; i++)
                if (clGlobal.usedCards[i] == false)
                    return false; // נמצא כרטיס שאינו בשימוש

            return true; //כל הכרטיסים בשימוש - סיום משחק
        }




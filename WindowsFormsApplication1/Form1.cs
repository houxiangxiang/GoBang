using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace WindowsFormsApplication1
{
    enum EGAMEMODE { eMANVSPC, eCHESSNOTATION }; 
    public partial class FormMain : Form
    {
        public static readonly int ChessBoardScale = 15;    // scale of chess board
        private const int GRIDSIZE = 32;                    // size of grid containing a chessman
        private const int CHESSMANSIZE = 28;                // chessman size
        private const int X_START = 30;                     // ?
        private const int Y_START = 32;                     // ?

        bool bPresentHistory = false;                       // need present step history?

        private int iMaxRepeatNum = 30;                // how many repeats are allowed;

        private int iPlayNum = 0;                           // number of steps
        private int iHour = 0;                              // elapsed hour
        private int iMinute = 0;                            // elapsed minute
        private int iSecond = 0;                            // elapsed second

        private int iElpasedAfterEnd;                       // how many seconds elapsed after game ends

        private int iHour_Mine = 0;                              // elapsed hour
        private int iMinute_Mine = 0;                            // elapsed minute
        private int iSecond_Mine = 0;                            // elapsed second

        private float[] rowPosition = new float[ChessBoardScale];   // store position of every row on chessman board
        private float[] colPosition = new float[ChessBoardScale];   // store position of every col on chessman board

        float chessManSemiDiameter = 0;                     // semi diameter of a chessman
        float chessManDiameter = 0;                         // diameter of a chessman

        GoBang goBang;                                      // instance of GoBang class. the structure of this program contains 2 levels:
                                                            // 1. GUI, which present chessman board and chessman to user
                                                            // 2. GoBang, which store and compute data

        private int mySide = -1;  // computer's side == 1, means computer takes black; side == -1, means computer takes white
        private int peerSide = 1; // computer's peer side, == 1, means component takes black, side == -1, means component taks white
        private bool running = false;                       // whether game is started
        private int style = 0;                              // style of game, i.e., aggressive, balanced, or defensive
        private int level = 0;                              // level of game, i.e., easy, normal, or difficult
        private int forbidRule = 1;                         // 0 - no forbidden rule, 1 - have forbidden rule
        private PictureBox[,] ChessMan = new PictureBox[15,15]; // containing the graph of chessman which is present on the board
        private Label[,] LabelGameRecord = new Label[15, 15];   // record of the game 

        private List<PairStepInfo> historySteps;            // steps
        private List<PairStepInfo> removedsteps;            // retracted steps

        private List<StepInfo> historySteps_ChessNotation; // steps used by chess-notation mode

        private int preX_Comp = -1, preY_Comp = -1;         // component's last step, used to change chessman's image later
        private int preX_Self = -1, preY_Self = -1;         // self (computer)'s last step, used to change chessman's image later

        private Point myNextPosition;                       // the next position I should take ( I am computer )
        private int compIndexX, compIndexY;                 // index of X & Y of my component ( I am computer )
        private bool bIAmThinking;                          // I am thinking and don't accept input ( I am computer )

        private int iMaxProbeSteps = 6;                     // max probe steps set to strategy

        private int iPeerIndex = 0;
        private int iPeerNum = 2;

        EGAMEMODE eGameMode = EGAMEMODE.eMANVSPC;
        int iSide_ChessNotation = 1;                        // which side's order to put a chessman. used by chess-notation mode.

        private class ConfigItems
        {
            public EGAMEMODE iMode;
            public bool bTakeBlack;
            public bool bShowHistory;
            public int iLevel;
            public bool bUseRule;
            public int iConponent;
        };
        private ConfigItems configItems = new ConfigItems();

        // construct function
        //=====================
        public FormMain()
        {
            InitializeComponent();

            LogInfo.getInstance().WriteLogFlush("---------------");
            LogInfo.getInstance().WriteLogFlush("Program starts");
            LogInfo.getInstance().WriteLogFlush("---------------");

            // calculate raw & col position
            for (int i = 0; i < ChessBoardScale; i++)
            {
                rowPosition[i] = panelChessBorad.Height * (i + 1) / (ChessBoardScale + 1);
                colPosition[i] = panelChessBorad.Width * (i + 1) / (ChessBoardScale + 1);
            }

            chessManSemiDiameter = panelChessBorad.Height / (ChessBoardScale + 1) / 3;
            chessManDiameter = chessManSemiDiameter * 2;
            applyRuleMenuItem.Checked = true;
            historySteps = new List<PairStepInfo>();
            removedsteps = new List<PairStepInfo>();
            historySteps_ChessNotation = new List<StepInfo>();
            buttonRegret.Enabled = false;
            buttonRegret.Text = "悔棋 （" + iMaxRepeatNum  + "）";

            goBang = new GoBang(/*this*/);
            
            
            // create 2 dimension array of Picture Box. used to present chessman.
            ChessMan[0, 0]  = pbCM0;  ChessMan[0, 1]  = pbCM1;  ChessMan[0, 2]  = pbCM2;  ChessMan[0, 3]  = pbCM3;  ChessMan[0, 4]  = pbCM4;
            ChessMan[0, 5]  = pbCM5;  ChessMan[0, 6]  = pbCM6;  ChessMan[0, 7]  = pbCM7;  ChessMan[0, 8]  = pbCM8;  ChessMan[0, 9]  = pbCM9;
            ChessMan[0, 10] = pbCM10; ChessMan[0, 11] = pbCM11; ChessMan[0, 12] = pbCM12; ChessMan[0, 13] = pbCM13; ChessMan[0, 14] = pbCM14;

            ChessMan[1, 0]  = pbCM15; ChessMan[1, 1]  = pbCM16; ChessMan[1, 2]  = pbCM17; ChessMan[1, 3]  = pbCM18; ChessMan[1, 4]  = pbCM19;
            ChessMan[1, 5]  = pbCM20; ChessMan[1, 6]  = pbCM21; ChessMan[1, 7]  = pbCM22; ChessMan[1, 8]  = pbCM23; ChessMan[1, 9]  = pbCM24;
            ChessMan[1, 10] = pbCM25; ChessMan[1, 11] = pbCM26; ChessMan[1, 12] = pbCM27; ChessMan[1, 13] = pbCM28; ChessMan[1, 14] = pbCM29;

            ChessMan[2, 0]  = pbCM30; ChessMan[2, 1]  = pbCM31; ChessMan[2, 2]  = pbCM32; ChessMan[2, 3]  = pbCM33; ChessMan[2, 4]  = pbCM34;
            ChessMan[2, 5]  = pbCM35; ChessMan[2, 6]  = pbCM36; ChessMan[2, 7]  = pbCM37; ChessMan[2, 8]  = pbCM38; ChessMan[2, 9]  = pbCM39;
            ChessMan[2, 10] = pbCM40; ChessMan[2, 11] = pbCM41; ChessMan[2, 12] = pbCM42; ChessMan[2, 13] = pbCM43; ChessMan[2, 14] = pbCM44;

            ChessMan[3, 0]  = pbCM45; ChessMan[3, 1]  = pbCM46; ChessMan[3, 2]  = pbCM47; ChessMan[3, 3]  = pbCM48; ChessMan[3, 4]  = pbCM49;
            ChessMan[3, 5]  = pbCM50; ChessMan[3, 6]  = pbCM51; ChessMan[3, 7]  = pbCM52; ChessMan[3, 8]  = pbCM53; ChessMan[3, 9]  = pbCM54;
            ChessMan[3, 10] = pbCM55; ChessMan[3, 11] = pbCM56; ChessMan[3, 12] = pbCM57; ChessMan[3, 13] = pbCM58; ChessMan[3, 14] = pbCM59;

            ChessMan[4, 0]  = pbCM60; ChessMan[4, 1]  = pbCM61; ChessMan[4, 2]  = pbCM62; ChessMan[4, 3]  = pbCM63; ChessMan[4, 4]  = pbCM64;
            ChessMan[4, 5]  = pbCM65; ChessMan[4, 6]  = pbCM66; ChessMan[4, 7]  = pbCM67; ChessMan[4, 8]  = pbCM68; ChessMan[4, 9]  = pbCM69;
            ChessMan[4, 10] = pbCM70; ChessMan[4, 11] = pbCM71; ChessMan[4, 12] = pbCM72; ChessMan[4, 13] = pbCM73; ChessMan[4, 14] = pbCM74;

            ChessMan[5, 0]  = pbCM75; ChessMan[5, 1]  = pbCM76; ChessMan[5, 2]  = pbCM77; ChessMan[5, 3]  = pbCM78; ChessMan[5, 4]  = pbCM79;
            ChessMan[5, 5]  = pbCM80; ChessMan[5, 6]  = pbCM81; ChessMan[5, 7]  = pbCM82; ChessMan[5, 8]  = pbCM83; ChessMan[5, 9]  = pbCM84;
            ChessMan[5, 10] = pbCM85; ChessMan[5, 11] = pbCM86; ChessMan[5, 12] = pbCM87; ChessMan[5, 13] = pbCM88; ChessMan[5, 14] = pbCM89;

            ChessMan[6, 0]  = pbCM90;  ChessMan[6, 1]  = pbCM91;  ChessMan[6, 2]  = pbCM92;  ChessMan[6, 3]  = pbCM93;  ChessMan[6, 4]  = pbCM94;
            ChessMan[6, 5]  = pbCM95;  ChessMan[6, 6]  = pbCM96;  ChessMan[6, 7]  = pbCM97;  ChessMan[6, 8]  = pbCM98;  ChessMan[6, 9]  = pbCM99;
            ChessMan[6, 10] = pbCM100; ChessMan[6, 11] = pbCM101; ChessMan[6, 12] = pbCM102; ChessMan[6, 13] = pbCM103; ChessMan[6, 14] = pbCM104;

            ChessMan[7, 0]  = pbCM105; ChessMan[7, 1]  = pbCM106; ChessMan[7, 2]  = pbCM107; ChessMan[7, 3]  = pbCM108; ChessMan[7, 4]  = pbCM109;
            ChessMan[7, 5]  = pbCM110; ChessMan[7, 6]  = pbCM111; ChessMan[7, 7]  = pbCM112; ChessMan[7, 8]  = pbCM113; ChessMan[7, 9]  = pbCM114;
            ChessMan[7, 10] = pbCM115; ChessMan[7, 11] = pbCM116; ChessMan[7, 12] = pbCM117; ChessMan[7, 13] = pbCM118; ChessMan[7, 14] = pbCM119;

            ChessMan[8, 0]  = pbCM120; ChessMan[8, 1]  = pbCM121; ChessMan[8, 2]  = pbCM122; ChessMan[8, 3]  = pbCM123; ChessMan[8, 4]  = pbCM124;
            ChessMan[8, 5]  = pbCM125; ChessMan[8, 6]  = pbCM126; ChessMan[8, 7]  = pbCM127; ChessMan[8, 8]  = pbCM128; ChessMan[8, 9]  = pbCM129;
            ChessMan[8, 10] = pbCM130; ChessMan[8, 11] = pbCM131; ChessMan[8, 12] = pbCM132; ChessMan[8, 13] = pbCM133; ChessMan[8, 14] = pbCM134;

            ChessMan[9, 0]  = pbCM135; ChessMan[9, 1]  = pbCM136; ChessMan[9, 2]  = pbCM137; ChessMan[9, 3]  = pbCM138; ChessMan[9, 4]  = pbCM139;
            ChessMan[9, 5]  = pbCM140; ChessMan[9, 6]  = pbCM141; ChessMan[9, 7]  = pbCM142; ChessMan[9, 8]  = pbCM143; ChessMan[9, 9]  = pbCM144;
            ChessMan[9, 10] = pbCM145; ChessMan[9, 11] = pbCM146; ChessMan[9, 12] = pbCM147; ChessMan[9, 13] = pbCM148; ChessMan[9, 14] = pbCM149;

            ChessMan[10, 0]  = pbCM150; ChessMan[10, 1]  = pbCM151; ChessMan[10, 2]  = pbCM152; ChessMan[10, 3]  = pbCM153; ChessMan[10, 4]  = pbCM154;
            ChessMan[10, 5]  = pbCM155; ChessMan[10, 6]  = pbCM156; ChessMan[10, 7]  = pbCM157; ChessMan[10, 8]  = pbCM158; ChessMan[10, 9]  = pbCM159;
            ChessMan[10, 10] = pbCM160; ChessMan[10, 11] = pbCM161; ChessMan[10, 12] = pbCM162; ChessMan[10, 13] = pbCM163; ChessMan[10, 14] = pbCM164;

            ChessMan[11, 0]  = pbCM165; ChessMan[11, 1]  = pbCM166; ChessMan[11, 2]  = pbCM167; ChessMan[11, 3]  = pbCM168; ChessMan[11, 4]  = pbCM169;
            ChessMan[11, 5]  = pbCM170; ChessMan[11, 6]  = pbCM171; ChessMan[11, 7]  = pbCM172; ChessMan[11, 8]  = pbCM173; ChessMan[11, 9]  = pbCM174;
            ChessMan[11, 10] = pbCM175; ChessMan[11, 11] = pbCM176; ChessMan[11, 12] = pbCM177; ChessMan[11, 13] = pbCM178; ChessMan[11, 14] = pbCM179;

            ChessMan[12, 0]  = pbCM180; ChessMan[12, 1]  = pbCM181; ChessMan[12, 2]  = pbCM182; ChessMan[12, 3]  = pbCM183; ChessMan[12, 4]  = pbCM184;
            ChessMan[12, 5]  = pbCM185; ChessMan[12, 6]  = pbCM186; ChessMan[12, 7]  = pbCM187; ChessMan[12, 8]  = pbCM188; ChessMan[12, 9]  = pbCM189;
            ChessMan[12, 10] = pbCM190; ChessMan[12, 11] = pbCM191; ChessMan[12, 12] = pbCM192; ChessMan[12, 13] = pbCM193; ChessMan[12, 14] = pbCM194;

            ChessMan[13, 0]  = pbCM195; ChessMan[13, 1]  = pbCM196; ChessMan[13, 2]  = pbCM197; ChessMan[13, 3]  = pbCM198; ChessMan[13, 4]  = pbCM199;
            ChessMan[13, 5]  = pbCM200; ChessMan[13, 6]  = pbCM201; ChessMan[13, 7]  = pbCM202; ChessMan[13, 8]  = pbCM203; ChessMan[13, 9]  = pbCM204;
            ChessMan[13, 10] = pbCM205; ChessMan[13, 11] = pbCM206; ChessMan[13, 12] = pbCM207; ChessMan[13, 13] = pbCM208; ChessMan[13, 14] = pbCM209;

            ChessMan[14, 0]  = pbCM210; ChessMan[14, 1]  = pbCM211; ChessMan[14, 2]  = pbCM212; ChessMan[14, 3]  = pbCM213; ChessMan[14, 4]  = pbCM214;
            ChessMan[14, 5]  = pbCM215; ChessMan[14, 6]  = pbCM216; ChessMan[14, 7]  = pbCM217; ChessMan[14, 8]  = pbCM218; ChessMan[14, 9]  = pbCM219;
            ChessMan[14, 10] = pbCM220; ChessMan[14, 11] = pbCM221; ChessMan[14, 12] = pbCM222; ChessMan[14, 13] = pbCM223; ChessMan[14, 14] = pbCM224;

            for (int i = 0; i < ChessBoardScale; i++)
            {
                for (int j = 0; j < ChessBoardScale; j++)
                {
                    LabelGameRecord[i, j] = new Label();
                    LabelGameRecord[i, j].Visible = false;
                    LabelGameRecord[i, j].AutoSize = false;
                    LabelGameRecord[i, j].Size = ChessMan[i, j].Size;
                    LabelGameRecord[i, j].Location = ChessMan[i, j].Location;
                    LabelGameRecord[i, j].BackColor = Color.Transparent;
                    this.panelChessBorad.Controls.Add(LabelGameRecord[i, j]);
                }
            }

            loadConfiguration();
        }

        private void applyConfiguration()
        {
            if (configItems.bShowHistory)
            {
                bPresentHistory = true;
                showHistoryToolStripMenuItem.Checked = true;
            }
            else
            {
                bPresentHistory = false;
                showHistoryToolStripMenuItem.Checked = false;
            }

            if (configItems.bTakeBlack)
            {
                takeBlackToolStripMenuItem1.Checked = true;
                pbChessman_up.Image = ilChessmanBoxes.Images[1];
                pbChessman_down.Image = ilChessmanBoxes.Images[0];
            }
            else
            {
                takeBlackToolStripMenuItem1.Checked = false;
                pbChessman_up.Image = ilChessmanBoxes.Images[0];
                pbChessman_down.Image = ilChessmanBoxes.Images[1];
            }

            if (configItems.bUseRule)
            {

            }
            else
            {

            }

            if (configItems.iConponent == 1)
            {
                Item1ItemMenu.Checked = true;
                Item2MenuItem.Checked = false;
                notSelectMenuItem.Checked = false;
                iPeerIndex = 0;
            }
            else if (configItems.iConponent == 2)
            {
                Item1ItemMenu.Checked = false;
                Item2MenuItem.Checked = true;
                notSelectMenuItem.Checked = false;
                iPeerIndex = 1;
            }
            else
            {
                Item1ItemMenu.Checked = false;
                Item2MenuItem.Checked = false;
                notSelectMenuItem.Checked = true;
                iPeerIndex = -1;
            }

            if (configItems.iLevel == 1)
            {
                level1ToolStripMenuItem.Checked = true;
                level2ToolStripMenuItem.Checked = false;
                level3ToolStripMenuItem.Checked = false;
                level4ToolStripMenuItem.Checked = false;
                iMaxProbeSteps = 4;
            }
            else if (configItems.iLevel == 2)
            {
                level1ToolStripMenuItem.Checked = false;
                level2ToolStripMenuItem.Checked = true;
                level3ToolStripMenuItem.Checked = false;
                level4ToolStripMenuItem.Checked = false;
                iMaxProbeSteps = 5;
            }
            else if (configItems.iLevel == 3)
            {
                level1ToolStripMenuItem.Checked = false;
                level2ToolStripMenuItem.Checked = false;
                level3ToolStripMenuItem.Checked = true;
                level4ToolStripMenuItem.Checked = false;
                iMaxProbeSteps = 6;
            }
            else if (configItems.iLevel == 4)
            {
                level1ToolStripMenuItem.Checked = false;
                level2ToolStripMenuItem.Checked = false;
                level3ToolStripMenuItem.Checked = false;
                level4ToolStripMenuItem.Checked = true;
                iMaxProbeSteps = 7;
            }

            if (configItems.iMode == EGAMEMODE.eMANVSPC)
            {
                manComputerToolStripMenuItem.Checked = true;
                chessNotationToolStripMenuItem.Checked = false;
                eGameMode = EGAMEMODE.eMANVSPC;
                changeItemStatusWithMode(eGameMode);
            }
            else if (configItems.iMode == EGAMEMODE.eCHESSNOTATION)
            {
                manComputerToolStripMenuItem.Checked = false;
                chessNotationToolStripMenuItem.Checked = true;
                eGameMode = EGAMEMODE.eCHESSNOTATION;
                changeItemStatusWithMode(eGameMode);
            }
        }

        private void dumpConfiguration()
        {
            StreamWriter sw = new StreamWriter(@"config.txt");

            if (configItems.bShowHistory)
            {
                sw.WriteLine("presentHistory=true");
            }
            else
            {
                sw.WriteLine("presentHistory=false");
            }

            if (configItems.bTakeBlack)
            {
                sw.WriteLine("takeBlack=true");
            }
            else
            {
                sw.WriteLine("takeBlack=false");
            }

            if (configItems.bUseRule)
            {
                sw.WriteLine("useRule=true");
            }
            else
            {
                sw.WriteLine("useRule=false");
            }

            sw.WriteLine("conponent=" + configItems.iConponent.ToString());
            sw.WriteLine("level=" + configItems.iLevel.ToString());
            sw.WriteLine("mode=" + configItems.iMode.ToString());

            sw.Close();
        }

        private void loadConfiguration()
        {
            StreamReader sr = new StreamReader(@"config.txt");
            while (!sr.EndOfStream)
            {
                string str = sr.ReadLine();

                if (str.IndexOf("presentHistory=true") >= 0)
                {
                    configItems.bShowHistory = true;
                }
                if (str.IndexOf("presentHistory=false") >= 0)
                {
                    configItems.bShowHistory = false;
                }
                if (str.IndexOf("takeBlack=true") >= 0)
                {
                    configItems.bTakeBlack = true;
                }
                if (str.IndexOf("takeBlack=false") >= 0)
                {
                    configItems.bTakeBlack = false;
                }
                if (str.IndexOf("useRule=true") >= 0)
                {
                    configItems.bUseRule = true;
                }
                if (str.IndexOf("useRule=false") >= 0)
                {
                    configItems.bUseRule = false;
                }
                if (str.IndexOf("conponent=") >= 0)
                {
                    string prefix = "conponent=";
                    configItems.iConponent = int.Parse(str.Substring(prefix.Length));
                }
                if (str.IndexOf("level=") >= 0)
                {
                    string prefix = "level=";
                    configItems.iLevel = int.Parse(str.Substring(prefix.Length));
                }
                if (str.IndexOf("mode=eMANVSPC") >= 0)
                {
                    configItems.iMode = EGAMEMODE.eMANVSPC;
                }
                else if (str.IndexOf("mode=eCHESSNOTATION") >= 0)
                {
                    configItems.iMode = EGAMEMODE.eCHESSNOTATION;
                }
            }
            sr.Close();

            applyConfiguration();
        }

        // present one step on GUI
        //=========================
        private void showStep(StepInfo si, int iStep)
        {
            Label label = LabelGameRecord[si.point.Y, si.point.X];
            label.Visible = true;
            label.Text = iStep.ToString();

            if (si.side == 1)
            {
                label.ForeColor = Color.White;
                if (iPlayNum - iStep > 1)
                    label.Image = ilChessman.Images[0];
                else
                    label.Image = ilChessman.Images[2];
            }
            else
            {
                label.ForeColor = Color.Black;
                if (iPlayNum - iStep > 1)
                    label.Image = ilChessman.Images[1];
                else
                    label.Image = ilChessman.Images[3];
            }

            label.BackColor = Color.Transparent;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.BringToFront();
        }

        // present all steps on GUI
        //=========================
        private void showSteps()
        {
            if (eGameMode == EGAMEMODE.eMANVSPC)
            {
                int baseStep = 0;
                if (mySide == 1)
                {
                    showStep(new StepInfo(mySide, new Point(7, 7)), 1);
                    baseStep = 1;
                }

                for (int i = 0; i < historySteps.Count; i++)
                {
                    showStep(historySteps[i].stepinfo[0], i * 2 + 1 + baseStep);
                    showStep(historySteps[i].stepinfo[1], i * 2 + 2 + baseStep);
                }

                if ((iPlayNum - baseStep) % 2 != 0)
                {
                    // one extra step no in historySteps
                    showStep(new StepInfo(peerSide, new Point(compIndexX, compIndexY)), iPlayNum);
                }
            }
            else if (eGameMode == EGAMEMODE.eCHESSNOTATION)
            {
                for (int i = 0; i < historySteps_ChessNotation.Count; i++)
                {
                    showStep(historySteps_ChessNotation[i], i + 1);
                }
            }
        }

        // hide all steps
        //=================
        private void hideSteps()
        {
            for (int i=0; i<ChessBoardScale; i++)
                for (int j=0; j<ChessBoardScale; j++)
                    LabelGameRecord[i, j].Visible = false;
        }

        // play music. not implemented yet.
        //=================================
        private void playMusic()
        {
            
            // dummy function for used later
            /*
            System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
            sp.SoundLocation = @"C:\Documents and Settings\zhe.hou\Desktop\Sounds\click\BALLDPLY.wav";
            sp.Play();
            */
        }

        private void changeItemStatusWithMode(EGAMEMODE eMode)
        {
            if (eMode == EGAMEMODE.eMANVSPC)
            {
                buttonRegret.Enabled = false;
                btnTest.Enabled = true;
                buttonRegret.Text = "悔棋 （" + iMaxRepeatNum + "）";
                btnTest.Enabled = true;
                btnTest.Text = "开始";

                historySteps.Clear();
            }
            else if (eMode == EGAMEMODE.eCHESSNOTATION)
            {
                buttonRegret.Enabled = false;
                buttonRegret.Text = "悔棋";
                btnTest.Enabled = true;
                btnTest.Text = "重置";

                iSide_ChessNotation = 1;
                historySteps_ChessNotation.Clear();

                resetChessBoard();
            }
        }

        private void changeItemStatusWithRunning(bool bRunning)
        {
            if (bRunning == true)
            {
                takeBlackToolStripMenuItem1.Enabled = false;
                level1ToolStripMenuItem.Enabled = false;
                level2ToolStripMenuItem.Enabled = false;
                level3ToolStripMenuItem.Enabled = false;
                level4ToolStripMenuItem.Enabled = false;
                manComputerToolStripMenuItem.Enabled = false;
                chessNotationToolStripMenuItem.Enabled = false;
                loadToolStripMenuItem.Enabled = false;

                buttonRegret.Enabled = true;
            }
            else
            {
                takeBlackToolStripMenuItem1.Enabled = true;
                level1ToolStripMenuItem.Enabled = true;
                level2ToolStripMenuItem.Enabled = true;
                level3ToolStripMenuItem.Enabled = true;
                level4ToolStripMenuItem.Enabled = true;
                manComputerToolStripMenuItem.Enabled = true;
                chessNotationToolStripMenuItem.Enabled = true;
                loadToolStripMenuItem.Enabled = true;

                buttonRegret.Enabled = false;
            }
        }

        private void resetChessBoard()
        {
            bPresentHistory = showHistoryToolStripMenuItem.Checked;
            historySteps.Clear();

            iElpasedAfterEnd = 0;

            // init buttons' and labels' content
            lPlayNum.Text = "0";
            iPlayNum = 0;

            lHour.Text = "00";
            iHour = 0;
            lMinute.Text = "00";
            iMinute = 0;
            lSecond.Text = "00";
            iSecond = 0;

            lHour_Mine.Text = "00";
            iHour_Mine = 0;
            lMinute_Mine.Text = "00";
            iMinute_Mine = 0;
            lSecond_Mine.Text = "00";
            iSecond_Mine = 0;

            lHint.Text = "";

            preX_Comp = preY_Comp = -1;        
            preX_Self = preY_Self = -1; 

            // goBang begin to run
            goBang.run();
            ClearChessMan();
            hideSteps();
            bIAmThinking = false;

            playMusic();
        }

        // 'Start'/'Drop' button is clicked
        //=================================
        private void buttonTst_Click(object sender, EventArgs e)
        {
            if (eGameMode == EGAMEMODE.eCHESSNOTATION)
            {
                changeItemStatusWithMode(EGAMEMODE.eCHESSNOTATION);
                return; 
            }

            if (!running)
            {
                LogInfo.getInstance().WriteLogFlush("----------------");
                LogInfo.getInstance().WriteLogFlush("A new set starts");
                LogInfo.getInstance().WriteLogFlush("----------------");

                // indicate game is starting now
                running = true;
                resetChessBoard();

                goBang.setMaxProbeStep(iMaxProbeSteps);
                iMaxRepeatNum = 3;
                buttonRegret.Text = "悔棋 （" + iMaxRepeatNum + "）";
                btnTest.Text = "认输";

                tmTimer.Enabled = true;

                if (takeBlackToolStripMenuItem1.Checked)
                {
                    mySide = -1; // I take white (I am computer).
                    peerSide = 1;
                }
                else
                {
                    mySide = 1; // wow, I take black (I am computer).
                    peerSide = -1;

                    // put the first chessman at center
                    goBang.putChessMan(ChessBoardScale / 2, ChessBoardScale / 2, mySide);
                    PutChessMan(ChessBoardScale / 2, ChessBoardScale / 2, ref preX_Self, ref preY_Self, mySide);
                    preX_Self = ChessBoardScale / 2;
                    preY_Self = ChessBoardScale / 2;
                }
            }
            else
            {
                // user accept fail, and we restore buttons' and labels' content
                backgroundWorker_Steps.CancelAsync();

                running = false;
                lHint.Text = "HeiHei, I Win!!";
                if (iPeerIndex >= 0)
                    pictureBox1.Image = ilPresentation.Images[iPeerIndex * 4 + 0];
                btnTest.Text = "开始";
                changeItemStatusWithRunning(running);
            }

            changeItemStatusWithRunning(running);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        /*
        // input a chessman and flash it
        private void flash(int x, int y, int side)
        {
            goBang.putChessMan(x, y, side);
            drawChessBoard();
            System.Threading.Thread.Sleep(500);
            goBang.removePos(x, y);
            drawChessBoard();
            System.Threading.Thread.Sleep(500);
            goBang.putChessMan(x, y, side);
            drawChessBoard();
            System.Threading.Thread.Sleep(500);
        }
        */

        // flash a red circle to indicate it is a forbidden position
        //==========================================================
        private void flashRecCircle(int x, int y, int width)
        {
            
            Pen redPen = new Pen(Color.Red, width);

            Rectangle rect = new Rectangle(
                new Point((int)colPosition[y] - (int)chessManSemiDiameter - width, (int)rowPosition[x] - (int)chessManSemiDiameter - width),
                new Size((int)chessManDiameter + width + width, (int)chessManDiameter + width + width));

            float startAngle = 0.0F;
            float sweepAngle = 360.0F;
            Graphics grap = panelChessBorad.CreateGraphics();
            grap.DrawArc(redPen, rect, startAngle, sweepAngle);

            System.Threading.Thread.Sleep(500);
            // drawChessBoard();

            System.Threading.Thread.Sleep(500);
            // drawChessBoard();
            grap.DrawArc(redPen, rect, startAngle, sweepAngle);

            System.Threading.Thread.Sleep(500);
            // drawChessBoard();
        }

        // menu item of 'Forbidding Rule' is clicked
        //===========================================
        private void applyRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                ((ToolStripMenuItem)sender).Checked = false;
                forbidRule = 0;
            }
            else
            {
                ((ToolStripMenuItem)sender).Checked = true;
                forbidRule = 1;
            }
        }

        // menu item to present Forbidding Rule is clicked
        //==================================================
        private void ruleIntroductionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormRule formRule = new FormRule();
            formRule.ShowDialog(this);
        }

        // menu item to present About is clicked
        //=======================================
        private void aboutGoBangToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();
            aboutBox.ShowDialog(this);
        }

        // button to 'Regret' is clicked
        //==============================
        private void buttonRegret_Click(object sender, EventArgs e)
        {
            if (eGameMode == EGAMEMODE.eMANVSPC)
            {
                if (iMaxRepeatNum <= 0)
                    return;

                if (bIAmThinking)
                    return;

                iMaxRepeatNum--;
                buttonRegret.Text = "悔棋 （" + iMaxRepeatNum + "）";

                if (iMaxRepeatNum == 0)
                    buttonRegret.Enabled = false;

                PairStepInfo psi = historySteps[historySteps.Count - 1];
                PairStepInfo psi_pre = new PairStepInfo(mySide, new Point(-1, -1), peerSide, new Point(-1, -1));
                if (historySteps.Count > 1)
                    psi_pre = historySteps[historySteps.Count - 2];

                goBang.removePos(psi.stepinfo[1].point.X, psi.stepinfo[1].point.Y);
                takeChessMan(psi.stepinfo[1].point.X, psi.stepinfo[1].point.Y,
                    psi_pre.stepinfo[1].point.X, psi_pre.stepinfo[1].point.Y, psi.stepinfo[1].side);
                preX_Self = psi_pre.stepinfo[1].point.X;
                preY_Self = psi_pre.stepinfo[1].point.Y;

                goBang.removePos(psi.stepinfo[0].point.X, psi.stepinfo[0].point.Y);
                takeChessMan(psi.stepinfo[0].point.X, psi.stepinfo[0].point.Y,
                    psi_pre.stepinfo[0].point.X, psi_pre.stepinfo[0].point.Y, psi.stepinfo[0].side);
                preX_Comp = psi_pre.stepinfo[0].point.X;
                preY_Comp = psi_pre.stepinfo[0].point.Y;

                removedsteps.Add(historySteps[historySteps.Count - 1]);
                historySteps.Remove(historySteps[historySteps.Count - 1]);
                if (historySteps.Count == 0)
                {
                    buttonRegret.Enabled = false;
                }
            }
            else if (eGameMode == EGAMEMODE.eCHESSNOTATION)
            {
                if (historySteps_ChessNotation.Count == 0)
                    return; 
                StepInfo si = historySteps_ChessNotation[historySteps_ChessNotation.Count - 1];
                StepInfo preSI = new StepInfo(si.side, new Point(-1, -1));

                if (historySteps_ChessNotation.Count >= 3) {
                    preSI = historySteps_ChessNotation[historySteps_ChessNotation.Count - 3];
                }
                goBang.removePos(si.point.X, si.point.Y);
                takeChessMan(si.point.X, si.point.Y, preSI.point.X, preSI.point.Y, si.side);

                historySteps_ChessNotation.Remove(historySteps_ChessNotation[historySteps_ChessNotation.Count - 1]);
                iSide_ChessNotation = si.side;
                if (historySteps_ChessNotation.Count == 0)
                {
                    buttonRegret.Enabled = false;
                    preX_Self = preY_Self = preX_Comp = preY_Comp = -1;
                    return; 
                }

                if (iSide_ChessNotation == 1){
                    preX_Self = preY_Self = -1;
                    if (historySteps_ChessNotation.Count >= 2)
                    {
                        preX_Self = historySteps_ChessNotation[historySteps_ChessNotation.Count - 2].point.X;
                        preY_Self = historySteps_ChessNotation[historySteps_ChessNotation.Count - 2].point.Y;
                    }
                }
                else if (iSide_ChessNotation == -1)
                {
                    preX_Comp = preY_Comp = -1;
                    if (historySteps_ChessNotation.Count > 2)
                    {
                        preX_Comp = historySteps_ChessNotation[historySteps_ChessNotation.Count - 2].point.X;
                        preY_Comp = historySteps_ChessNotation[historySteps_ChessNotation.Count - 2].point.Y;
                    }
                }
            }
        }

        /*
        private void buttonRepeat_Click(object sender, EventArgs e)
        {
            int index = removedsteps.Count - 1;
            goBang.putChessMan(removedsteps[index].stepinfo[0].point.X,
                removedsteps[index].stepinfo[0].point.Y,
                removedsteps[index].stepinfo[0].side);
            goBang.putChessMan(removedsteps[index].stepinfo[1].point.X,
                removedsteps[index].stepinfo[1].point.Y,
                removedsteps[index].stepinfo[1].side);
            historySteps.Add(removedsteps[index]);
            buttonRegret.Enabled = true;
            removedsteps.Remove(removedsteps[index]);
            if (removedsteps.Count == 0)
            {
                // buttonRepeat.Enabled = false;
            }
        }
        */

        // when user click a position to put a chessman
        //==============================================
        private void pbCM_Click(object sender, EventArgs e)
        {
            if (!running) return;

            // update chessman on the board
            PictureBox pb = (PictureBox)sender;
            pb.Image = global::WindowsFormsApplication1.Properties.Resources.blackchessman1;
            pb.Visible = true;
            pb.Refresh();

            // update chessman on the goBang
            // MessageBox.Show(pb.Name.ToString());
        }

        // when user click a position to put a chessman
        //==============================================
        private void chessBoardPictureBox_Click(object sender, EventArgs e)
        {
            if (eGameMode == EGAMEMODE.eMANVSPC && (!running || bIAmThinking))
                return;

            PictureBox pb = (PictureBox)sender;
            MouseEventArgs mea = (MouseEventArgs)e;

            if (mea.X < X_START || mea.Y < Y_START)
                return;

            // calculate x index
            int xIndex = 0, yIndex = 0;
            for (xIndex = 0; xIndex < ChessBoardScale; xIndex++)
            {
                if (Math.Abs(ChessMan[0, xIndex].Location.X + GRIDSIZE / 2 - mea.X) < GRIDSIZE / 2)
                    break;
            }

            for (yIndex = 0; yIndex < ChessBoardScale; yIndex++)
            {
                if (Math.Abs(ChessMan[yIndex, 0].Location.Y + GRIDSIZE / 2 - mea.Y) < GRIDSIZE / 2)
                    break;
            }
 
            if (xIndex >= ChessBoardScale || yIndex >= ChessBoardScale)
                return;

            if (goBang.isOccupied(xIndex, yIndex))
                return;

            if (eGameMode == EGAMEMODE.eMANVSPC)
            {
                // players try to put a chessman
                //===============================
                int ret = goBang.putChessMan(xIndex, yIndex, peerSide);

                // check whether the chessman can be put
                if ((forbidRule == 1) && (ret == -1))
                {
                    flashRecCircle(xIndex, yIndex, 10);
                    return;
                }
                PutChessMan(xIndex, yIndex, ref preX_Comp, ref preY_Comp, peerSide);

                // TBD: play some sound
                // System.Media.SystemSounds.Question.Play();

                if (goBang.hasWon(peerSide, xIndex, yIndex))
                {
                    running = false;
                    changeItemStatusWithRunning(running);
                    lHint.Text = "Wow, You Win!!";
                    btnTest.Text = "开始";
                    if (iPeerIndex >= 0)
                        pictureBox1.Image = ilPresentation.Images[iPeerIndex * 4 + 1];
                    return;
                }
                else if (goBang.isDraw())
                {
                    running = false;
                    changeItemStatusWithRunning(running);

                    lHint.Text = "uh, it is a draw!!";

                    if (iPeerIndex >= 0)
                        pictureBox1.Image = ilPresentation.Images[iPeerIndex * 4 + 0];
                    return;
                }

                // run algorithm async
                compIndexX = xIndex;
                compIndexY = yIndex;
                backgroundWorker_Steps.RunWorkerAsync();


                /*
                // it is my turn to put a chessman 
                Point point = goBang.setAChessman_new(mySide, style, forbidRule); // TBD: may need execute in another thread


                if ((point.X < 0) || (point.Y < 0))
                {
                    // TBD: is this possible?
                    return;
                }
                else
                {
                    // int temp = point.Y; point.Y = point.X; point.X = temp; // gaona still has something wrong
                    goBang.putChessMan(point.X, point.Y, mySide);
                    PutChessMan(point.X, point.Y, ref preX_Self, ref preY_Self, mySide);
                    preX_Self = point.X;
                    preY_Self = point.Y;

                    int testNum = goBang.numOfChessman(); // gaona
                }

                if (goBang.hasWon(mySide, point.X, point.Y))
                {
                    running = false;
                    MessageBox.Show("HeiHei, I Win!!");
                    return;
                }
                else if (goBang.isDraw())
                {
                    running = false;
                    MessageBox.Show("uh, it is a draw!!");
                    return;
                }

                historySteps.Add(new PairStepInfo(peerSide, new Point(xIndex, yIndex), mySide, point));
                buttonRegret.Enabled = true;
                */
            }
            else if (eGameMode == EGAMEMODE.eCHESSNOTATION)
            {
                int ret = goBang.putChessMan(xIndex, yIndex, iSide_ChessNotation);

                if (iSide_ChessNotation == 1)
                    PutChessMan(xIndex, yIndex, ref preX_Self, ref preY_Self, iSide_ChessNotation);
                else
                    PutChessMan(xIndex, yIndex, ref preX_Comp, ref preY_Comp, iSide_ChessNotation);
                historySteps_ChessNotation.Add(new StepInfo (iSide_ChessNotation, new Point(xIndex, yIndex)));

                buttonRegret.Enabled = true;

                // TBD: play some sound
                // System.Media.SystemSounds.Question.Play();

                if (goBang.hasWon(iSide_ChessNotation, xIndex, yIndex))
                {
                    if (iSide_ChessNotation == 1)
                        lHint.Text = "Black Win!!";
                    else
                        lHint.Text = "White Win!!";
                }
                else if (goBang.isDraw())
                {
                    lHint.Text = "uh, it is a draw!!";
                }

                iSide_ChessNotation *= -1; 
            }
        }

        // clear all checkman on GUI
        //==========================
        private void ClearChessMan()
        {
            for (int i = 0; i < ChessBoardScale; i++)
                for (int j = 0; j < ChessBoardScale; j++)
                {
                    ChessMan[i, j].Visible = false;
                    ChessMan[i, j].Refresh();
                }
        }

        // put a chessman on GUI
        //======================
        private void PutChessMan(int x, int y, ref int preX, ref int preY, int side)
        {
            if (side == 1)
            {
                if ((preX >= 0) && (preY >= 0))
                {
                    ChessMan[preY, preX].Image = ilChessman.Images[0];
                    ChessMan[preY, preX].Refresh();
                }
                ChessMan[y, x].Image = ilChessman.Images[2];
            }
            else
            {
                if ((preX >= 0) && (preY >= 0))
                {
                    ChessMan[preY, preX].Image = ilChessman.Images[1];
                    ChessMan[preY, preX].Refresh();
                }
                ChessMan[y, x].Image = ilChessman.Images[3];
            }
            ChessMan[y, x].Visible = true;
            iPlayNum++;
            lPlayNum.Text = iPlayNum.ToString();

            LogInfo.getInstance().WriteLogFlush("\nSTEPS: " + iPlayNum + "\n");

            if (bPresentHistory)
            {
                showStep(new StepInfo(side, new Point(x, y)), iPlayNum);

                if (iPlayNum >= 3)
                {
                    showStep(new StepInfo(side, new Point(preX, preY)), iPlayNum - 2);
                }
            }

            preX = x;
            preY = y;
            ChessMan[y, x].Refresh();
        }

        // withdraw a chessman from GUI
        //=============================
        private void takeChessMan(int x, int y, int preX, int preY, int side)
        {
            ChessMan[y, x].Visible = false;
            ChessMan[y, x].Refresh();
            LabelGameRecord[y, x].Visible = false;

            if (side == 1)
            {
                if ((preX >= 0) && (preY >= 0))
                {
                    ChessMan[preY, preX].Image = ilChessman.Images[2];
                    ChessMan[preY, preX].Refresh();

                    LabelGameRecord[preY, preX].Visible = false;
                    ChessMan[preY, preX].Refresh();
                }
                ChessMan[y, x].Image = null;
            }
            else
            {
                if ((preX >= 0) && (preY >= 0))
                {
                    ChessMan[preY, preX].Image = ilChessman.Images[3];
                    ChessMan[preY, preX].Refresh();

                    LabelGameRecord[preY, preX].Visible = false;
                    ChessMan[preY, preX].Refresh();
                }
                ChessMan[y, x].Image = null;
            }
            
            iPlayNum--;
            lPlayNum.Text = iPlayNum.ToString();

            if (bPresentHistory)
            {
                if (preX > -1 && preY > -1)
                    showStep(new StepInfo(side, new Point(preX, preY)), iPlayNum - 1);
            }
        }

        // menu item to select side is clicked
        //=====================================
        private void takeBlackToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mi = (ToolStripMenuItem)sender;
            if (mi.Checked)
            {
                mi.Checked = false;
                pbChessman_up.Image = ilChessmanBoxes.Images[0];
                pbChessman_down.Image = ilChessmanBoxes.Images[1];
                configItems.bTakeBlack = false;
                dumpConfiguration();
            }
            else
            {
                mi.Checked = true;
                pbChessman_up.Image = ilChessmanBoxes.Images[1];
                pbChessman_down.Image = ilChessmanBoxes.Images[0];
                configItems.bTakeBlack = true;
                dumpConfiguration();
            }
        }

        // timer clicks. we will update hint picture, hint message, and timer on GUI
        //==========================================================================
        private void tmTimer_Tick(object sender, EventArgs e)
        {
            if (running)
            {
                int s = 0; 
                if (bIAmThinking)
                {
                    iSecond++;
                    s = iSecond;
                    if (iSecond == 60)
                    {
                        iMinute++;
                        iSecond = 0;
                        if (iMinute == 60)
                        {
                            iHour++;
                            iMinute = 0;
                            if (iHour == 99)
                            {
                                iHour = 0;
                            }
                        }
                    }
                    if (iSecond < 10)
                    {
                        lSecond.Text = "0" + iSecond.ToString();
                    }
                    else
                    {
                        lSecond.Text = iSecond.ToString();
                    }
                    if (iMinute < 10)
                    {
                        lMinute.Text = "0" + iMinute.ToString();
                    }
                    else
                    {
                        lMinute.Text = iMinute.ToString();
                    }
                    if (iHour < 10)
                    {
                        lHour.Text = "0" + iHour.ToString();
                    }
                    else
                    {
                        lHour.Text = iHour.ToString();
                    }
                }
                else
                {
                    iSecond_Mine++;
                    s = iSecond_Mine;
                    if (iSecond_Mine == 60)
                    {
                        iMinute_Mine++;
                        iSecond_Mine = 0;
                        if (iMinute_Mine == 60)
                        {
                            iHour_Mine++;
                            iMinute_Mine = 0;
                            if (iHour_Mine == 99)
                            {
                                iHour_Mine = 0;
                            }
                        }
                    }
                    if (iSecond_Mine < 10)
                    {
                        lSecond_Mine.Text = "0" + iSecond_Mine.ToString();
                    }
                    else
                    {
                        lSecond_Mine.Text = iSecond_Mine.ToString();
                    }
                    if (iMinute_Mine < 10)
                    {
                        lMinute_Mine.Text = "0" + iMinute_Mine.ToString();
                    }
                    else
                    {
                        lMinute_Mine.Text = iMinute_Mine.ToString();
                    }
                    if (iHour_Mine < 10)
                    {
                        lHour_Mine.Text = "0" + iHour_Mine.ToString();
                    }
                    else
                    {
                        lHour_Mine.Text = iHour_Mine.ToString();
                    }
                }

                {
                    string str = "";
                    if (s % 5 == 0)
                        str = ".";
                    else if (s % 5 == 1)
                        str = "..";
                    else if (s % 5 == 2)
                        str = "...";
                    else if (s % 5 == 3)
                        str = "....";
                    else if (s % 5 == 4)
                        str = "";
                    if (bIAmThinking)
                    {
                        lHint.Text = "Thinking " + str;
                        if (iPeerIndex >= 0)
                            pictureBox1.Image = ilPresentation.Images[iPeerIndex * 4 + 2];
                    }
                    else
                    {
                        lHint.Text = "Waiting " + str;
                        if (iPeerIndex >= 0)
                            pictureBox1.Image = ilPresentation.Images[iPeerIndex * 4 + 3];
                    }

                    lHint.Refresh();
                }
            }
            else
            {
                // restore picture after 3 seconds
                if (iElpasedAfterEnd >= 0)
                    iElpasedAfterEnd++;

                if (iElpasedAfterEnd == 5)
                {
                    if (iPeerIndex >= 0)
                        pictureBox1.Image = ilPresentation.Images[iPeerNum * 4];
                    iElpasedAfterEnd = -1;
                }

            }
        }

        // call algorithm to calculate next step asynchronously
        //=====================================================
        private void backgroundWorker_Steps_DoWork(object sender, DoWorkEventArgs e)
        {
            // it is my turn to put a chessman 
            bIAmThinking = true;
            // chessBoardPictureBox.Cursor = Cursors.WaitCursor;

            myNextPosition = goBang.setAChessman_new(mySide, style, forbidRule);  
        }

        // we get the result of next step 
        //================================
        private void backgroundWorker_Steps_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bIAmThinking = false;
            chessBoardPictureBox.Cursor = Cursors.Default;
            if ((myNextPosition.X < 0) || (myNextPosition.Y < 0))
            {
                // TBD: is this possible?
                return;
            }
            else
            {
                // int temp = point.Y; point.Y = point.X; point.X = temp; // gaona still has something wrong
                goBang.putChessMan(myNextPosition.X, myNextPosition.Y, mySide);
                PutChessMan(myNextPosition.X, myNextPosition.Y, ref preX_Self, ref preY_Self, mySide);
                preX_Self = myNextPosition.X;
                preY_Self = myNextPosition.Y;
            }

            historySteps.Add(new PairStepInfo(peerSide, new Point(compIndexX, compIndexY), mySide, myNextPosition));

            if (goBang.hasWon(mySide, myNextPosition.X, myNextPosition.Y))
            {
                running = false;
                lHint.Text = "HeiHei, I Win!!";
                if (iPeerIndex >= 0)
                    pictureBox1.Image = ilPresentation.Images[iPeerIndex * 4 + 0];
                btnTest.Text = "开始";
                changeItemStatusWithRunning(running);
                return;
            }
            else if (goBang.isDraw())
            {
                running = false;
                lHint.Text = "uh, it is a draw!!";
                if (iPeerIndex >= 0)
                    pictureBox1.Image = ilPresentation.Images[iPeerIndex * 4 + 1];
                btnTest.Text = "开始";
                changeItemStatusWithRunning(running);
                return;
            }

            if (iMaxRepeatNum > 0)
                buttonRegret.Enabled = true;
        }

        // user change to present/hide steps
        //==================================
        private void showHistoryToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                bPresentHistory = true;
                showSteps();
                configItems.bShowHistory = true;
                dumpConfiguration();
            }
            else
            {
                bPresentHistory = false;
                hideSteps();
                configItems.bShowHistory = false;
                dumpConfiguration();
            }
        }

        private void level1ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                // un-check all other levels
                level2ToolStripMenuItem.Checked = false;
                level3ToolStripMenuItem.Checked = false;
                level4ToolStripMenuItem.Checked = false;
                iMaxProbeSteps = 4;

                configItems.iLevel = 1;
                dumpConfiguration();
            }
        }

        private void level2ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                // un-check all other levels
                level1ToolStripMenuItem.Checked = false;
                level3ToolStripMenuItem.Checked = false;
                level4ToolStripMenuItem.Checked = false;
                iMaxProbeSteps = 5;

                configItems.iLevel = 2;
                dumpConfiguration();
            }
        }

        private void level3ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                // un-check all other levels
                level1ToolStripMenuItem.Checked = false;
                level2ToolStripMenuItem.Checked = false;
                level4ToolStripMenuItem.Checked = false;
                iMaxProbeSteps = 6;

                configItems.iLevel = 3;
                dumpConfiguration();
            }
        }

        private void level4ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                // un-check all other levels
                level1ToolStripMenuItem.Checked = false;
                level2ToolStripMenuItem.Checked = false;
                level3ToolStripMenuItem.Checked = false;
                iMaxProbeSteps = 7;

                configItems.iLevel = 4;
                dumpConfiguration();
            }
        }

        private void mickeyMouToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                Item2MenuItem.Checked = false;
                notSelectMenuItem.Checked = false;
                iPeerIndex = 0;
            }
        }

        private void Item2MenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                Item1ItemMenu.Checked = false;
                notSelectMenuItem.Checked = false;
                iPeerIndex = 1;
            }
        }

        private void notSelectMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                Item1ItemMenu.Checked = false;
                Item2MenuItem.Checked = false;
                iPeerIndex = -1;
                pictureBox1.Image = ilPresentation.Images[iPeerNum * 4];
            }
        }

        private void chessNotationToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                manComputerToolStripMenuItem.Checked = false;
                eGameMode = EGAMEMODE.eCHESSNOTATION;
                changeItemStatusWithMode(eGameMode);
                configItems.iMode = EGAMEMODE.eCHESSNOTATION;
                dumpConfiguration();
            }
        }

        private void manComputerToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                chessNotationToolStripMenuItem.Checked = false;
                eGameMode = EGAMEMODE.eMANVSPC;
                changeItemStatusWithMode(eGameMode);
                configItems.iMode = EGAMEMODE.eMANVSPC;
                dumpConfiguration();
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "xxgb files(*.xxgb)|*.xxgb|All files(*.*)|*.*";
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                string str = "";
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                if (sr.EndOfStream)
                {
                    MessageBox.Show("不合法的文件格式！");
                    return; 
                }
                str = sr.ReadLine();
                if (str.IndexOf("running=true") >= 0)
                {
                    running = true;
                }
                else
                {
                    running = false;
                }

                if (sr.EndOfStream)
                {
                    MessageBox.Show("不合法的文件格式！");
                    return;
                }
                str = sr.ReadLine();
                if (str.IndexOf("c") >= 0)
                {
                    mySide = 1;
                    peerSide = -1;

                    pbChessman_up.Image = ilChessmanBoxes.Images[0];
                    pbChessman_down.Image = ilChessmanBoxes.Images[1];
                    takeBlackToolStripMenuItem1.Checked = false;
                }
                else
                {
                    mySide = -1;
                    peerSide = 1;

                    pbChessman_up.Image = ilChessmanBoxes.Images[1];
                    pbChessman_down.Image = ilChessmanBoxes.Images[0];
                    takeBlackToolStripMenuItem1.Checked = true;
                }

                if (running)
                {
                    chessNotationToolStripMenuItem.Checked = false;
                    manComputerToolStripMenuItem.Checked = true;
                    eGameMode = EGAMEMODE.eMANVSPC;
                    changeItemStatusWithMode(eGameMode);

                    resetChessBoard();

                    goBang.setMaxProbeStep(iMaxProbeSteps);
                    iMaxRepeatNum = 3;
                    buttonRegret.Text = "悔棋 （" + iMaxRepeatNum + "）";
                    btnTest.Text = "认输";

                    tmTimer.Enabled = true;

                    str = sr.ReadLine();
                    lHour_Mine.Text = str.Substring(0, 2);
                    lMinute_Mine.Text = str.Substring(3, 2);
                    lSecond_Mine.Text = str.Substring(6, 2);
                    iHour_Mine = int.Parse(lHour_Mine.Text);
                    iMinute_Mine = int.Parse(lMinute_Mine.Text);
                    iSecond_Mine = int.Parse(lSecond_Mine.Text);

                    str = sr.ReadLine();
                    lHour.Text = str.Substring(0, 2);
                    lMinute.Text = str.Substring(3, 2);
                    lSecond.Text = str.Substring(6, 2);
                    iHour = int.Parse(lHour.Text);
                    iMinute = int.Parse(lMinute.Text);
                    iSecond = int.Parse(lSecond.Text);

                    if (mySide == 1)
                    {
                        // put (7, 7)
                        goBang.putChessMan(7, 7, mySide);
                        PutChessMan(7, 7, ref preX_Self, ref preY_Self, mySide);
                        preX_Self = 7;
                        preY_Self = 7;
                    }
                    while (!sr.EndOfStream)
                    {
                        str = sr.ReadLine();
                        
                        // players try to put a chessman
                        //===============================
                        int xIndex = OpsOfChessman_Singleton.DeConvertX(str[0]);
                        int yIndex = OpsOfChessman_Singleton.DeConvertY(int.Parse(str.Substring(2)));
                        int ret = goBang.putChessMan(xIndex, yIndex, peerSide);

                        PutChessMan(xIndex, yIndex, ref preX_Comp, ref preY_Comp, peerSide);

                        // TBD: play some sound
                        // System.Media.SystemSounds.Question.Play();

                        compIndexX = xIndex;
                        compIndexY = yIndex;

                        // computer put a chessman
                        //=========================
                        str = sr.ReadLine();
                        xIndex = OpsOfChessman_Singleton.DeConvertX(str[0]);
                        yIndex = OpsOfChessman_Singleton.DeConvertY(int.Parse(str.Substring(2)));

                        goBang.putChessMan(xIndex, yIndex, mySide);
                        PutChessMan(xIndex, yIndex, ref preX_Self, ref preY_Self, mySide);
                        preX_Self = xIndex;
                        preY_Self = yIndex;

                        historySteps.Add(new PairStepInfo(peerSide, new Point(compIndexX, compIndexY), mySide, new Point(xIndex, yIndex)));
                    }
                }
                else
                {
                    sr.ReadLine();
                    sr.ReadLine();

                    chessNotationToolStripMenuItem.Checked = true;
                    manComputerToolStripMenuItem.Checked = false;
                    eGameMode = EGAMEMODE.eCHESSNOTATION;
                    changeItemStatusWithMode(eGameMode);
                    iSide_ChessNotation = 1;
                    while (!sr.EndOfStream)
                    {
                        str = sr.ReadLine();
                        int xIndex = OpsOfChessman_Singleton.DeConvertX(str[0]);
                        int yIndex = OpsOfChessman_Singleton.DeConvertY(int.Parse(str.Substring(2)));

                        int ret = goBang.putChessMan(xIndex, yIndex, iSide_ChessNotation);

                        if (iSide_ChessNotation == 1)
                            PutChessMan(xIndex, yIndex, ref preX_Self, ref preY_Self, iSide_ChessNotation);
                        else
                            PutChessMan(xIndex, yIndex, ref preX_Comp, ref preY_Comp, iSide_ChessNotation);
                        historySteps_ChessNotation.Add(new StepInfo(iSide_ChessNotation, new Point(xIndex, yIndex)));

                        buttonRegret.Enabled = true;

                        iSide_ChessNotation *= -1; 
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "xxgb files(*.xxgb)|*.xxgb|All files(*.*)|*.*";
            DialogResult result = saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(saveFileDialog1.FileName);
                if (fi.Exists)
                    fi.Delete();

                FileStream fs = fi.OpenWrite();
                StreamWriter sw = new StreamWriter(fs);

                if (running)
                    sw.WriteLine("running=true");
                else
                    sw.WriteLine("running=false");

                if (mySide == 1)
                    sw.WriteLine("c");
                else
                    sw.WriteLine("m");

                if (running)
                {
                    sw.WriteLine(lHour_Mine.Text + " " + lMinute_Mine.Text + " " + lSecond_Mine.Text);
                    sw.WriteLine(lHour.Text + " " + lMinute.Text + " " + lSecond.Text);

                }
                else
                {
                    sw.WriteLine("00 00 00");
                    sw.WriteLine("00 00 00");
                }

                // dump steps into file

                if (eGameMode == EGAMEMODE.eMANVSPC)
                {
                    if (mySide == 1)
                    {
                        sw.WriteLine(OpsOfChessman_Singleton.ConvertX(7) + " " + OpsOfChessman_Singleton.ConvertY(7));
                    }

                    for (int i = 0; i < historySteps.Count; i++)
                    {
                        sw.WriteLine(OpsOfChessman_Singleton.ConvertX(historySteps[i].stepinfo[0].point.X) + " " + 
                                     OpsOfChessman_Singleton.ConvertY(historySteps[i].stepinfo[0].point.Y));

                        sw.WriteLine(OpsOfChessman_Singleton.ConvertX(historySteps[i].stepinfo[1].point.X) + " " +
                                     OpsOfChessman_Singleton.ConvertY(historySteps[i].stepinfo[1].point.Y));
                    }
                }
                else if (eGameMode == EGAMEMODE.eCHESSNOTATION)
                {
                    for (int i = 0; i < historySteps_ChessNotation.Count; i++)
                    {
                        sw.WriteLine(OpsOfChessman_Singleton.ConvertX(historySteps_ChessNotation[i].point.X) + " " +
                                     OpsOfChessman_Singleton.ConvertY(historySteps_ChessNotation[i].point.Y));
                    }
                }

                // end

                
                sw.Close();
            }
        }

    }

    //================================
    // detail information of one step
    //================================
    class StepInfo
    {
        public int side;
        public Point point;

        public StepInfo(int side, Point point)
        {
            this.side = side;
            this.point = point;
        }
    }

    //====================================
    // information of pair of steps
    //====================================
    class PairStepInfo
    {
        public StepInfo[] stepinfo;

        public PairStepInfo(int side1, Point point1, int side2, Point point2)
        {
            stepinfo = new StepInfo[2];
            stepinfo[0] = new StepInfo(side1, point1);
            stepinfo[1] = new StepInfo(side2, point2);
        }
    }

    //==================================================
    // facility class to record status of some chessman
    //==================================================
    public class ChessmenStatus
    {
        public uint numEndEmpty = 0; // how many ends are empty
        public uint numContChessmen = 0; // how many chessmen are continuous
        public List<Point> lstEPs = new List<Point>(); // all end points if there are any
    }

    //======================
    //to present a chessman
    //======================
    public class ChessMan : IComparable<ChessMan>
    {
        private int weight = 0;
        private Point point = new Point();
        public ChessMan(int x, int y, int weight)
        {
            point.X = x;
            point.Y = y;
            this.weight = weight;
        }
        public Point C_Point
        {
            set { point = value; }
            get { return point; }
        }
        public int C_Weight
        {
            set { weight = value; }
            get { return weight; }
        }
        public int CompareTo(ChessMan other)
        {
            if (other == null) return 1;
            return weight.CompareTo(other.weight);
        }
    }

    //========================================
    // store chessman and return one randomly
    //========================================
    class ChessmenStore
    {
        // private int MAX_PROBE_STEPS_NORMAL = 2;
        private int MAX_PROBE_RANGE_I = 4;  // my probe range 
        private int MAX_PROBE_RANGE_P = 4;  // peer's probe range

        private List<ChessMan> points = new List<ChessMan>();
        private int[,] ChessMan = null;
        private int side;
        private int MAX_PROBE_STEPS_SEMI = OpsOfChessman_Singleton.GetMaxProbeSteps();

        public void setChessMan(int[,] chessMan, int side)
        {
            ChessMan = chessMan;
            this.side = side;
        }

        public void setMaxProbeStep(int iMaxSteps)
        {
            MAX_PROBE_STEPS_SEMI = OpsOfChessman_Singleton.GetMaxProbeSteps();
        }

        public void storeChessmen(int x, int y, int weight = 0)
        {
            points.Add(new ChessMan(x, y, weight));
        }

        public void clear()
        {
            points.Clear();
        }

        public int getPointNum()
        {
            return points.Count;
        }

        public bool empty()
        {
            if (points.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // bFullyCheck means not only check based on weight, but check whether we can get a stronger semi-WIN point or
        // I can remove all the semi-LOSS point.
        public Point getPoint(bool bFullyCheck = false)
        {
            List<ChessMan> finalPointsSet = new List<ChessMan>();

            if (empty())
            {
                return (new Point(-1, -1));
            }
            else
            {
                if (bFullyCheck)
                {
                    List<ChessMan> lstCheckMan = new List<ChessMan>();
                    int iMinSemiLoss = -1;
                    for (int i = 0; i < points.Count; i++)
                    {
                        List<Point> lstSemiLoss = new List<Point>();
                        ChessMan[points[i].C_Point.X, points[i].C_Point.Y] = side;

                        // check whether peer still have semi-win
                        
                        for (int ii = 0; ii < FormMain.ChessBoardScale; ii++)
                        {
                            for (int jj = 0; jj < FormMain.ChessBoardScale; jj++)
                            {
                                // LogInfo.getInstance().WriteLogFlush("RawStrategyImp.GetNextPos(): try semi-LOSS position " + ii + " " + jj);
                                int steps = 0;
                                int noMeaning = 0; 
                                if (OpsOfChessman_Singleton.Instance.IsSemiWinPoint(ChessMan, side * (-1), ii, jj, OpsOfChessman_Singleton.GetMaxProbeSteps(), ref steps, MAX_PROBE_RANGE_P, MAX_PROBE_RANGE_I, ref noMeaning))
                                {
                                    // LogInfo.getInstance().WriteLogFlush("RawStrategyImp.GetNextPos(): probe semi-LOSS position " + ii + " " + jj + " steps = " + steps);
                                    lstSemiLoss.Add(new Point(ii, jj));
                                }
                            }
                        }

                        if (iMinSemiLoss == -1 || iMinSemiLoss > lstSemiLoss.Count)
                        {
                            lstCheckMan.Clear();
                            lstCheckMan.Add(new ChessMan(points[i].C_Point.X, points[i].C_Point.Y, OpsOfChessman_Singleton.Instance.GetWeight(ChessMan, points[i].C_Point.X, points[i].C_Point.Y, side)));
                            iMinSemiLoss = lstSemiLoss.Count;
                        }

                        ChessMan[points[i].C_Point.X, points[i].C_Point.Y] = 0;
                    }

                    points.Clear();
                    points = lstCheckMan;
                }
                
                // order the list
                points.Sort();
                points.Reverse();
                int maxWeight = points.ElementAt(0).C_Weight;
                for (int i = 1; i < points.Count; i++)
                {
                    if (points.ElementAt(i).C_Weight < maxWeight)
                    {
                        // remove remaining elements
                        points.RemoveRange(i, (points.Count - i));
                    }
                }

                for (int i = 0; i < points.Count; i++)
                    finalPointsSet.Add(points[i]);
                

                Random randomObj = new Random();
                int random = randomObj.Next(0, finalPointsSet.Count);

                LogInfo.getInstance().WriteLogFlush("ChessManStore.getPoint(): select " + 
                    OpsOfChessman_Singleton.ConvertX(finalPointsSet[random].C_Point.X) + " " + 
                    OpsOfChessman_Singleton.ConvertY(finalPointsSet[random].C_Point.Y));
                return finalPointsSet[random].C_Point;
            }
        }
    }

    interface StrategyImp
    {
        Point GetNextPos(int[,] ChessMan, int side, int bForthForbidden);
        void setMaxProbeStep(int iMaxProbeStep); 
    }

    class TempStep
    {
        public Point p;
        public List<List<Point>> lstPoints;

        public TempStep(int x, int y)
        {
            p = new Point(x, y);
            lstPoints = new List<List<Point>>();
        }

        public void Copy(TempStep ts) {
            p.X = ts.p.X;
            p.Y = ts.p.Y;

            for (int i = 0; i < ts.lstPoints.Count; i++)
            {
                List<Point> ld = new List<Point>();
                List<Point> ls = ts.lstPoints[i];
                for (int j = 0; j < ls.Count; j++)
                {
                    ld.Add(new Point(ls[j].X, ls[j].Y));
                }

                lstPoints.Add(ld);
            }
        }
    }

    //======================================================
    // this is the first strategy to calculate the next step
    //======================================================
    class RawStrategyImp : StrategyImp
    {
        private int MAX_PROBE_STEPS_SEMI = 6;
        // private int MAX_PROBE_STEPS_NORMAL = 3;
        private int MAX_PROBE_RANGE_I = 4;  // my probe range 
        private int MAX_PROBE_RANGE_P = 0;  // peer's probe range. initialized by mine.

        public RawStrategyImp()
        {
            MAX_PROBE_RANGE_P = MAX_PROBE_RANGE_I/* + 1*/;
        }

        public void setMaxProbeStep(int iMaxProbeStep)
        {
            MAX_PROBE_STEPS_SEMI = OpsOfChessman_Singleton.GetMaxProbeSteps();
        }

        public Point GetNextPos(int[,] ChessMan, int side, int bForthForbidden)
        {
            //--------------------------------------------------------------------------
            // this is the raw strategy implementation, and also the first one - run run
            //--------------------------------------------------------------------------

            LogInfo.getInstance().WriteLogFlush("\n\nGetNextPos Begin\n\n");

            bool bCheckFully = false;
            ChessmenStore chessmanStore = new ChessmenStore();
            // chessmanStore.setMaxProbeStep(MAX_PROBE_STEPS_SEMI);
            chessmanStore.setChessMan(ChessMan, side);
            if (bForthForbidden == 1)
            {
                // mark all of chessman-server-forbidden position
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        if (OpsOfChessman_Singleton.Instance.isForthgoerForbidden(ChessMan, i, j, side))
                        {
                            ChessMan[i, j] = 2; // '2' means is a chessman-server-forbidden position
                        }
                    }
                }
            }

            bool bFindSemiLoss = false; 

            // find WIN points
            for (int i = 0; i < FormMain.ChessBoardScale; i++)
            {
                for (int j = 0; j < FormMain.ChessBoardScale; j++)
                {
                    if (OpsOfChessman_Singleton.Instance.IsWinPoint(ChessMan, side, i, j))
                    {
                        LogInfo.getInstance().WriteLogFlush("find WIN position " + OpsOfChessman_Singleton.ConvertX(i) + " " + OpsOfChessman_Singleton.ConvertY(j));
                        chessmanStore.storeChessmen(i, j);
                    }
                }
            }

            if (chessmanStore.empty())
            {
                // find LOSS points
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        if (OpsOfChessman_Singleton.Instance.IsLOSSPoint(ChessMan, side, i, j))
                        {
                            LogInfo.getInstance().WriteLogFlush("find LOSS position " + OpsOfChessman_Singleton.ConvertX(i) + " " + OpsOfChessman_Singleton.ConvertY(j));
                            chessmanStore.storeChessmen(i, j);
                        }
                    }
                }
            }

            int iMinSteps = -1;
            if (chessmanStore.empty())
            {
                // LogInfo.getInstance().WriteLogFlush("\nRawStrategyImp.GetNextPos(): try SEMI-WIN position");
                // LogInfo.getInstance().WriteLogFlush("====================================================");

                bFindSemiLoss = true;
                int iProbeSteps = OpsOfChessman_Singleton.GetMaxProbeSteps();
                // find semi-WIN points
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        int steps = 0;
                        int iRealProbeSteps = 0; 
                        if (OpsOfChessman_Singleton.Instance.IsSemiWinPoint(ChessMan, side, i, j, iProbeSteps, ref steps, MAX_PROBE_RANGE_P, MAX_PROBE_RANGE_I, ref iRealProbeSteps))
                        {
                            LogInfo.getInstance().WriteLogFlush("find semi-WIN position " + OpsOfChessman_Singleton.ConvertX(i) + " " + OpsOfChessman_Singleton.ConvertY(j) + " steps = " + steps);
                            chessmanStore.storeChessmen(i, j, steps * 2 * (-1)); // I need find points with minimum steps

                            if (iMinSteps == -1 || iMinSteps > steps)
                            {
                                iMinSteps = steps;
                                if (iProbeSteps > iRealProbeSteps) iProbeSteps = iRealProbeSteps;
                            }
                        }
                    }
                }
            }

            // find semi-LOSS points. If a semi-LOSS has less steps, we try to block semi-LOSS then.
            if (bFindSemiLoss)
            {
                // LogInfo.getInstance().WriteLogFlush("\nRawStrategyImp.GetNextPos(): try SEMI-LOSS position");
                // LogInfo.getInstance().WriteLogFlush("=====================================================");

                int iProbeSteps = OpsOfChessman_Singleton.GetMaxProbeSteps();
                // find semi-LOSS points
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        int steps = 0;
                        int iRealProbeSteps = 0; 
                        if (OpsOfChessman_Singleton.Instance.IsSemiWinPoint(ChessMan, side * (-1), i, j, iProbeSteps, ref steps, MAX_PROBE_RANGE_P, MAX_PROBE_RANGE_I, ref iRealProbeSteps))
                        {
                            LogInfo.getInstance().WriteLogFlush("find semi-LOSS position " + OpsOfChessman_Singleton.ConvertX(i) + " " + OpsOfChessman_Singleton.ConvertY(j) + " steps = " + steps);
                            if (iMinSteps != -1 && steps < iMinSteps)
                            {
                                chessmanStore.clear();
                                iMinSteps = -1;
                            }

                            if (iMinSteps != -1 && steps >= iMinSteps)
                                continue;

                            chessmanStore.storeChessmen(i, j, steps * 2 * (-1) - 1); // I need find points with minimum steps
                            bCheckFully = true;

                            if (iProbeSteps > iRealProbeSteps) iProbeSteps = iRealProbeSteps;
                        }
                    }
                }
            }

            // here, we miss something valuable to check. even if a point is not semi-win, it is important is we hold and can get multiple semi-win in the next step
            // gaona: do it

            if (chessmanStore.empty())
            {
                // first time of search
                int iMaxWeight = 0; 
                bool bFirstWeight = true;

                int iTempWeight = 0;
                List<TempStep> tempSteps = new List<TempStep>();
                for (int x = 0; x < FormMain.ChessBoardScale; x++)
                {
                    for (int y = 0; y < FormMain.ChessBoardScale; y++)
                    {
                        if (ChessMan[x, y] != 0) continue;

                        int minX = 0, minY = 0, maxX = 0, maxY = 0;
                        OpsOfChessman_Singleton.Instance.GetRange(x, y, 2, ref minX, ref minY, ref maxX, ref maxY);
                        bool bContinue = false;
                       
                        for (int xx = minX; xx < maxX; xx++)
                        {
                            for (int yy = minY; yy < maxY; yy++)
                            {
                                if (ChessMan[xx, yy] != 0)
                                {
                                    bContinue = true;
                                    break;
                                }
                            }
                            if (bContinue) break;
                        }

                        if (!bContinue) continue;

                        List<Point> lstTempPoints = new List<Point>();
                        if (OpsOfChessman_Singleton.Instance.getWeight_Peers(ChessMan, x, y, side, ref iTempWeight, ref lstTempPoints))
                        {
                            if (bFirstWeight || iTempWeight == iMaxWeight)
                            {
                                bFirstWeight = false;
                                iMaxWeight = iTempWeight;

                                TempStep tempStep = new TempStep(x, y);
                                for (int k = 0; k < lstTempPoints.Count; k++)
                                {
                                    List<Point> lstp = new List<Point>();
                                    lstp.Add(new Point(x, y));
                                    lstp.Add(new Point(lstTempPoints[k].X, lstTempPoints[k].Y));
                                    tempStep.lstPoints.Add(lstp);
                                }
                                tempSteps.Add(tempStep);
                            }
                            else if (iTempWeight > iMaxWeight)
                            {
                                iMaxWeight = iTempWeight;

                                tempSteps.Clear();
                                TempStep tempStep = new TempStep(x, y);
                                for (int k = 0; k < lstTempPoints.Count; k++)
                                {
                                    List<Point> lstp = new List<Point>();
                                    lstp.Add(new Point(x, y));
                                    lstp.Add(new Point(lstTempPoints[k].X, lstTempPoints[k].Y));
                                    tempStep.lstPoints.Add(lstp);
                                }
                                tempSteps.Add(tempStep);
                            }
                        }
                    }
                }


                List<TempStep> tempStepsBackup = new List<TempStep>();
                for (int i = 0; i < tempSteps.Count; i++)
                {
                    TempStep ts = new TempStep(0, 0);
                    ts.Copy(tempSteps[i]);
                    tempStepsBackup.Add(ts);
                }

                if (tempStepsBackup.Count > 1)
                {
                    tempSteps.Clear();

                    // second time of search
                    int iBestWeight = 0;
                    bool bFirstBestWeight = true;

                    for (int i = 0; i < tempStepsBackup.Count; i++)
                    {
                        int iMinWeight = 0;
                        bool bFirstMinWeight = true;

                        TempStep ts = tempStepsBackup[i];
                        for (int j = 0; j < ts.lstPoints.Count; j++)
                        {
                            List<Point> lsSteps = ts.lstPoints[j];
                            for (int k = 0; k < lsSteps.Count; k = k + 2)
                            {
                                ChessMan[lsSteps[k].X, lsSteps[k].Y] = side;
                                ChessMan[lsSteps[k + 1].X, lsSteps[k + 1].Y] = side * -1;
                            }
                            int iLocalBestWeight = 0;
                            bool bFirstLocalBestWeight = true;
                            for (int x = 0; x < FormMain.ChessBoardScale; x++)
                            {
                                for (int y = 0; y < FormMain.ChessBoardScale; y++)
                                {
                                    if (ChessMan[x, y] != 0) continue;

                                    int minX = 0, minY = 0, maxX = 0, maxY = 0;
                                    OpsOfChessman_Singleton.Instance.GetRange(x, y, 2, ref minX, ref minY, ref maxX, ref maxY);
                                    bool bContinue = false;

                                    for (int xx = minX; xx < maxX; xx++)
                                    {
                                        for (int yy = minY; yy < maxY; yy++)
                                        {
                                            if (ChessMan[xx, yy] != 0)
                                            {
                                                bContinue = true;
                                                break;
                                            }
                                        }
                                        if (bContinue) break;
                                    }

                                    if (!bContinue) continue;

                                    List<Point> lstTempPoints = new List<Point>();
                                    int iTWeight = 0;
                                    if (OpsOfChessman_Singleton.Instance.getWeight_Peers(ChessMan, x, y, side, ref iTWeight, ref lstTempPoints))
                                    {
                                        if (bFirstLocalBestWeight || iTWeight == iLocalBestWeight)
                                        {
                                            bFirstLocalBestWeight = false;
                                            iLocalBestWeight = iTWeight;
                                        }
                                        else if (iTWeight > iLocalBestWeight)
                                        {
                                            iLocalBestWeight = iTWeight;
                                        }
                                    }
                                }
                            }

                            if (bFirstMinWeight || iMinWeight > iLocalBestWeight)
                            {
                                bFirstMinWeight = false;
                                iMinWeight = iLocalBestWeight;
                            }

                            for (int k = 0; k < lsSteps.Count; k = k + 2)
                            {
                                ChessMan[lsSteps[k].X, lsSteps[k].Y] = 0;
                                ChessMan[lsSteps[k + 1].X, lsSteps[k + 1].Y] = 0;
                            }
                        }

                        if (bFirstBestWeight || iMinWeight == iBestWeight)
                        {
                            tempSteps.Add(new TempStep(tempStepsBackup[i].p.X, tempStepsBackup[i].p.Y));
                            bFirstBestWeight = false;
                            iBestWeight = iMinWeight;
                        }
                        else if (iMinWeight > iBestWeight)
                        {
                            tempSteps.Clear();
                            tempSteps.Add(new TempStep(tempStepsBackup[i].p.X, tempStepsBackup[i].p.Y));
                            iBestWeight = iMinWeight;
                        }
                    }
                }

                for (int i = 0; i < tempSteps.Count; i++)
                {
                    chessmanStore.storeChessmen(tempSteps[i].p.X, tempSteps[i].p.Y);
                }


            }


            if (bForthForbidden == 1)
            {
                // restore all chessman-server-forbidden position
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        if (ChessMan[i, j] == 2)
                            ChessMan[i, j] = 0;
                    }
                }
            }

            LogInfo.getInstance().WriteLogFlush("\n\nGetNextPos End\n\n");

            return chessmanStore.getPoint(bCheckFully);
        }
    }

    //====================
    //base strategy class
    //====================
    class Strategy
    {
        public enum PosStatus { Empty = 0, Black = 1, White = -1 }

        private int[,] chessMan;

        // constructor
        public Strategy(int [,] ChessMan)
        {
            chessMan = ChessMan;
        }

        public void setChessman(int[,] ChessMan)
        {
            this.chessMan = ChessMan;
        }

        virtual public void setMaxProbeStep(int iMaxProbeStep) { }

        // check how many chessmen connecting continuously - row
        //------------------------------------------------------
        private ChessmenStatus numContChessmen_row(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            if (value == 0)
            {
                // this must be a chessman-server-forbidden position
                return status;
            }

            // left side
            int cy = y - 1;
            while ((cy >= 0) && (chessMan[x, cy] == value))
            {
                status.numContChessmen++;
                cy--;
            }
            if ((cy >= 0) && (chessMan[x, cy] == 0))
            {
                status.numEndEmpty++;
            }

            // right side
            cy = y + 1;
            while ((cy < FormMain.ChessBoardScale) && (chessMan[x, cy] == value))
            {
                status.numContChessmen++;
                cy++;
            }
            if ((cy < FormMain.ChessBoardScale) && (chessMan[x, cy] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }

        // check how many chessmen connecting continuously - col
        //------------------------------------------------------
        private ChessmenStatus numContChessmen_col(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            while ((cx >= 0) && (chessMan[cx, y] == value))
            {
                status.numContChessmen++;
                cx--;
            }
            if ((cx >= 0) && (chessMan[cx, y] == 0))
            {
                status.numEndEmpty++;
            }

            // below
            cx = x + 1;
            while ((cx < FormMain.ChessBoardScale) && (chessMan[cx, y] == value))
            {
                status.numContChessmen++;
                cx++;
            }
            if ((cx < FormMain.ChessBoardScale) && (chessMan[cx, y] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }

        // check how many chessmen connecting continuously - backward flash
        //-----------------------------------------------------------------
        private ChessmenStatus numberContChessmen_bkcro(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            int cy = y - 1;
            while ((cx >= 0) && (cy >= 0) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx--;
                cy--;
            }
            if ((cx >= 0) && (cy >= 0) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            // below
            cx = x + 1;
            cy = y + 1;
            while ((cx < FormMain.ChessBoardScale) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx++;
                cy++;
            }
            if ((cx < FormMain.ChessBoardScale) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }

        // check how many chessmen connecting continuously - forward flash
        //----------------------------------------------------------------
        private ChessmenStatus numberContChessmen_fwcro(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            int cy = y + 1;
            while ((cx >= 0) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx--;
                cy++;
            }
            if ((cx >= 0) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            // below
            cx = x + 1;
            cy = y - 1;
            while ((cx < FormMain.ChessBoardScale) && (cy >= 0) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx++;
                cy--;
            }
            if ((cx < FormMain.ChessBoardScale) && (cy >= 0) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }

        //================================================================================
        // this is the most useful to detect chessman's status. 
        // parameters:
        //  continuousNum: how many chessmen are required to be put side by side
        //  emptEndNum: how many ends are required to be standalone (i.e. 0, 1, or 2)
        //  requireNum: how many the cases we need find
        //  isStrict: whether the "continuousNum" is required to be same with the first
        //            parameter exactly
        //  statusArray: the check direction, i.e. horizontal, vertical, and sidelong
        // return:
        //  true: we get the cases
        //  false: we don't get the case
        //=================================================================================
        private bool checkChessmanStatus(int continuousNum, int emptEndNum, int requireNum, bool isStrict,
                                         ChessmenStatus[] statusArray)
        {
            if (requireNum <= 0)
                return true;

            for (int i = 0; i < statusArray.Length; i++)
            {
                if (isStrict)
                {
                    if ((statusArray[i].numContChessmen == continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                            return true;
                    }
                }
                else
                {
                    if ((statusArray[i].numContChessmen >= continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        //-------------------------------------------------------------
        
        // is the position occupied
        public bool isOccupied(int x, int y)
        {
            if (chessMan[x, y] != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // is the position a chessman-server forbidden position?
        public bool isForthgoerForbidden(int x, int y, int side)
        {
            /*
            if (isOccupied(x, y))
            {
                return false;
            }

            bool result = false;

            if (side == -1)
            {
                chessMan[x, y] = side;

                ChessmenStatus[] status = new ChessmenStatus[4];
                status[0] = new ChessmenStatus();
                status[1] = new ChessmenStatus();
                status[2] = new ChessmenStatus();
                status[3] = new ChessmenStatus();

                status[0] = numContChessmen_row(x, y);
                status[1] = numContChessmen_col(x, y);
                status[2] = numberContChessmen_bkcro(x, y);
                status[3] = numberContChessmen_fwcro(x, y);

                // check double-live-3
                if (checkChessmanStatus(3, 2, 2, true, status))
                {
                    result = true;
                }

                // check double-4
                if (checkChessmanStatus(4, 0, 2, true, status))
                {
                    result = true;
                }

                // check long-link
                if (checkChessmanStatus(6, 0, 1, false, status))
                {
                    result = true;
                }

                chessMan[x, y] = 0;
            }
            
            return result;
            */
            return false;
        }

        public virtual  Point GetNextPos(int side, int bForthForbidden){
            // we should never call this function, as child class will overwrite it.
            System.Diagnostics.Debug.Assert(false);
            
            Point p = new Point(0, 0);
            return p;
        }
    }

    //====================================================
    // first Strategy. The implementation is by StrategyImp
    //====================================================
    class RawStrategy : Strategy
    {
        private StrategyImp strategyImpInst;
        public StrategyImp StragegyImpInst
        {
            get
            {
                return strategyImpInst;
            }
            set
            {
                strategyImpInst = value;
            }
        }

        private int[,] chessMan;

        // constructor
        //------------
        public RawStrategy(int[,] ChessMan)
            : base(ChessMan)
        {
            chessMan = ChessMan;
        }

        public override Point GetNextPos(int side, int bForthForbidden)
        {
            return strategyImpInst.GetNextPos(chessMan, side, bForthForbidden);
        }

        public override void setMaxProbeStep(int iMaxProbeStep) {
            strategyImpInst.setMaxProbeStep(iMaxProbeStep);
        }
    }

    public class OpsOfChessman_Singleton
    {
        private static OpsOfChessman_Singleton instance;
        private static int MAX_PROBE_STEPS = 6;

        private OpsOfChessman_Singleton() { }

        public static int GetMaxProbeSteps() {
            return MAX_PROBE_STEPS;
        }

        public static char ConvertX(int x)
        {
            char c = 'A';

            c += (char)x;
            return c; 
        }

        public static int ConvertY(int y)
        {
            return 15 - y;
        }

        public static int DeConvertX(char c)
        {
            char cc = 'A';
            return c - cc;
        }

        public static int DeConvertY(int i)
        {
            return 15 - i;
        }

        public struct PointInfo
        {
            public int continuousChessman; // how many continuous chessman
            public int emptyPosition;      // how many empty positions beside the last chessman
            public int closestFriend;      // how close is the first chessman of my side 
        }

        public static OpsOfChessman_Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OpsOfChessman_Singleton();
                }
                return instance;
            }
        }

        private int GetWeight_OneDirection(OpsOfChessman_Singleton.PointInfo pi1, OpsOfChessman_Singleton.PointInfo pi2)
        {
            int weight = 0;

            if (pi1.emptyPosition == 0 && pi2.emptyPosition == 0)
            {
                // double sides are blocked
                return 0;
            }

            if (pi1.emptyPosition == 0 || pi2.emptyPosition == 0)
            {
                // one side is blocked
                int positions = pi1.continuousChessman + pi2.continuousChessman;
                switch (positions)
                {
                    case 0:
                        if (pi1.emptyPosition + pi2.emptyPosition >= 4)
                            weight = 10;
                        else
                            weight = 0;
                        break;
                    case 1:
                        if (pi1.emptyPosition + pi2.emptyPosition >= 3)
                            weight = 11;
                        else
                            weight = 0;
                        break;
                    case 2:
                        if (pi1.emptyPosition + pi2.emptyPosition >= 2)
                            weight = 13;
                        else
                            weight = 0;
                        break;
                    case 3:
                        if (pi1.emptyPosition + pi2.emptyPosition >= 2)
                            weight = 16;
                        else
                            weight = 0;
                        break;
                    default:
                        // this case should fall in Win Point
                        break;
                }

                // at most only one side has closes friend
                int closestFriend = 0;
                if (pi1.closestFriend > 0) closestFriend = pi1.closestFriend;
                if (pi2.closestFriend > 0) closestFriend = pi2.closestFriend;
                switch (closestFriend)
                {
                    case 0:
                        // means no closest friend
                        break;
                    case 4:
                        weight++;
                        break;
                    case 3:
                        weight += 2;
                        break;
                    case 2:
                        weight += 3;
                        break;
                    case 1:
                        weight += 4;
                        break;
                    default:
                        // don't consider if the closest friend is too far
                        break;
                }
            }

            if (pi1.emptyPosition > 0 && pi2.emptyPosition > 0)
            {
                // both sides have free position
                int positions = pi1.continuousChessman + pi2.continuousChessman;
                switch (positions)
                {
                    case 0:
                        if (pi1.emptyPosition + pi2.emptyPosition >= 4)
                            weight = 12;
                        else
                            weight = 0;
                        break;
                    case 1:
                        if (pi1.emptyPosition + pi2.emptyPosition >= 3)
                            weight = 16;
                        else
                            weight = 0;
                        break;
                    case 2:
                        if (pi1.emptyPosition + pi2.emptyPosition >= 2)
                            weight = 33;
                        else
                            weight = 0;
                        break;
                    case 3:
                        if (pi1.emptyPosition + pi2.emptyPosition < 2)
                            weight = 0;
                        break;
                    default:
                        // this case should fall in Win Point
                        break;
                }

                int closestFriend = 0;
                if (pi1.closestFriend > 0) closestFriend = pi1.closestFriend;
                if (pi2.closestFriend > 0 && closestFriend > pi2.closestFriend) closestFriend = pi2.closestFriend;
                switch (closestFriend)
                {
                    case 0:
                        // means no closest friend
                        break;
                    case 4:
                        weight++;
                        break;
                    case 3:
                        weight += 2;
                        break;
                    case 2:
                        weight += 3;
                        break;
                    case 1:
                        weight += 4;
                        break;
                    default:
                        // don't consider if the closest friend is too far
                        break;
                }
            }

            return weight;
        }

        // gaona: we check number of active-two and active-three to get weight. and we also consider the peer's next step,
        // we return the worst case as the weight.
        public int GetWeight(int[,] ChessMan, int x, int y, int side)
        {
            int weight = 0;
            

            return weight;
        }

        // is the position occupied
        public bool isOccupied(int[,] chessMan, int x, int y)
        {
            if (chessMan[x, y] != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /*
        // check whether opponent must be forced to put a chessman if I hold it. and return value difference
        // if it is. 
        public bool IsForcePosition(int[,] ChessMan, int side, int x, int y, ref int diffValue)
        {
            if (isOccupied(ChessMan, x, y))
                return false;

            List<Point> lstCandidatePointsOfComp = new List<Point>();

            // cx, cy: means candidate x and y. if they are not '-1', they are the position opponent must hold
            int num = 0, xx = x, yy = y, cx = -1, cy = -1;
            bool hasOneEmpty_mid = false, hasOneEmpty_end = false;
            // check row
            while (--xx >= 0)
            {
                if (ChessMan[xx, yy] == side)
                    num++;
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else 
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break; 
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while (++xx < FormMain.ChessBoardScale)
            {
                if (ChessMan[xx, yy] == side)
                    num++;
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else 
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break; 
                    }
                }
                else
                {
                    break;
                }
            }
            if (num >= 3)
            {
                if (hasOneEmpty_mid || hasOneEmpty_end)
                {
                    // calculate value difference
                    ChessMan[x, y] = side;
                    ChessMan[cx, cy] = side * (-1);
                    int mw = GetWeight(ChessMan, x, y);
                    int pw = GetWeight(ChessMan, cx, cy);
                    ChessMan[cx, cy] = 0;
                    ChessMan[x, y] = 0;
                    diffValue = mw - pw;
                    return true;
                }
                else
                {
                    // not a candidate position
                }
            }

            // check col
            num = 0; xx = x; yy = y; cx = -1; cy = -1;
            hasOneEmpty_mid = false; hasOneEmpty_end = false;
            while (--yy >= 0)
            {
                if (ChessMan[xx, yy] == side)
                    num++;
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while (++yy < FormMain.ChessBoardScale)
            {
                if (ChessMan[xx, yy] == side)
                    num++;
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (num >= 3)
            {
                if (hasOneEmpty_mid || hasOneEmpty_end)
                {
                    // calculate value difference
                    ChessMan[x, y] = side;
                    ChessMan[cx, cy] = side * (-1);
                    int mw = GetWeight(ChessMan, x, y);
                    int pw = GetWeight(ChessMan, cx, cy);
                    ChessMan[cx, cy] = 0;
                    ChessMan[x, y] = 0;
                    diffValue = mw - pw;
                    return true;
                }
                else
                {
                    // not a candidate position
                }
            }

            // check forth-cross
            num = 0; xx = x; yy = y; cx = -1; cy = -1;
            hasOneEmpty_mid = false; hasOneEmpty_end = false;
            while ((--yy >= 0) && (++xx < FormMain.ChessBoardScale))
            {
                if (ChessMan[xx, yy] == side)
                {
                    num++;
                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while ((++yy < FormMain.ChessBoardScale) && (--xx >= 0))
            {
                if (ChessMan[xx, yy] == side)
                    num++;
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (num >= 3)
            {
                if (hasOneEmpty_mid || hasOneEmpty_end)
                {
                    // calculate value difference
                    ChessMan[x, y] = side;
                    ChessMan[cx, cy] = side * (-1);
                    int mw = GetWeight(ChessMan, x, y);
                    int pw = GetWeight(ChessMan, cx, cy);
                    ChessMan[cx, cy] = 0;
                    ChessMan[x, y] = 0;
                    diffValue = mw - pw;
                    return true;
                }
                else
                {
                    // not a candidate position
                }
            }

            // check back-cross
            num = 0; xx = x; yy = y; cx = -1; cy = -1;
            hasOneEmpty_mid = false; hasOneEmpty_end = false;
            while ((--yy >= 0) && (--xx >= 0))
            {
                if (ChessMan[xx, yy] == side)
                    num++;
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while ((++yy < FormMain.ChessBoardScale) && (++xx < FormMain.ChessBoardScale))
            {
                if (ChessMan[xx, yy] == side)
                    num++;
                else if (ChessMan[xx, yy] == 0)
                {
                    if (!hasOneEmpty_mid)
                    {
                        hasOneEmpty_mid = true;
                        cx = xx; cy = yy;
                        num++;
                    }
                    else
                    {
                        hasOneEmpty_end = true;
                        cx = xx; cy = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (num >= 3)
            {
                if (hasOneEmpty_mid || hasOneEmpty_end)
                {
                    // calculate value difference
                    ChessMan[x, y] = side;
                    ChessMan[cx, cy] = side * (-1);
                    int mw = GetWeight(ChessMan, x, y);
                    int pw = GetWeight(ChessMan, cx, cy);
                    ChessMan[cx, cy] = 0;
                    ChessMan[x, y] = 0;
                    diffValue = mw - pw;
                    return true;
                }
                else
                {
                    // not a candidate position
                }
            }

            return false;
        }
        */

        // check whether opponent must be forced to put a chessman if I hold it. and return value difference
        // if it is. 
        class ChessManInfo
        {
            public ChessManInfo()
            {
                bEmptyMid = bEmptyEnd = bEmptyEnd_2 = false;
                iContinuousNum = iContinuousNum_2 = iContinuousNum_3 = 0;
                x_end = y_end = x_mid = y_mid = x_end_2 = y_end_2 = 0;
            }

            public void Reset()
            {
                bEmptyMid = bEmptyEnd = bEmptyEnd_2 = false;
                iContinuousNum = iContinuousNum_2 = iContinuousNum_3 = 0;
                x_end = y_end = x_mid = y_mid = x_end_2 = y_end_2 = 0;
            }

            public bool bEmptyEnd_2, bEmptyEnd, bEmptyMid;
            public int iContinuousNum;
            public int iContinuousNum_2;
            public int iContinuousNum_3;
            public int x_end_2, y_end_2; 
            public int x_end, y_end;
            public int x_mid, y_mid;
        }
        private bool IsForceInfo(int[,] chessMan, int side, int x, int y, ChessManInfo chessManInfo1, ChessManInfo chessManInfo2, ref bool bFurtherStep, List<Point> lstForcedPoints)
        {
            // List<Point> lstPoints = new List<Point>();
            bFurtherStep = true;
            int num = chessManInfo1.iContinuousNum + chessManInfo2.iContinuousNum + 1;
            if (num == 4)
            {
                if (chessManInfo1.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo1.bEmptyEnd) {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_end, chessManInfo1.y_end));
                    bFurtherStep = false;
                }
                else if (chessManInfo2.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo2.bEmptyEnd)
                {
                    lstForcedPoints.Add(new Point(chessManInfo2.x_end, chessManInfo2.y_end));
                    bFurtherStep = false;
                }
            }
            else if (num == 3)
            {
                if (chessManInfo1.bEmptyMid && chessManInfo1.iContinuousNum_2 > 0)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo2.bEmptyMid && chessManInfo2.iContinuousNum_2 > 0)
                {
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo1.bEmptyMid && chessManInfo2.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                }
                else if (chessManInfo1.bEmptyMid && chessManInfo2.bEmptyEnd)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_end, chessManInfo2.y_end));
                }
                else if (chessManInfo1.bEmptyEnd && chessManInfo2.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_end, chessManInfo1.y_end));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                }
                else if (chessManInfo1.bEmptyEnd && chessManInfo2.bEmptyEnd)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_end, chessManInfo1.y_end));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_end, chessManInfo2.y_end));
                }
            }
            else if (num == 2)
            {
                if (chessManInfo1.bEmptyMid && chessManInfo1.iContinuousNum_2 >= 2)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo2.bEmptyMid && chessManInfo2.iContinuousNum_2 > 2)
                {
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo1.bEmptyMid && chessManInfo1.bEmptyEnd && chessManInfo1.iContinuousNum_2 == 1 && chessManInfo2.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    lstForcedPoints.Add(new Point(chessManInfo1.x_end, chessManInfo1.y_end));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                }
                else if (chessManInfo2.bEmptyMid && chessManInfo2.bEmptyEnd && chessManInfo2.iContinuousNum_2 == 1 && chessManInfo1.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_end, chessManInfo2.y_end));
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                }
            }
            else if (num == 1)
            {
                if (chessManInfo1.bEmptyMid && chessManInfo1.iContinuousNum_2 >= 3)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo2.bEmptyMid && chessManInfo2.iContinuousNum_2 >= 3)
                {
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                    bFurtherStep = false;
                }
                else if (chessManInfo1.bEmptyMid && chessManInfo1.bEmptyEnd && chessManInfo1.iContinuousNum_2 == 2 && chessManInfo2.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                    lstForcedPoints.Add(new Point(chessManInfo1.x_end, chessManInfo1.y_end));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                }
                else if (chessManInfo2.bEmptyMid && chessManInfo2.bEmptyEnd && chessManInfo2.iContinuousNum_2 == 2 && chessManInfo1.bEmptyMid)
                {
                    lstForcedPoints.Add(new Point(chessManInfo2.x_mid, chessManInfo2.y_mid));
                    lstForcedPoints.Add(new Point(chessManInfo2.x_end, chessManInfo2.y_end));
                    lstForcedPoints.Add(new Point(chessManInfo1.x_mid, chessManInfo1.y_mid));
                }
            }

            bool result = false;
            if (lstForcedPoints.Count > 0)
                result = true;

            return result;
        }

        private bool getChessManInfo(int[,] chessman, int side, int x, int y, ref List<ChessManInfo> chessManInfo_1, ref List<ChessManInfo> chessManInfo_2)
        {
            /**********************************/
            int xx = x, yy = y;
            ChessManInfo c1 = new ChessManInfo();
            ChessManInfo c2 = new ChessManInfo();
            // check row
            while (--xx >= 0)
            {
                if (chessman[xx, yy] == side)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.iContinuousNum++;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.iContinuousNum_2++;
                    }
                    else
                    {
                        c1.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.bEmptyMid = true;
                        c1.x_mid = xx;
                        c1.y_mid = yy;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.bEmptyEnd = true;
                        c1.x_end = xx;
                        c1.y_end = yy;
                    }
                    else
                    {
                        c1.bEmptyEnd_2 = true;
                        c1.x_end_2 = xx;
                        c1.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while (++xx < FormMain.ChessBoardScale)
            {
                if (chessman[xx, yy] == side)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.iContinuousNum++;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.iContinuousNum_2++;
                    }
                    else
                    {
                        c2.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.bEmptyMid = true;
                        c2.x_mid = xx;
                        c2.y_mid = yy;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.bEmptyEnd = true;
                        c2.x_end = xx;
                        c2.y_end = yy;
                    }
                    else
                    {
                        c2.bEmptyEnd_2 = true;
                        c2.x_end_2 = xx;
                        c2.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            chessManInfo_1.Add(c1);
            chessManInfo_2.Add(c2);

            // check col
            c1 = new ChessManInfo();
            c2 = new ChessManInfo();

            xx = x; yy = y;
            while (--yy >= 0)
            {
                if (chessman[xx, yy] == side)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.iContinuousNum++;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.iContinuousNum_2++;
                    }
                    else
                    {
                        c1.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.bEmptyMid = true;
                        c1.x_mid = xx;
                        c1.y_mid = yy;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.bEmptyEnd = true;
                        c1.x_end = xx;
                        c1.y_end = yy;
                    }
                    else
                    {
                        c1.bEmptyEnd_2 = true;
                        c1.x_end_2 = xx;
                        c1.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while (++yy < FormMain.ChessBoardScale)
            {
                if (chessman[xx, yy] == side)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.iContinuousNum++;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.iContinuousNum_2++;
                    }
                    else
                    {
                        c2.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.bEmptyMid = true;
                        c2.x_mid = xx;
                        c2.y_mid = yy;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.bEmptyEnd = true;
                        c2.x_end = xx;
                        c2.y_end = yy;
                    }
                    else
                    {
                        c2.bEmptyEnd_2 = true;
                        c2.x_end_2 = xx;
                        c2.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            chessManInfo_1.Add(c1);
            chessManInfo_2.Add(c2);

            // check forth-cross
            c1 = new ChessManInfo();
            c2 = new ChessManInfo();
            xx = x; yy = y;
            while ((--yy >= 0) && (++xx < FormMain.ChessBoardScale))
            {
                if (chessman[xx, yy] == side)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.iContinuousNum++;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.iContinuousNum_2++;
                    }
                    else
                    {
                        c1.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.bEmptyMid = true;
                        c1.x_mid = xx;
                        c1.y_mid = yy;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.bEmptyEnd = true;
                        c1.x_end = xx;
                        c1.y_end = yy;
                    }
                    else {
                        c1.bEmptyEnd_2 = true;
                        c1.x_end_2 = xx;
                        c1.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while ((++yy < FormMain.ChessBoardScale) && (--xx >= 0))
            {
                if (chessman[xx, yy] == side)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.iContinuousNum++;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.iContinuousNum_2++;
                    }
                    else if (c2.bEmptyEnd_2 == false)
                    {
                        c2.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.bEmptyMid = true;
                        c2.x_mid = xx;
                        c2.y_mid = yy;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.bEmptyEnd = true;
                        c2.x_end = xx;
                        c2.y_end = yy;
                    }
                    else
                    {
                        c2.bEmptyEnd_2 = true;
                        c2.x_end_2 = xx;
                        c2.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            chessManInfo_1.Add(c1);
            chessManInfo_2.Add(c2);

            // check back-cross
            c1 = new ChessManInfo();
            c2 = new ChessManInfo();
            xx = x; yy = y;
            while ((--yy >= 0) && (--xx >= 0))
            {
                if (chessman[xx, yy] == side)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.iContinuousNum++;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.iContinuousNum_2++;
                    }
                    else
                    {
                        c1.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c1.bEmptyMid == false)
                    {
                        c1.bEmptyMid = true;
                        c1.x_mid = xx;
                        c1.y_mid = yy;
                    }
                    else if (c1.bEmptyEnd == false)
                    {
                        c1.bEmptyEnd = true;
                        c1.x_end = xx;
                        c1.y_end = yy;
                    }
                    else
                    {
                        c1.bEmptyEnd_2 = true;
                        c1.x_end_2 = xx;
                        c1.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while ((++yy < FormMain.ChessBoardScale) && (++xx < FormMain.ChessBoardScale))
            {
                if (chessman[xx, yy] == side)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.iContinuousNum++;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.iContinuousNum_2++;
                    }
                    else
                    {
                        c2.iContinuousNum_3++;
                    }

                }
                else if (chessman[xx, yy] == 0)
                {
                    if (c2.bEmptyMid == false)
                    {
                        c2.bEmptyMid = true;
                        c2.x_mid = xx;
                        c2.y_mid = yy;
                    }
                    else if (c2.bEmptyEnd == false)
                    {
                        c2.bEmptyEnd = true;
                        c2.x_end = xx;
                        c2.y_end = yy;
                    }
                    else
                    {
                        c2.bEmptyEnd_2 = true;
                        c2.x_end_2 = xx;
                        c2.y_end_2 = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            chessManInfo_1.Add(c1);
            chessManInfo_2.Add(c2);
            /**********************************/

            return true;
        }

        public bool IsForcePosition(int[,] ChessMan, int side, int x, int y, ref bool furtherStep, List<Point> lstForcedPoints)
        {
            bool bFurtherStep = true, bTempFurtherStep = true, result = false;

            if (isOccupied(ChessMan, x, y))
            {
                return false;
            }

            // cx, cy: means candidate x and y. if they are not '-1', they are the position opponent must hold
            ChessManInfo chessManInfo_1 = new ChessManInfo(), chessManInfo_2 = new ChessManInfo();
            int xx = x, yy = y;

            // check row
            while (--xx >= 0)
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_1.iContinuousNum_2++;
                    }
                    
                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.bEmptyMid = true;
                        chessManInfo_1.x_mid = xx;
                        chessManInfo_1.y_mid = yy;
                    }
                    else
                    {
                        chessManInfo_1.bEmptyEnd = true;
                        chessManInfo_1.x_end = xx;
                        chessManInfo_1.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while (++xx < FormMain.ChessBoardScale)
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_2.iContinuousNum_2++;
                    }

                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.bEmptyMid = true;
                        chessManInfo_2.x_mid = xx;
                        chessManInfo_2.y_mid = yy;
                    }
                    else
                    {
                        chessManInfo_2.bEmptyEnd = true;
                        chessManInfo_2.x_end = xx;
                        chessManInfo_2.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (IsForceInfo(ChessMan, side, x, y, chessManInfo_1, chessManInfo_2, ref bTempFurtherStep, lstForcedPoints))
            {
                result = true;
                if (bTempFurtherStep == false)
                    bFurtherStep = false;
            }

            // check col
            chessManInfo_1.Reset(); chessManInfo_2.Reset();
            xx = x; yy = y;
            while (--yy >= 0)
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_1.iContinuousNum_2++;
                    }

                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.bEmptyMid = true;
                        chessManInfo_1.x_mid = xx;
                        chessManInfo_1.y_mid = yy;
                    }
                    else
                    {
                        chessManInfo_1.bEmptyEnd = true;
                        chessManInfo_1.x_end = xx;
                        chessManInfo_1.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while (++yy < FormMain.ChessBoardScale)
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_2.iContinuousNum_2++;
                    }

                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.bEmptyMid = true;
                        chessManInfo_2.x_mid = xx;
                        chessManInfo_2.y_mid = yy;
                        break;
                    }
                    else
                    {
                        chessManInfo_2.bEmptyEnd = true;
                        chessManInfo_2.x_end = xx;
                        chessManInfo_2.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (IsForceInfo(ChessMan, side, x, y, chessManInfo_1, chessManInfo_2, ref bTempFurtherStep, lstForcedPoints))
            {
                result = true;
                if (bTempFurtherStep == false)
                    bFurtherStep = false;
            }

            // check forth-cross
            chessManInfo_1.Reset(); chessManInfo_2.Reset();
            xx = x; yy = y;
            while ((--yy >= 0) && (++xx < FormMain.ChessBoardScale))
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_1.iContinuousNum_2++;
                    }

                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.bEmptyMid = true;
                        chessManInfo_1.x_mid = xx;
                        chessManInfo_1.y_mid = yy;
                    }
                    else
                    {
                        chessManInfo_1.bEmptyEnd = true;
                        chessManInfo_1.x_end = xx;
                        chessManInfo_1.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while ((++yy < FormMain.ChessBoardScale) && (--xx >= 0))
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_2.iContinuousNum_2++;
                    }

                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.bEmptyMid = true;
                        chessManInfo_2.x_mid = xx;
                        chessManInfo_2.y_mid = yy;
                    }
                    else
                    {
                        chessManInfo_2.bEmptyEnd = true;
                        chessManInfo_2.x_end = xx;
                        chessManInfo_2.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (IsForceInfo(ChessMan, side, x, y, chessManInfo_1, chessManInfo_2, ref bTempFurtherStep, lstForcedPoints))
            {
                result = true;
                if (bTempFurtherStep == false)
                    bFurtherStep = false;
            }

            // check back-cross
            chessManInfo_1.Reset(); chessManInfo_2.Reset();
            xx = x; yy = y;
            while ((--yy >= 0) && (--xx >= 0))
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_1.iContinuousNum_2++;
                    }

                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_1.bEmptyMid == false)
                    {
                        chessManInfo_1.bEmptyMid = true;
                        chessManInfo_1.x_mid = xx;
                        chessManInfo_1.y_mid = yy;
                    }
                    else
                    {
                        chessManInfo_1.bEmptyEnd = true;
                        chessManInfo_1.x_end = xx;
                        chessManInfo_1.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            xx = x; yy = y;
            while ((++yy < FormMain.ChessBoardScale) && (++xx < FormMain.ChessBoardScale))
            {
                if (ChessMan[xx, yy] == side)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.iContinuousNum++;
                    }
                    else
                    {
                        chessManInfo_2.iContinuousNum_2++;
                    }

                }
                else if (ChessMan[xx, yy] == 0)
                {
                    if (chessManInfo_2.bEmptyMid == false)
                    {
                        chessManInfo_2.bEmptyMid = true;
                        chessManInfo_2.x_mid = xx;
                        chessManInfo_2.y_mid = yy;
                    }
                    else
                    {
                        chessManInfo_2.bEmptyEnd = true;
                        chessManInfo_2.x_end = xx;
                        chessManInfo_2.y_end = yy;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (IsForceInfo(ChessMan, side, x, y, chessManInfo_1, chessManInfo_2, ref bTempFurtherStep, lstForcedPoints))
            {
                result = true;
                if (bTempFurtherStep == false)
                    bFurtherStep = false;
            }

            furtherStep = bFurtherStep;

            return result;
        }

        int getWeight_OneDirection(ChessManInfo c1, ChessManInfo c2)
        {
            int f = 0;

            if ((c1.iContinuousNum + c2.iContinuousNum) == 3)
            {
                // x...._   15
                if (c1.bEmptyMid || c2.bEmptyMid)
                {
                    f += 15;
                }
            }
            
            if ((c1.iContinuousNum + c2.iContinuousNum) == 2)
            {
                // x..._.   15
                // x...__   13
                // x_...__  14
                // ._...__  23
                // __...__  22
                if (!c1.bEmptyMid)
                {
                    if (c2.bEmptyMid && c2.iContinuousNum_2 > 0)
                        f += 15;
                    else if (c2.bEmptyMid && c2.bEmptyEnd)
                        f += 15;
                }
                else if (!c2.bEmptyMid)
                {
                    if (c1.bEmptyMid && c1.iContinuousNum_2 > 0)
                        f += 15;
                    else if (c1.bEmptyMid && c1.bEmptyEnd)
                        f += 15;
                }
                else if (c1.bEmptyMid && !c1.bEmptyEnd)
                {
                    if (c2.bEmptyMid && c2.bEmptyEnd)
                        f += 13;
                }
                else if (c2.bEmptyMid && !c2.bEmptyEnd)
                {
                    if (c1.bEmptyMid && c1.bEmptyEnd)
                        f += 13;
                }
                else if (c1.bEmptyMid && c1.iContinuousNum_2 > 0 && c2.bEmptyMid && c2.bEmptyEnd)
                    f += 23;
                else if (c1.bEmptyMid && c1.bEmptyEnd && c2.bEmptyMid && c2.bEmptyEnd)
                    f += 22;
            }
            
            if ((c1.iContinuousNum + c2.iContinuousNum) == 1)
            {
                // .._..    15
                // _..___   12
                // __..__   13
                if (c1.bEmptyMid && c1.iContinuousNum_2 == 2)
                    f += 15;
                else if (c2.bEmptyMid && c2.iContinuousNum_2 == 2)
                    f += 15;
                else if (c1.bEmptyMid && c1.iContinuousNum_2 > 2)
                    f += 23;
                else if (c2.bEmptyMid && c2.iContinuousNum_2 > 2)
                    f += 23;
                else if (c1.bEmptyMid && c2.bEmptyMid && c2.bEmptyEnd && c2.bEmptyEnd_2)
                    f += 12;
                else if (c2.bEmptyMid && c1.bEmptyMid && c1.bEmptyEnd && c1.bEmptyEnd_2)
                    f += 12;
                else if (c1.bEmptyMid && c1.bEmptyEnd && c2.bEmptyMid && c2.bEmptyEnd)
                    f += 13;
            }

            if ((c1.iContinuousNum + c2.iContinuousNum) == 0)
            {
                // x..._.   15
                // __..._.  23
                // _._.__   10
                // _.__._   10
                if (c1.bEmptyMid && c1.iContinuousNum_2 >= 3)
                {
                    if (c1.bEmptyEnd && c1.bEmptyEnd_2)
                        f += 23;
                    else
                        f += 15;
                }
                else if (c2.bEmptyMid && c2.iContinuousNum_2 >= 3)
                {
                    if (c2.bEmptyEnd && c2.bEmptyEnd_2)
                        f += 23;
                    else
                        f += 15;
                }
                else if (c1.bEmptyMid && c2.bEmptyMid && c2.bEmptyEnd && c2.bEmptyEnd_2 && (c2.iContinuousNum_2 >= 1 || c2.iContinuousNum_3 >= 1))
                    f += 10;
                else if (c2.bEmptyMid && c1.bEmptyMid && c1.bEmptyEnd && c1.bEmptyEnd_2 && (c1.iContinuousNum_2 >= 1 || c1.iContinuousNum_3 >= 1))
                    f += 10;
            }

            if (c1.iContinuousNum > 0)
                f += 1;
            if (c2.iContinuousNum > 0)
                f += 1;

            return f;
        }

        int getWeight_OneSide(List<ChessManInfo> c1, List<ChessManInfo> c2)
        {
            int iWeight = 0; 
            for (int i = 0; i < c1.Count; i++)
            {
                iWeight += getWeight_OneDirection(c1[i], c2[i]);
            }

            return iWeight;
        }
        
        public void GetRange(int baseX, int baseY, int range,
            ref int minX, ref int minY, ref int maxX, ref int maxY)
        {
            minX = baseX - range;
            if (minX < 0) minX = 0;

            minY = baseY - range;
            if (minY < 0) minY = 0;

            maxX = baseX + range + 1;
            if (maxX > FormMain.ChessBoardScale) maxX = FormMain.ChessBoardScale;

            maxY = baseY + range + 1;
            if (maxY > FormMain.ChessBoardScale) maxY = FormMain.ChessBoardScale;
        }

        // return value is my weight minus peer weight
        public bool getWeight_Peers(int[,] chessman, int my_x, int my_y, int side, ref int iWeight, ref List<Point> lstPoints)
        {
            lstPoints.Clear();
            if (chessman[my_x, my_y] != 0)
            {
                return false; 
            }

            int iWorstWeight = 0;
            bool bFirstWeight = true; 
            //////////////////////////////////////////////////////////////////////////

            // is there a forced position for peer?
            List<Point> lstForcedPoints = new List<Point>();
            bool bTemp = false;
            if (OpsOfChessman_Singleton.Instance.IsForcePosition(chessman, side, my_x, my_y, ref bTemp, lstForcedPoints))
            {
                chessman[my_x, my_y] = side;
                for (int i = 0; i < lstForcedPoints.Count; i++)
                {
                    List<ChessManInfo> c1 = new List<ChessManInfo>(), c2 = new List<ChessManInfo>();
                    int iTempWeight = 0; 
                    chessman[lstForcedPoints[i].X, lstForcedPoints[i].Y] = side * -1;
                    if (getChessManInfo(chessman, side, my_x, my_y, ref c1, ref c2))
                    {
                        iTempWeight = getWeight_OneSide(c1, c2);
                    }

                    c1.Clear(); c2.Clear();
                    if (getChessManInfo(chessman, side * -1, lstForcedPoints[i].X, lstForcedPoints[i].Y, ref c1, ref c2))
                    {
                        iTempWeight -= getWeight_OneSide(c1, c2);
                    }
                    chessman[lstForcedPoints[i].X, lstForcedPoints[i].Y] = 0;

                    if (bFirstWeight || iTempWeight < iWeight)
                    {
                        lstPoints.Clear();
                        lstPoints.Add(new Point(lstForcedPoints[i].X, lstForcedPoints[i].Y));
                        iWeight = iTempWeight;
                        bFirstWeight = false;
                    }
                    else if (iTempWeight == iWeight)
                    {
                        lstPoints.Add(new Point(lstForcedPoints[i].X, lstForcedPoints[i].Y));
                    }
                }
                chessman[my_x, my_y] = 0;
            }


            // means no forced position
            if (lstForcedPoints.Count == 0)
            {
                chessman[my_x, my_y] = side;
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        if (isOccupied(chessman, i, j))
                            continue;

                        chessman[i, j] = (side * -1);
                        int iTempWeight = 0;

                        /********************************************************/
                        List<ChessManInfo> c1 = new List<ChessManInfo>(), c2 = new List<ChessManInfo>();

                        if (getChessManInfo(chessman, side, my_x, my_y, ref c1, ref c2))
                        {
                            iTempWeight = getWeight_OneSide(c1, c2);
                        }

                        c1.Clear(); c2.Clear();
                        if (getChessManInfo(chessman, side * -1, i, j, ref c1, ref c2))
                        {
                            iTempWeight -= getWeight_OneSide(c1, c2);
                        }

                        /********************************************************/

                        if (iTempWeight < iWorstWeight || bFirstWeight)
                        {
                            iWorstWeight = iTempWeight;
                            bFirstWeight = false;
                            lstPoints.Clear();
                            lstPoints.Add(new Point(i, j));
                        }
                        else if (iTempWeight == iWorstWeight)
                        {
                            lstPoints.Add(new Point(i, j));
                        }

                        chessman[i, j] = 0;
                    }
                chessman[my_x, my_y] = 0;
            }

            //////////////////////////////////////////////////////////////////////////
            iWeight = iWorstWeight;

            return true; 
        }

        /*
        public bool getClosestEmpty_row(int[,] chessman, int x, int y, int side, ref Point point)
        {
            int xx = x, yy = y;
            while (--xx >= 0)
            {
                if (chessman[xx, yy] == 0)
                {
                    point.X = xx;
                    point.Y = yy;
                    return true;
                }
                if (chessman[xx, yy] != side)
                    return false;
            }
            xx = x; yy = y;
            while (++xx < FormMain.ChessBoardScale)
            {
                if (chessman[xx, yy] == 0)
                {
                    point.X = xx;
                    point.Y = yy;
                    return true;
                }
                if (chessman[xx, yy] != side)
                    return false;
            }
            return false;
        }
        */

        public List<PointInfo> getPointInfo_row(int[,] chessMan, int x, int y, int side)
        {
            List<PointInfo> lstPointInfo = new List<PointInfo>();

            PointInfo pi = new PointInfo();
            int cs = 0, ep = 0, cf = 0, xx = x, yy = y; 

            // check left
            while ((--xx >= 0) && chessMan[xx, yy] == side)
                cs++;
            while ((xx >= 0) && (chessMan[xx, yy] == 0 || chessMan[xx, yy] == side))
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                xx--;
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            pi = new PointInfo();
            cs = 0; ep = 0; cf = 0;  xx = x; yy = y;
            //check right
            while ((++xx < FormMain.ChessBoardScale) && chessMan[xx, yy] == side)
                cs++;
            while ((xx < FormMain.ChessBoardScale) && (chessMan[xx, yy] == 0  || chessMan[xx, yy] == side))
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                xx++;
                
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            return lstPointInfo;
        }

        public List<PointInfo> getPointInfo_col(int[,] chessMan, int x, int y, int side)
        {
            List<PointInfo> lstPointInfo = new List<PointInfo>();

            PointInfo pi = new PointInfo();
            int cs = 0, ep = 0, cf = 0, xx = x, yy = y;

            // check up
            while ((--yy >= 0) && chessMan[xx, yy] == side)
                cs++;
            while ((yy >= 0) && (chessMan[xx, yy] == 0 || chessMan[xx, yy] == side))
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                yy--;
                
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            pi = new PointInfo();
            cs = 0; ep = 0; cf = 0;  xx = x; yy = y;
            //check down
            while ((++yy < FormMain.ChessBoardScale) && chessMan[xx, yy] == side)
                cs++;
            while ((yy < FormMain.ChessBoardScale) && (chessMan[xx, yy] == 0  || chessMan[xx, yy] == side))
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                yy++;
                
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            return lstPointInfo;
        }

        public List<PointInfo> getPointInfo_bkcro(int[,] chessMan, int x, int y, int side)
        {
            List<PointInfo> lstPointInfo = new List<PointInfo>();

            PointInfo pi = new PointInfo();
            int cs = 0, ep = 0, cf = 0, xx = x, yy = y;

            // check left and up
            while ((--xx >= 0) && (--yy >= 0) && chessMan[xx, yy] == side)
                cs++;
            while ((xx >= 0) && (yy >= 0) && (chessMan[xx, yy] == 0  || chessMan[xx, yy] == side))
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                xx--;
                yy--;
                
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            pi = new PointInfo();
            cs = 0; ep = 0; cf = 0;  xx = x; yy = y;
            //check right and down
            while ((++xx < FormMain.ChessBoardScale) && (++yy < FormMain.ChessBoardScale) && chessMan[xx, yy] == side)
                cs++;
            while ((xx < FormMain.ChessBoardScale) && (yy < FormMain.ChessBoardScale) && (chessMan[xx, yy] == 0  || chessMan[xx, yy] == side))
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                xx++;
                yy++;
                
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            return lstPointInfo;
        }

        public List<PointInfo> getPointInfo_fwcro(int[,] chessMan, int x, int y, int side)
        {
            List<PointInfo> lstPointInfo = new List<PointInfo>();

            PointInfo pi = new PointInfo();
            int cs = 0, ep = 0, cf = 0, xx = x, yy = y;

            // check right and up
            while ((--yy >= 0) && (++xx < FormMain.ChessBoardScale) && chessMan[xx, yy] == side)
                cs++;
            while ((yy >= 0) && (xx < FormMain.ChessBoardScale) && (chessMan[xx, yy] == 0  || chessMan[xx, yy] == side))
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                yy--;
                xx++;
                
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            pi = new PointInfo();
            cs = 0; ep = 0; cf = 0;  xx = x; yy = y;
            //check left and down
            while ((++yy < FormMain.ChessBoardScale) && (--xx >= 0) && chessMan[xx, yy] == side)
                cs++;
            while ((yy < FormMain.ChessBoardScale) && (xx >= 0) && chessMan[xx, yy] == 0)
            {
                if (cf == 0 && chessMan[xx, yy] != side)
                {
                    cf++;
                }
                ep++;
                yy++;
                xx--;
                
            }
            pi.continuousChessman = cs;
            pi.emptyPosition = ep;
            pi.closestFriend = cf;
            lstPointInfo.Add(pi);

            return lstPointInfo;
        }

        // check how many chessmen connecting continuously
        //-------------------------------------------------
        public ChessmenStatus numContChessmen_row(int[,] chessMan, int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            if (value == 0)
            {
                // this must be a forthgoer-forbidden position
                return status;
            }

            // left side
            int cy = y - 1;
            while ((cy >= 0) && (chessMan[x, cy] == value))
            {
                status.numContChessmen++;
                cy--;
            }
            if ((cy >= 0) && (chessMan[x, cy] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(x, cy));
            }

            // right side
            cy = y + 1;
            while ((cy < FormMain.ChessBoardScale) && (chessMan[x, cy] == value))
            {
                status.numContChessmen++;
                cy++;
            }
            if ((cy < FormMain.ChessBoardScale) && (chessMan[x, cy] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(x, cy));
            }

            return status;
        }

        public ChessmenStatus numContChessmen_col(int[,] chessMan, int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            while ((cx >= 0) && (chessMan[cx, y] == value))
            {
                status.numContChessmen++;
                cx--;
            }
            if ((cx >= 0) && (chessMan[cx, y] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(cx, y));
            }

            // below
            cx = x + 1;
            while ((cx < FormMain.ChessBoardScale) && (chessMan[cx, y] == value))
            {
                status.numContChessmen++;
                cx++;
            }
            if ((cx < FormMain.ChessBoardScale) && (chessMan[cx, y] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(cx, y));
            }

            return status;
        }

        public ChessmenStatus numberContChessmen_bkcro(int[,] chessMan, int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            int cy = y - 1;
            while ((cx >= 0) && (cy >= 0) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx--;
                cy--;
            }
            if ((cx >= 0) && (cy >= 0) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(cx, cy));
            }

            // below
            cx = x + 1;
            cy = y + 1;
            while ((cx < FormMain.ChessBoardScale) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx++;
                cy++;
            }
            if ((cx < FormMain.ChessBoardScale) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(cx, cy));
            }

            return status;
        }

        public ChessmenStatus numberContChessmen_fwcro(int[,] chessMan, int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            int cy = y + 1;
            while ((cx >= 0) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx--;
                cy++;
            }
            if ((cx >= 0) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(cx, cy));
            }

            // below
            cx = x + 1;
            cy = y - 1;
            while ((cx < FormMain.ChessBoardScale) && (cy >= 0) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx++;
                cy--;
            }
            if ((cx < FormMain.ChessBoardScale) && (cy >= 0) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
                status.lstEPs.Add(new Point(cx, cy));
            }

            return status;
        }

        //================================================================================
        // this is the most useful to detect chessman's status. 
        // parameters:
        //  continuousNum: how many chessmen are required to be put side by side
        //  emptEndNum: how many ends are required to be standalone (i.e. 0, 1, or 2)
        //  requireNum: how many the cases we need find
        //  isStrict: whether the "continuousNum" is required to be same with the first
        //            parameter exactly
        //  statusArray: the check direction, i.e. horizontal, vertical, and sidelong
        // return:
        //  true: we get the cases
        //  false: we don't get the case
        //=================================================================================
        public bool checkChessmanStatus(int continuousNum, int emptEndNum, int requireNum, bool isStrict,
                                         ChessmenStatus[] statusArray)
        {
            if (requireNum <= 0)
                return true;

            for (int i = 0; i < statusArray.Length; i++)
            {
                if (isStrict)
                {
                    if ((statusArray[i].numContChessmen == continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                            return true;
                    }
                }
                else
                {
                    if ((statusArray[i].numContChessmen >= continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        public bool checkChessmanStatus_ReturnEP(int continuousNum, int emptEndNum, int requireNum, bool isStrict,
                                         ChessmenStatus[] statusArray, ref List<Point> lstEPs)
        {
            if (requireNum <= 0)
                return true;

            bool result = false; 

            for (int i = 0; i < statusArray.Length; i++)
            {
                if (isStrict)
                {
                    if ((statusArray[i].numContChessmen == continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                        {
                            lstEPs.AddRange(statusArray[i].lstEPs);
                            result = true;
                        }
                    }
                }
                else
                {
                    if ((statusArray[i].numContChessmen >= continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                        {
                            lstEPs.AddRange(statusArray[i].lstEPs);
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        // is the position a chessman-server forbidden position?
        public bool isForthgoerForbidden(int[,] chessMan, int x, int y, int side)
        {
            /*
            if (isOccupied(chessMan, x, y))
            {
                return false;
            }

            bool result = false;

            if (side == -1)
            {
                chessMan[x, y] = side;

                ChessmenStatus[] status = new ChessmenStatus[4];
                status[0] = new ChessmenStatus();
                status[1] = new ChessmenStatus();
                status[2] = new ChessmenStatus();
                status[3] = new ChessmenStatus();

                status[0] = numContChessmen_row(chessMan, x, y);
                status[1] = numContChessmen_col(chessMan, x, y);
                status[2] = numberContChessmen_bkcro(chessMan, x, y);
                status[3] = numberContChessmen_fwcro(chessMan, x, y);

                // check double-live-3
                if (checkChessmanStatus(3, 2, 2, true, status))
                {
                    result = true;
                }

                // check double-4
                if (checkChessmanStatus(4, 0, 2, true, status))
                {
                    result = true;
                }

                // check long-link
                if (checkChessmanStatus(6, 0, 1, false, status))
                {
                    result = true;
                }

                chessMan[x, y] = 0;
            }

            return result;
            */
            return false;
        }

        public bool IsWinPoint(int[,] ChessMan, int side, int x, int y)
        {
            // suppose if my op hold this position to see the result
            if (ChessMan[x, y] != 0)
            {
                return false;
            }

            ChessMan[x, y] = side;

            ChessmenStatus[] status = new ChessmenStatus[4];
            status[0] = new ChessmenStatus();
            status[1] = new ChessmenStatus();
            status[2] = new ChessmenStatus();
            status[3] = new ChessmenStatus();

            status[0] = OpsOfChessman_Singleton.Instance.numContChessmen_row(ChessMan, x, y);
            status[1] = OpsOfChessman_Singleton.Instance.numContChessmen_col(ChessMan, x, y);
            status[2] = OpsOfChessman_Singleton.Instance.numberContChessmen_bkcro(ChessMan, x, y);
            status[3] = OpsOfChessman_Singleton.Instance.numberContChessmen_fwcro(ChessMan, x, y);

            ChessMan[x, y] = 0;

            if (OpsOfChessman_Singleton.Instance.checkChessmanStatus(5, 0, 1, false, status))
            {
                return true;
            }

            return false;
        }
        public bool IsLOSSPoint(int[,] ChessMan, int side, int x, int y)
        {
            // suppose if peer hold this position to see the result
            if (ChessMan[x, y] != 0)
            {
                return false;
            }

            ChessMan[x, y] = side * (-1);

            ChessmenStatus[] status = new ChessmenStatus[4];
            status[0] = new ChessmenStatus();
            status[1] = new ChessmenStatus();
            status[2] = new ChessmenStatus();
            status[3] = new ChessmenStatus();

            status[0] = OpsOfChessman_Singleton.Instance.numContChessmen_row(ChessMan, x, y);
            status[1] = OpsOfChessman_Singleton.Instance.numContChessmen_col(ChessMan, x, y);
            status[2] = OpsOfChessman_Singleton.Instance.numberContChessmen_bkcro(ChessMan, x, y);
            status[3] = OpsOfChessman_Singleton.Instance.numberContChessmen_fwcro(ChessMan, x, y);

            ChessMan[x, y] = 0;

            if (OpsOfChessman_Singleton.Instance.checkChessmanStatus(5, 0, 1, false, status))
            {
                return true;
            }

            return false;
        }

        private bool haveWinPoint(int[,] chessMan, int side)
        {
            for (int i = 0; i < FormMain.ChessBoardScale; i++)
                for (int j = 0; j < FormMain.ChessBoardScale; j++)
                {
                    if (IsWinPoint(chessMan, side, i, j))
                        return true;
                }

            return false;
        }

        private bool GetNextPoint(int baseX, int baseY, int iDirection, ref int x, ref int y,
            int iMaxX, int iMaxY)
        {
            if (iDirection == 0)
            {
                // means horizontal
                if (x + 1 < iMaxX)
                {
                    x = x + 1;
                    return true;
                }
            }
            else if (iDirection == 1)
            {
                // means vertical
                if (y + 1 < iMaxY)
                {
                    y = y + 1;
                    return true;
                }
            }
            else if (iDirection == 2)
            {
                // means left-up
                if ((x + 1 < iMaxX) && (y + 1 < iMaxY))
                {
                    x += 1;
                    y += 1;
                    return true;
                }
            }
            else if (iDirection == 3)
            {
                // means up-right
                if ((x > 0) && (y + 1 < iMaxY))
                {
                    x -= 1;
                    y += 1;
                    return true;
                }
            }

            return false;
        }

        // iRealProbeSteps is only meaningful when probe successfully
        public bool IsSemiWinPoint(int[,] ChessMan, int side, int x, int y, int probeSteps, ref int requiredSteps, 
            int maxProbeRange_P, int maxProbeRange_I, ref int iRealProbeSteps)
        {
            if ((probeSteps == 0) || (ChessMan[x, y] != 0))
            {
                // stop probing
                return false;
            }

            // check there are any chessmen within range of 2 grids. 
            // if not, it cannot be a semi-win point. 
            int minX = 0, minY = 0, maxX = 0, maxY = 0;
            bool go_on_Checking = false;
            GetRange(x, y, 2, ref minX, ref minY, ref maxX, ref maxY);
            for (int i = minX; i < maxX; i++)
            {
                for (int j = minY; j < maxY; j++)
                {
                    if (ChessMan[i, j] != side)
                    {
                        go_on_Checking = true;
                        break;
                    }
                }
                if (go_on_Checking)
                    break;
            }
            if (!go_on_Checking)
                return false;

            if (IsWinPoint(ChessMan, side, x, y))
            {
                requiredSteps++;
                iRealProbeSteps++;
                return true;
            }

            if (probeSteps == 1)
            {
                // no chance to probe other points
                return false;
            }

            bool bFurtherStep = false;
            List<Point> lstForcedPoints = new List<Point>();
            if (!OpsOfChessman_Singleton.Instance.IsForcePosition(ChessMan, side, x, y, ref bFurtherStep, lstForcedPoints))
                return false;

            if (OpsOfChessman_Singleton.GetMaxProbeSteps() == probeSteps)
            {
                LogInfo.getInstance().WriteLogFlush("\n\n********* Semi Win **********\n");
            }
            string str = "";
            for (int ii = 0; ii < (OpsOfChessman_Singleton.GetMaxProbeSteps() - probeSteps); ii++)
                str += "    ";
            LogInfo.getInstance().WriteLogFlush(str + "Detect: (" + OpsOfChessman_Singleton.ConvertX(x) + ", " + OpsOfChessman_Singleton.ConvertY(y) + ")");

            // suppose I hold it
            ChessMan[x, y] = side;

            // if I can find a Win or semi-Win point no matter what is opponent's next step, 
            // it is a semi-Win point
            int minFurtherSteps = 0;
            int peerSide = side * (-1);

            for (int idx=0; idx<lstForcedPoints.Count; idx++)
            {
                int i = lstForcedPoints[idx].X;
                int j = lstForcedPoints[idx].Y;
                if (ChessMan[i, j] == 0)
                {
                    ChessMan[i, j] = peerSide;

                    // let's check whether I am forced to put chessman somewhere?
                    ChessmenStatus[] status = new ChessmenStatus[4];
                    status[0] = new ChessmenStatus();
                    status[1] = new ChessmenStatus();
                    status[2] = new ChessmenStatus();
                    status[3] = new ChessmenStatus();

                    status[0] = OpsOfChessman_Singleton.Instance.numContChessmen_row(ChessMan, i, j);
                    status[1] = OpsOfChessman_Singleton.Instance.numContChessmen_col(ChessMan, i, j);
                    status[2] = OpsOfChessman_Singleton.Instance.numberContChessmen_bkcro(ChessMan, i, j);
                    status[3] = OpsOfChessman_Singleton.Instance.numberContChessmen_fwcro(ChessMan, i, j);

                    List<Point> lstForcedPointsByPeer = new List<Point>();
                    bool bPeerForcedPoint = false;
                    if (OpsOfChessman_Singleton.Instance.checkChessmanStatus_ReturnEP(4, 1, 1, false, status, ref lstForcedPointsByPeer))
                    {
                        bPeerForcedPoint = true;
                    }

                    // can I find a Win point or a semi-Win point?
                    bool findWinPoint = false;
                    if (!bPeerForcedPoint)
                    {
                        int minXI = 0, minYI = 0, maxXI = 0, maxYI = 0;
                        GetRange(x, y, maxProbeRange_I, ref minXI, ref minYI, ref maxXI, ref maxYI);
                        
                        for (int ii = minXI; ii < maxXI; ii++)
                        {
                            for (int jj = minYI; jj < maxYI; jj++)
                            {
                                int steps = 0;
                                if (IsSemiWinPoint(ChessMan, side, ii, jj, (probeSteps - 1), ref steps, maxProbeRange_P, maxProbeRange_I, ref iRealProbeSteps))
                                {
                                    if ((minFurtherSteps == 0) || (steps < minFurtherSteps))
                                        minFurtherSteps = steps;
                                    findWinPoint = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        // we can only select from the list forced by peers
                        for (int ii = 0; ii < lstForcedPointsByPeer.Count; ii++)
                        {
                            int steps = 0;
                            if (IsSemiWinPoint(ChessMan, side, lstForcedPointsByPeer[ii].X, lstForcedPointsByPeer[ii].Y, (probeSteps - 1), ref steps, maxProbeRange_P, maxProbeRange_I, ref iRealProbeSteps))
                            {
                                if ((minFurtherSteps == 0) || (steps < minFurtherSteps))
                                    minFurtherSteps = steps;
                                findWinPoint = true;
                            }
                        }
                    }

                    ChessMan[i, j] = 0;

                    if (!findWinPoint)
                    {
                        // I cannot find a Win point
                        ChessMan[x, y] = 0;
                        return false;
                    }
                        
                }
            }

            if (bFurtherStep)
                requiredSteps += minFurtherSteps + 1;
            else
                requiredSteps = minFurtherSteps;

            ChessMan[x, y] = 0;
            iRealProbeSteps++;
            return true;
        }
    }

    class GoBang
    {
        // private FormMain formReference;
        // data structure storing chessman: 
        // -1 black chessman, 1 white chessman, 0 empty
        private int[,] chessMan = new int[FormMain.ChessBoardScale, FormMain.ChessBoardScale];

        private Strategy strategy; 

        // constructor
        public GoBang(/*FormMain form1*/)
        {
            // formReference = form1;
            strategy = new RawStrategy(chessMan);
            ((RawStrategy)strategy).StragegyImpInst = new RawStrategyImp();
        }

        public void setStrategy(Strategy strategy)
        {
            this.strategy = strategy;
            strategy.setChessman(chessMan);
        }

        // is draw?
        public bool isDraw()
        {
            // TBD: this can be more accurate. if neither side can win, then it is draw, 
            // no need wait for chess board becomes full.
            for (int i = 0; i < FormMain.ChessBoardScale; i++)
            {
                for (int j = 0; j < FormMain.ChessBoardScale; j++)
                {
                    if (chessMan[i, j] == 0)
                        return false;
                }
            }
            return true;
        }

        // remove position
        public void removePos(int x, int y)
        {
            chessMan[x, y] = 0;
        }

        // run the game
        public void run()
        {
            // initialize
            for (int i = 0; i < FormMain.ChessBoardScale; i++)
            {
                for (int j = 0; j < FormMain.ChessBoardScale; j++)
                {
                    chessMan[i, j] = 0;
                }
            }
        }

        public bool isForthgoerForbidden(int x, int y, int side)
        {
            /*
            return (OpsOfChessman_Singleton.Instance.isForthgoerForbidden(chessMan, x, y, side));
            */
            return false;
        }

        //================================================================================
        // this is the most useful to detect chessman's status. 
        // parameters:
        //  continuousNum: how many chessmen are required to be put side by side
        //  emptEndNum: how many ends are required to be standalone (i.e. 0, 1, or 2)
        //  requireNum: how many the cases we need find
        //  isStrict: whether the "continuousNum" is required to be same with the first
        //            parameter exactly
        //  statusArray: the check direction, i.e. horizontal, vertical, and sidelong
        // return:
        //  true: we get the cases
        //  false: we don't get the case
        //=================================================================================
        private bool checkChessmanStatus(int continuousNum, int emptEndNum, int requireNum, bool isStrict,
                                         ChessmenStatus[] statusArray)
        {
            if (requireNum <= 0)
                return true;

            for (int i = 0; i < statusArray.Length; i++)
            {
                if (isStrict)
                {
                    if ((statusArray[i].numContChessmen == continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                            return true;
                    }
                }
                else
                {
                    if ((statusArray[i].numContChessmen >= continuousNum) &&
                        (statusArray[i].numEndEmpty >= emptEndNum))
                    {
                        requireNum--;
                        if (requireNum == 0)
                            return true;
                    }
                }
            }

            return false;
        }

        /*
        // TBD: not quite sure the functionality clearly yet
        private int checkQualifiedPositionNum(int level, int side)
        {
            if (level >= 7)
            {
                // here we check how many positions can quality level above 7 included
                int qualifiedPos = 0;
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        int bak = 0;
                        if ((chessMan[i, j] == 0) || ((side == 1) && (chessMan[i, j] == 2)))
                        {
                            bak = chessMan[i, j];
                            chessMan[i, j] = side;
                            ChessmenStatus[] status = new ChessmenStatus[4];
                            status[0] = new ChessmenStatus();
                            status[1] = new ChessmenStatus();
                            status[2] = new ChessmenStatus();
                            status[3] = new ChessmenStatus();

                            status[0] = numContChessmen_row(i, j);
                            status[1] = numContChessmen_col(i, j);
                            status[2] = numberContChessmen_bkcro(i, j);
                            status[3] = numberContChessmen_fwcro(i, j);

                            if (checkChessmanStatus(4, 2, 1, false, status) ||
                                checkChessmanStatus(3, 2, 2, false, status) ||
                                (checkChessmanStatus(4, 1, 1, false, status) && checkChessmanStatus(3, 2, 1, false, status)) ||
                                checkChessmanStatus(5, 0, 1, false, status))
                            {
                                qualifiedPos++;
                            }
                            chessMan[i, j] = bak;
                        }
                    }
                }

                return qualifiedPos;
            }

            return 0;
        }
        */

        /*
        //==========================================================================
        // TBD: this is the initial algorithm to decide how valuable is a position. 
        // we may replace it later.
        //==========================================================================
        // check status around a chessman to all possible directions
        // valid continuous chessman + 3
        // valid individual chessman + 2
        // valid empty position + 1
        private int checkAround(int x, int y, int side)
        {
            bool up = true;
            bool left = true;
            bool below = true;
            bool right = true;

            int value = 0;

            const int cc = 3;
            const int ic = 2;
            const int ep = 1;

            int weight = FormMain.ChessBoardScale / 2;

            // how about up?
            if (x >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((chessMan[x - i, y] == (side * -1)) ||
                        ((side == -1) && (chessMan[x -i, y] == 2)))
                    {
                        up = false;
                        break;
                    }
                }
            }
            else
            {
                up = false;
            }

            // how about below?
            if ((FormMain.ChessBoardScale - x) > 4) 
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((chessMan[x + i, y] == (side * -1)) ||
                        ((side == -1) && (chessMan[x + i, y] == 2)))
                    {
                        below = false;
                        break;
                    }
                }
            }
            else
            {
                below = false;
            }

            // how about left?
            if (y >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((chessMan[x, y - i] == (side * -1)) ||
                        ((side == -1) && (chessMan[x, y - i] == 2)))
                    {
                        left = false;
                        break;
                    }
                }
            }
            else
            {
                left = false;
            }

            // how about right?
            if ((FormMain.ChessBoardScale - y) > 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    if ((chessMan[x, y + i] == (side * -1)) ||
                        ((side == -1) && (chessMan[x, y + i] == 2)))
                    {
                        left = false;
                        break;
                    }
                }
            }
            else
            {
                left = false;
            }

            // calculate up-left
            if (up && left)
            {
                int iUp = x - 1;
                int iLeft = y - 1;
                int tempWeight = weight;

                while ((iUp > 0) && (iLeft > 0) && 
                       ((chessMan[iUp, iLeft] == side) ||
                        (chessMan[iUp, iLeft] == 0)    ||
                        ((chessMan[iUp, iLeft] == 2) && (side == 1))) )
                {
                    if (chessMan[iUp, iLeft] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[iUp + 1, iLeft + 1] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iUp--;
                    iLeft--;
                    tempWeight--;
                }
            }

            // calculate up
            if (up)
            {
                int iUp = x - 1;
                int tempWeight = weight;

                while ((iUp > 0) && ((chessMan[iUp, y] == side) ||
                        (chessMan[iUp, y] == 0) ||
                        ((chessMan[iUp, y] == 2) && (side == 1))) )
                {
                    if (chessMan[iUp, y] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[iUp + 1, y] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iUp--;
                    tempWeight--;
                }
            }

            // calculate up-right
            if (up && right)
            {
                int iUp = x - 1;
                int iRight = y + 1;
                int tempWeight = weight;

                while ((iUp > 0) && (iRight < FormMain.ChessBoardScale) &&
                       ((chessMan[iUp, iRight] == side) ||
                        (chessMan[iUp, iRight] == 0) ||
                        ((chessMan[iUp, iRight] == 2) && (side == 1))) )
                {
                    if (chessMan[iUp, iRight] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[iUp + 1, iRight - 1] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iUp--;
                    iRight++;
                    tempWeight--;
                }
            }

            // calculate right
            if (right)
            {
                int iRight = y + 1;
                int tempWeight = weight;

                while ((iRight < FormMain.ChessBoardScale) &&
                       ((chessMan[x, iRight] == side) ||
                        (chessMan[x, iRight] == 0) ||
                        ((chessMan[x, iRight] == 2) && (side == 1))) )                    
                {
                    if (chessMan[x, iRight] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[x, iRight - 1] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iRight++;
                    tempWeight--;
                }
            }

            // calculate right-below
            if (right && below)
            {
                int iBelow = x + 1;
                int iRight = y + 1;
                int tempWeight = weight;

                while ((iBelow < FormMain.ChessBoardScale) && (iRight < FormMain.ChessBoardScale) && 
                       ((chessMan[iBelow, iRight] == side) ||
                        (chessMan[iBelow, iRight] == 0) ||
                        ((chessMan[iBelow, iRight] == 2) && (side == 1))) )                      
                {
                    if (chessMan[iBelow, iRight] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[iBelow - 1, iRight - 1] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iBelow++;
                    iRight++;
                    tempWeight--;
                }
            }

            // calculate below
            if (below)
            {
                int iBelow = x + 1;
                int tempWeight = weight;

                while ((iBelow < FormMain.ChessBoardScale) && 
                       ((chessMan[iBelow, y] == side) ||
                        (chessMan[iBelow, y] == 0) ||
                        ((chessMan[iBelow, y] == 2) && (side == 1))) )       
                {
                    if (chessMan[iBelow, y] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[iBelow - 1, y] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iBelow++;
                    tempWeight--;
                }
            }

            // calculate left-below
            if (below && left)
            {
                int iBelow = x + 1;
                int iLeft = y - 1;
                int tempWeight = weight;

                while ((iBelow < FormMain.ChessBoardScale) && (iLeft > 0) && 
                       ((chessMan[iBelow, iLeft] == side) ||
                        (chessMan[iBelow, iLeft] == 0) ||
                        ((chessMan[iBelow, iLeft] == 2) && (side == 1))) )     
                {
                    if (chessMan[iBelow, iLeft] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[iBelow - 1, iLeft + 1] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iBelow++;
                    iLeft--;
                    tempWeight--;
                }
            }

            // calculate left
            if (left)
            {
                int iLeft = y - 1;
                int tempWeight = weight;

                while ((iLeft > 0) && 
                       ((chessMan[x, iLeft] == side) ||
                        (chessMan[x, iLeft] == 0) ||
                        ((chessMan[x, iLeft] == 2) && (side == 1))) )   
                {
                    if (chessMan[x, iLeft] == 0)
                    {
                        value += (ep * tempWeight);
                    }
                    else
                    {
                        // is it a continuous chessman?
                        if (chessMan[x, iLeft + 1] == side)
                        {
                            value += (cc * tempWeight);
                        }
                        else
                        {
                            value += (ic * tempWeight);
                        }
                    }
                    iLeft--;
                    tempWeight--;
                }
            }

            return value;
        }
        */

        // set a chessman automatically and return the new chessman's position
        // this algorithm is newer than "setChessman()"
        public Point setAChessman_new(int side, int forbidRule = 1, int style = 0)
        {
            return strategy.GetNextPos(side, forbidRule);
        }

        public void setMaxProbeStep(int iMaxProbeStep)
        {
            strategy.setMaxProbeStep(iMaxProbeStep);
        }

        /*
        // set a chessman automatically and return the new chessman's position
        public Point setAChessman(int side, int style, int forbidRule)
        {
            ChessmenStore chessmanStore = new ChessmenStore();
            int level = 0;  // 10 - I win if I hold this position
                            // 9 - op win if he hold this position
                            // 8 - I definitely win if I hold this position
                            //      . dead 4, double 3, or 3 + live 4
                            // 7 - op definitely win if he hold this position
                            //      . dead 4, double 3, or 3 + live 4
                            // 6 - best position to hit almost win status
                            //      (+1) if I can be dead 4, double 3, 3 + live 4, or 5 in 2 steps
                            //      (+1) if op can be dead 4, double 3, 3 + live 4, or 5 in 2 steps
                            //      more possible positions have higher priority
                            // 1 - with highest around-checking value
            int tempSubLevel6 = 0;
            int aroundInfo = 0;

            if (forbidRule == 1)
            {
                // mark all of chessman-server-forbidden position
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        //
                        // if (isForthgoerForbidden(i, j, -1))
                        // {
                        //     chessMan[i, j] = 2; // '2' means is a chessman-server-forbidden position
                        // }
                    }
                }
            }

            for (int i = 0; i < FormMain.ChessBoardScale; i++)
            {
                for (int j = 0; j < FormMain.ChessBoardScale; j++)
                {
                    int tempLevel = 0;
                    if ((chessMan[i, j] == 0) || 
                        (chessMan[i, j] == 2) && (side == 1))
                    {
                        // suppose I hold this position to see the result at first
                        ChessmenStatus[] status = new ChessmenStatus[4];
                        ChessmenStatus[] status_op = new ChessmenStatus[4];
                        int bak = chessMan[i, j];
                        chessMan[i, j] = side;
                        status[0] = new ChessmenStatus();
                        status[1] = new ChessmenStatus();
                        status[2] = new ChessmenStatus();
                        status[3] = new ChessmenStatus();

                        status[0] = numContChessmen_row(i, j);
                        status[1] = numContChessmen_col(i, j);
                        status[2] = numberContChessmen_bkcro(i, j);
                        status[3] = numberContChessmen_fwcro(i, j);
                        chessMan[i, j] = bak;

                        // suppose if my op hold this position to see the result
                        if (chessMan[i, j] != 2)
                            chessMan[i, j] = side * (-1);
                        status_op[0] = new ChessmenStatus();
                        status_op[1] = new ChessmenStatus();
                        status_op[2] = new ChessmenStatus();
                        status_op[3] = new ChessmenStatus();

                        status_op[0] = numContChessmen_row(i, j);
                        status_op[1] = numContChessmen_col(i, j);
                        status_op[2] = numberContChessmen_bkcro(i, j);
                        status_op[3] = numberContChessmen_fwcro(i, j);

                        if (chessMan[i, j] != 2)
                            chessMan[i, j] = 0;

                        // check level 10
                        if ((checkChessmanStatus(5, 0, 1, false, status)) && (level <= 10))
                        {
                            tempLevel = 10;
                        }

                        // check level 9
                        if ((tempLevel < 9) && (level <= 9))
                        {
                            if (checkChessmanStatus(5, 0, 1, false, status_op))
                            {
                                tempLevel = 9;
                            }
                        }

                        // check level 8
                        if ((tempLevel < 8) && (level <= 8))
                        {
                            if (checkChessmanStatus(4, 2, 1, false, status) ||
                                checkChessmanStatus(3, 2, 2, false, status) ||
                                (checkChessmanStatus(4, 1, 1, false, status) && checkChessmanStatus(3, 2, 1, false, status)))
                            {
                                tempLevel = 8;
                            }
                        }

                        // check level 7
                        if ((tempLevel < 7) && (level <= 7))
                        {
                            if (checkChessmanStatus(4, 2, 1, false, status_op) ||
                                checkChessmanStatus(3, 2, 2, false, status_op) ||
                                (checkChessmanStatus(4, 1, 1, false, status_op) && checkChessmanStatus(3, 2, 1, false, status_op)))
                            {
                                tempLevel = 7;
                            }
                        }

                        // check level 6
                        if ((tempLevel < 6) && (level <= 6))
                        {
                            chessMan[i, j] = side;
                            int myNum = checkQualifiedPositionNum(7, side);
                            chessMan[i, j] = 0;

                            chessMan[i, j] = side * -1;
                            int opNum = checkQualifiedPositionNum(7, side * -1);
                            chessMan[i, j] = 0;

                            if ((myNum + opNum) > tempSubLevel6)
                            {
                                chessmanStore.clear();
                            }

                            if ((myNum + opNum > 0) && (myNum + opNum) >= tempSubLevel6)
                            {
                                tempLevel = 6;
                                tempSubLevel6 = myNum + opNum;
                            }
                        }

                        // check level 1
                        if ((tempLevel < 1) && (level <= 1))
                        {
                            int tempAroundInfo = 0;
                            if (style == 0)
                            {
                                // ATTACK
                                tempAroundInfo = checkAround(i, j, side);
                            }
                            else if (style == 1)
                            {
                                // BALLANCE
                                tempAroundInfo = 2 * checkAround(i, j, side) + checkAround(i, j, (-1) * side);
                            }
                            else
                            {
                                // DEFEND
                                tempAroundInfo = checkAround(i, j, (-1) * side);
                            }
                            if (aroundInfo < tempAroundInfo)
                            {
                                chessmanStore.clear();
                            }
                            if (aroundInfo <= tempAroundInfo)
                            {
                                tempLevel = 1;
                                aroundInfo = tempAroundInfo;
                            }
                        }

                        if (level < tempLevel)
                        {
                            chessmanStore.clear();
                            chessmanStore.storeChessmen(i, j);
                            level = tempLevel;
                        }
                        else if (level == tempLevel)
                        {
                            chessmanStore.storeChessmen(i, j);
                        }
                    }
                    
                }
            }

            Point point = chessmanStore.getPoint();

            // restore chessman
            if (forbidRule == 1)
            {
                for (int i = 0; i < FormMain.ChessBoardScale; i++)
                {
                    for (int j = 0; j < FormMain.ChessBoardScale; j++)
                    {
                        if (chessMan[i, j] == 2)
                        {
                            chessMan[i, j] = 0; 
                        }
                    }
                }
            }
            return chessmanStore.getPoint();
        }
        */

        // get chessman
        public int[,] getChessMan()
        {
            return chessMan;
        }

        // just for testing purpose
        public int numOfChessman()
        {
            int num = 0; 
            for (int x = 0; x < FormMain.ChessBoardScale; x++)
            {
                for (int y = 0; y < FormMain.ChessBoardScale; y++)
                {
                    if (chessMan[x, y] != 0)
                        num++;
                }
            }
            return num;
        }

        // put a chessman
        // return:  0 - normal
        //         -1 - is a forthforbidden
        public int putChessMan(int x, int y, int side)
        {
            if (OpsOfChessman_Singleton.Instance.isForthgoerForbidden(chessMan, x, y, side))
                return -1;

            if ((side == -1) || (side == 1))
            {
                chessMan[x, y] = side;
            }

            return 0;
        }

        // check how many chessmen connecting continuously
        //-------------------------------------------------
        private ChessmenStatus numContChessmen_row(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus() ;
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            if (value == 0)
            {
                // this must be a forthgoer-forbidden position
                return status;
            }
            
            // left side
            int cy = y - 1;
            while ((cy >= 0) && (chessMan[x, cy] == value))
            {
                status.numContChessmen++;
                cy--;
            }
            if ((cy >= 0) && (chessMan[x, cy] == 0))
            {
                status.numEndEmpty++;
            }

            // right side
            cy = y + 1;
            while ((cy < FormMain.ChessBoardScale) && (chessMan[x, cy] == value))
            {
                status.numContChessmen++;
                cy++;
            }
            if ((cy < FormMain.ChessBoardScale) && (chessMan[x, cy] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }

        private ChessmenStatus numContChessmen_col(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            while ((cx >= 0) && (chessMan[cx, y] == value))
            {
                status.numContChessmen++;
                cx--;
            }
            if ((cx >= 0) && (chessMan[cx, y] == 0))
            {
                status.numEndEmpty++;
            }

            // below
            cx = x + 1;
            while ((cx < FormMain.ChessBoardScale) && (chessMan[cx, y] == value))
            {
                status.numContChessmen++;
                cx++;
            }
            if ((cx < FormMain.ChessBoardScale) && (chessMan[cx, y] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }

        private ChessmenStatus numberContChessmen_bkcro(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            int cy = y - 1;
            while ((cx >= 0) && (cy >= 0) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx--;
                cy--;
            }
            if ((cx >= 0) && (cy >= 0) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            // below
            cx = x + 1;
            cy = y + 1;
            while ((cx < FormMain.ChessBoardScale) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx++;
                cy++;
            }
            if ((cx < FormMain.ChessBoardScale) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }

        private ChessmenStatus numberContChessmen_fwcro(int x, int y)
        {
            ChessmenStatus status = new ChessmenStatus();
            status.numContChessmen = 1;
            int value = chessMan[x, y];

            // up 
            int cx = x - 1;
            int cy = y + 1;
            while ((cx >= 0) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx--;
                cy++;
            }
            if ((cx >= 0) && (cy < FormMain.ChessBoardScale) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            // below
            cx = x + 1;
            cy = y - 1;
            while ((cx < FormMain.ChessBoardScale) && (cy >= 0) && (chessMan[cx, cy] == value))
            {
                status.numContChessmen++;
                cx++;
                cy--;
            }
            if ((cx < FormMain.ChessBoardScale) && (cy >= 0) && (chessMan[cx, cy] == 0))
            {
                status.numEndEmpty++;
            }

            return status;
        }
        //-------------------------------------------------------------

        // check how many dead-four
        private uint numDeadFour(int x, int y, int value)
        {
            // set value
            chessMan[x, y] = value;

            uint number = 0;
            ChessmenStatus status = numContChessmen_row(x, y);
            if ((status.numContChessmen == 4) && (status.numEndEmpty == 2))
            {
                number++;
            }
            status = numContChessmen_col(x, y);
            if ((status.numContChessmen == 4) && (status.numEndEmpty == 2))
            {
                number++;
            }
            status = numberContChessmen_bkcro(x, y);
            if ((status.numContChessmen == 4) && (status.numEndEmpty == 2))
            {
                number++;
            }
            status = numberContChessmen_fwcro(x, y);
            if ((status.numContChessmen == 4) && (status.numEndEmpty == 2))
            {
                number++;
            }

            // restore the chessman
            chessMan[x, y] = 0;

            return number;
        }

        // check whether the selected side has won the game
        // x, y is the latest chessman position
        public bool hasWon(int side, int x, int y)
        {
            if ((numContChessmen_row(x, y).numContChessmen > 4) ||
                (numContChessmen_col(x, y).numContChessmen > 4) ||
                (numberContChessmen_bkcro(x, y).numContChessmen > 4) ||
                (numberContChessmen_fwcro(x, y).numContChessmen > 4))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isOccupied(int x, int y)
        {
            return strategy.isOccupied(x, y);
        }
    }

    // write logs to file
    public class LogInfo
    {
        static public LogInfo getInstance() {
            if (pLogInfo == null)
                pLogInfo = new LogInfo();

            return pLogInfo;
        }

        private LogInfo()
        {
            iLine = 0;
            logFile = new StreamWriter(@"Gobang.log", true);
            logFile.AutoFlush = true;
        }

        ~LogInfo()
        {
            // why this operation can cause something that Windows regards as a problem?
            // logFile.Close();
        }

        public void WriteLog(string strInfo)
        {
            logFile.WriteLine(iLine + " " + strInfo);
            iLine++;
        }
        public void WriteLogFlush(string strInfo)
        {
            WriteLog(strInfo);
        }

        private static int iLine;
        private StreamWriter logFile;

        static private LogInfo pLogInfo;
    }
}

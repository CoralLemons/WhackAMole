using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace HW5_CSC3331
{
    public partial class Form1 : Form
    {
        const int grid_num = 5; // total grid of buttons
        const int top = 100; // initial y cordinate distance from the top of the form
        const int left = 100;  // initial x cordinate distance from the left of the form

        Button[,] barray = new Button[grid_num, grid_num]; // create array to store the 25 buttons

        Random rand = new Random(); // to generate random numbers
        

        public static EventWaitHandle gate = new ManualResetEvent(true); // true means the gate is open
        public static EventWaitHandle threadGate = new AutoResetEvent(false); // false means the gate is closed
        public static CountdownEvent missGate = new CountdownEvent(20); // Signals stopRequest when miss count reaches 20
        public static bool pauseRequested = false;

        #region  may need additional variables to implement the score and miss and request stop
        private static int score = 0;
        private static int miss = 0;

        private static bool stopRequested = false;
        private static object syncObj = new object();
        #endregion
        public Form1()
        {
            InitializeComponent();
        }

        // launches a thread to start the game. 
        private void beginGame()
        {
            Thread gamethread = new Thread(runGame);
            gamethread.Start();
            
        }

        private void runGame()
        {
            #region create 5 threads Every 10-14 (random) seconds
            #endregion
            for (int threads = 0; threads < 5; threads++)
            {
                Thread moleThread = new Thread(playGame); // create new thread that plays the game
                moleThread.Start();

                int moleTime = (rand.Next(10, 14) * 1000); // generate new random time between 10-14 seconds, convert to milisecs

                Thread.Sleep(moleTime); // now sleep
            }
            

        }

        private void playGame()
        {
            while (true)
            {
               if(pauseRequested)
                {
                    gate.WaitOne();
                }

                #region check if stop requested before runnign each time
                lock (syncObj)
                {
                    if (stopRequested)
                    {
                        break;
                    }
                }
                #endregion

                #region generate random row and col
                int row = rand.Next(0, 5);
                int col = rand.Next(0, 5);
                #endregion

                #region if background image is null, then display Image, sleep for 3 seconds, clear the image from button
                if (barray[row, col].BackgroundImage == null)
                {
                    displayImage(row, col);
                    Thread.Sleep(2700);
                    gate.WaitOne();
                    clearButton(row, col);
                    if (this.InvokeRequired)
                    {
                        var d = new Action(updateMiss);
                        this.Invoke(d);
                    }

                }
                #endregion
            }
        }

        private void displayImage(int row, int col)
        {
            // displays the image at given button
            barray[row, col].BackgroundImage = new Bitmap((System.Drawing.Image)(Properties.Resources.mole_png), new Size(50, 50));
        }
        private void clickEvent(object sender, EventArgs e)
        {
            #region Captures the button click event and increment the score and miss accordingly
            Button tmpButton = (Button)sender;
            if(tmpButton.BackgroundImage != null)
            {
                score += 100;
                lblScore.Text = Convert.ToString(score);
                tmpButton.BackgroundImage = null;
            }
            else
            {
                miss += 1;
                lblMiss.Text = Convert.ToString(miss);
                signalMiss();
            }
            #endregion

        }

        private void clearButton(int row, int col)
        {
            // if the image still exists on the button then it means a miss.
            // given the row and col, it sets the button background image to null

            #region clears the background Image 
            if (barray[row, col].BackgroundImage != null)
            {
                barray[row, col].BackgroundImage = null;
                miss += 1;
                signalMiss();
            }
            #endregion
        }

        #region Stop the game(but don’t exit) once 20 misses are accumulated
        #endregion
        private void signalMiss() {
            try{
                missGate.Signal();
            }
            catch (Exception)
            {
                Console.WriteLine("Miss gate has been flagged");
                MessageBox.Show("You reached the max number of misses!\nYour final score is " + score, "Whoops! Game over!", MessageBoxButtons.OK);
            }
            
        }
        private void updateMiss()
        {
            lblMiss.Text = miss.ToString();
            if (missGate.IsSet) {
                lock (syncObj)
                {
                    stopRequested = true;
                }
                btnPause.Enabled = false;
                btnStart.Enabled = false;
                foreach (Button b in barray)
                {
                    b.Enabled = false;
                }
                btnStop.Enabled = false;
                
            }
        }

        private void createButtonGrid()
        {
            #region create Grid of Button
            for (int row = 0; row < grid_num; row++)
            {
                for (int col = 0; col < grid_num; col++)
                {
                    Button tmpButton = new Button();
                    tmpButton.Width = 50;
                    tmpButton.Height = 50;
                    tmpButton.Top = top + row * tmpButton.Height;
                    tmpButton.Left = left + col * tmpButton.Width;
                    this.Controls.Add(tmpButton);
                    tmpButton.Click += clickEvent;
                    barray[row, col] = tmpButton;
                }
            }
            #endregion
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            createButtonGrid(); // create grid of buttons  // store the buttons in an array barray
            beginGame(); // begin game thread
        }

        
        private void btnStop_Click(object sender, EventArgs e)
        {
            btnPause.Enabled = false;
            btnStart.Enabled = false;
            foreach (Button b in barray)
            {
                b.Enabled = false;
            }
            btnStop.Enabled = false;

            // sets a static variable which requests to stop all the threads running the game
            #region stopRequested
            lock (syncObj)
            {
                stopRequested = true;
            }
            #endregion
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            pauseRequested = !pauseRequested;
            if(pauseRequested)
            {
                foreach(Button b in barray)
                {
                    b.Enabled = false;
                }
                gate.Reset();
                btnPause.Text = "Resume";
            }
            else
            {
                foreach (Button b in barray)
                {
                    b.Enabled = true;
                }
                gate.Set();
                btnPause.Text = "Pause";
            }
        }
    }
}

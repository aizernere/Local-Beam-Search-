using System;
using System.Drawing;

using System.Windows.Forms;

using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;

        SixState startState;
        SixState[] states;

        int moveCounter;
        int nstates;
        int optimalStateIndex;


        int[,,] hTable;
        ArrayList[] bMoves;
        Object[] chosenMove;

        public Form1()
        {
            InitializeComponent();

            side = pictureBox1.Width / n;

            startState = randomSixState();

            nstates = 3;
            optimalStateIndex = 0;

            states = new SixState[nstates];

            chosenMove = new object[nstates];

            states[0] = new SixState(startState);
            states[1] = randomSixState();
            states[2] = randomSixState();

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
        }

        private void updateUI()
        {
            //pictureBox1.Refresh();
            pictureBox2.Refresh();

            //label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
            label3.Text = "Attacking pairs: " + getAttackingPairs(states[optimalStateIndex]);
            label4.Text = "Moves: " + moveCounter;
            hTable = getHeuristicTableForPossibleMoves(states);
            bMoves = getBestMoves(hTable);

            listBox1.Items.Clear();

            for (int i = 0; i < nstates; i++)
                if (bMoves[i].Count > 0)
                    chosenMove[i] = chooseMove(bMoves[i]);

            foreach (Point move in bMoves[optimalStateIndex])
            {
                listBox1.Items.Add(move);
            }

            label2.Text = "Chosen move: " + chosenMove[optimalStateIndex];
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            // draw squares
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    // draw queens
                    if (j == states[optimalStateIndex].Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private SixState randomSixState()
        {
            Random r = new Random();
            SixState random = new SixState(r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n));

            return random;
        }

        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;

            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get horizontal attackers
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get diagonal down attackers
                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    // get diagonal up attackers
                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }

            return attackers;
        }

        private int[,,] getHeuristicTableForPossibleMoves(SixState[] thisState)
        {
            int[,,] hStates = new int[nstates, n, n];

            for (int i = 0; i < nstates; i++) // go through the indices
            {
                for (int j = 0; j < n; j++) // replace them with a new value
                {
                    for (int l = 0; l < n; l++)
                    {
                        SixState possible = new SixState(thisState[i]);
                        possible.Y[j] = l;
                        hStates[i, j, l] = getAttackingPairs(possible);
                    }
                }
            }

            return hStates;
        }

        private ArrayList[] getBestMoves(int[,,] heuristicTable)
        {
            ArrayList[] bestMoves = new ArrayList[nstates];
            for (int i = 0; i < nstates; i++)
                bestMoves[i] = new ArrayList();

            int[] bestHeuristicValues = new int[nstates];

            for (int i = 0; i < nstates; i++)
            {
                bestHeuristicValues[i] = heuristicTable[i, 0, 0];
                for (int j = 0; j < n; j++)
                {
                    for (int l = 0; l < n; l++)
                    {
                        if (bestHeuristicValues[i] > heuristicTable[i, j, l])
                        {
                            bestHeuristicValues[i] = heuristicTable[i, j, l];
                            bestMoves[i].Clear();
                            if (states[i].Y[j] != l)
                                bestMoves[i].Add(new Point(j, l));
                        }
                        else if (bestHeuristicValues[i] == heuristicTable[i, j, l])
                        {
                            if (states[i].Y[j] != l)
                                bestMoves[i].Add(new Point(j, l));
                        }
                    }
                }
            }
            for (int i = 0; i < nstates; i++)
                if (bestHeuristicValues[optimalStateIndex] > bestHeuristicValues[i])
                    optimalStateIndex = i;

            label5.Text = "Possible Moves (H=" + bestHeuristicValues[optimalStateIndex] + ")";
            return bestMoves;
        }

        private Object chooseMove(ArrayList possibleMoves)
        {
            int arrayLength = possibleMoves.Count;
            Random r = new Random();
            int randomMove = r.Next(arrayLength);

            return possibleMoves[randomMove];
        }

        private void executeMove(Point move)
        {
            for (int i = 0; i < n; i++)
            {
                startState.Y[i] = states[optimalStateIndex].Y[i];
            }
            states[optimalStateIndex].Y[move.X] = move.Y;
            moveCounter++;

            for (int i = 0; i < nstates; i++)
                chosenMove[i] = null;

            updateUI();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (getAttackingPairs(states[optimalStateIndex]) > 0)
                executeMove((Point)chosenMove[optimalStateIndex]);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            states[0] = startState = randomSixState();
            for (int i = 1; i < nstates; i++)
                states[i] = new SixState();

            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(states[optimalStateIndex]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (getAttackingPairs(states[optimalStateIndex]) > 0)
            {
                for (int i = 0; i < nstates; i++)
                    executeMove((Point)chosenMove[i]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
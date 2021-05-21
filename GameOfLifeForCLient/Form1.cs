using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        private Graphics graphics;
        private int resolution=1;
        private GameEngine gameEngine;
        private BinaryFormatter formatter = new BinaryFormatter();

        public Form1()
        {
            InitializeComponent();
        }
        private void TheBeginning()
        {
            Clear.Enabled = true;
            NUDResolution.Enabled = false;
            NUDDensity.Enabled = false;
            resolution = (int)NUDResolution.Value;

            gameEngine = new GameEngine
                (
                       ROWS : pictureBox1.Height / resolution,
                       COLS : pictureBox1.Width / resolution,
                       Density: (int)NUDDensity.Minimum + (int)NUDDensity.Maximum - (int)NUDDensity.Value
                );

            lblNumberOfGen.Text = Convert.ToString(gameEngine.CurrentGeneration);
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = Graphics.FromImage(pictureBox1.Image);
            timer1.Start();
        }

        private void DrawNextGeneration()
        {
            graphics.Clear(System.Drawing.Color.Black);

            var field = gameEngine.GetCurrentGeneration();
            for(int x=0;x<field.GetLength(0);x++)
            {
                for(int y=0;y<field.GetLength(1);y++)
                {
                    if(field[x,y])
                        graphics.FillRectangle(System.Drawing.Brushes.Crimson, x * resolution, y * resolution, resolution - 1, resolution - 1);
                }
            }            

            pictureBox1.Refresh();
            lblNumberOfGen.Text = Convert.ToString(gameEngine.CurrentGeneration);
            lblPopulation.Text = Convert.ToString(gameEngine.Population);
            gameEngine.NextGeneration();
        }

        private void StopGame()
        {
            if (!timer1.Enabled)
                return;
            timer1.Stop();
            NUDResolution.Enabled = true;
            NUDDensity.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DrawNextGeneration();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            TheBeginning();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopGame();
            btnResume.Enabled = true;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            var x= e.Location.X / resolution; 
            var y= e.Location.Y / resolution; 
            string StateOfCell = string.Empty;
            if (lblNumberOfGen.Text != "0" && NeigboursChown.Checked==true)
            {
                StateOfCell = gameEngine.GetCurrentGeneration()[x, y] ? "Alive" : "Dead";
                richTextBox2.Text = $"Number of Neigbours:{gameEngine.CountNeighbours(x, y)}\nState:{StateOfCell}";
            }

            if (!timer1.Enabled)
                return;

            if (e.Button == MouseButtons.Left)            
                gameEngine.AddCell(x, y);
            

            if (e.Button == MouseButtons.Right)            
                gameEngine.RemoveCell(x, y);
            
        }

        private void btnResume_Click_1(object sender, EventArgs e)
        {
            timer1.Start();
            btnResume.Enabled = false;
        }

        private void NUDResolution_ValueChanged(object sender, EventArgs e)
        {
            btnResume.Enabled = false;
        }

        private void NUDDensity_ValueChanged(object sender, EventArgs e)
        {
            btnResume.Enabled = false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)NUDTimer.Value;
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            gameEngine.EmpitAllLifeCell();
            DrawNextGeneration();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!timer1.Enabled)
            {
                if (e.KeyData == Keys.Right)
                {
                    DrawNextGeneration();
                }
            }
        }
        
        private void Save_Click(object sender, EventArgs e)
        {
            btnStop_Click(this, null);
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            using (FileStream fs = new FileStream(FileValidate(saveFileDialog1.FileName), FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, gameEngine);
            }
        }

        private void Open_Click(object sender, EventArgs e)
        {
            btnStop_Click(this, null);
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            using (FileStream fs= new FileStream(openFileDialog1.FileName, FileMode.Open))
            {
                gameEngine = (GameEngine)formatter.Deserialize(fs);
                pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                graphics = Graphics.FromImage(pictureBox1.Image);
                resolution = pictureBox1.Height / gameEngine.GetCurrentGeneration().GetLength(1);
                NUDResolution.Value = resolution;
                btnResume.Enabled = true;
                Clear.Enabled = true;
            }
            DrawNextGeneration();
        }

        private string FileValidate(string filename)
        {
            string CheckForTxtFormat=string.Empty;
            for(int i=0;i<4;i++)
            {
                CheckForTxtFormat = filename[filename.Length - 1 - i]+ CheckForTxtFormat;
            }
            if (CheckForTxtFormat == ".txt")
                return filename;
            else
                return $"{filename}.txt";
        }

        private void NeigboursChown_CheckedChanged(object sender, EventArgs e)
        {
            if (NeigboursChown.Checked == false)
            {
                richTextBox2.Text = string.Empty;
                richTextBox2.Hide();
            }
            else
                richTextBox2.Show();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.Http;
using NAudio.Wave;
using NAudio.CoreAudioApi;




namespace PodStudio
{
    public partial class Form1 : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CAPTION = 0x2;
        //private const int HTLEFT = 10;
        //private const int HTRIGHT = 11;
        //private const int HTTOP = 12;
        //private const int HTTOPLEFT = 13;
        //private const int HTTOPRIGHT = 14;
        //private const int HTBOTTOM = 15;
        //private const int HTBOTTOMLEFT = 16;
        //private const int HTBOTTOMRIGHT = 17;

        private bool isDragging = false;
        private Point dragStartPoint;
        private bool isFullScreen = false;
        private Size originalSize;
        private Bitmap bitmap;
        private Graphics graphics;

        private WaveInEvent waveIn;
        private WaveFileWriter writer;
        private string outputPath;

        public Form1()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.originalSize = this.Size;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && !isFullScreen)
            {
                int x = (int)(m.LParam.ToInt64() & 0xFFFF);
                int y = (int)((m.LParam.ToInt64() & 0xFFFF0000) >> 16);
                Point pt = PointToClient(new Point(x, y));

                if (pt.Y >= 0 && pt.Y < 50)
                {
                    if (pt.X >= 0 && pt.X < ClientSize.Width)
                        m.Result = (IntPtr)HT_CAPTION;
                }
                else if (pt.Y >= 50 && pt.Y <= ClientSize.Height)
                {
                    if (pt.X >= 0 && pt.X <= ClientSize.Width)
                        m.Result = (IntPtr)HT_CAPTION;
                }
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !isFullScreen)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && !isFullScreen)
            {
                Point diff = new Point(e.X - dragStartPoint.X, e.Y - dragStartPoint.Y);
                Location = new Point(Location.X + diff.X, Location.Y + diff.Y);
            }
        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            if (isFullScreen)
            {
                isFullScreen = false;
                this.WindowState = FormWindowState.Normal;
                this.Size = new Size((int)(originalSize.Width * 0.8), (int)(originalSize.Height * 0.8));
            }
            else
            {
                isFullScreen = true;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            // Fechar o programa
            this.Close();
        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            // Minimizar a janela
            this.WindowState = FormWindowState.Minimized;
        }


        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripComboBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dungeonComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void foreverButton1_Click(object sender, EventArgs e)
        {

        }

        private void materialButton1_Click(object sender, EventArgs e)
        {

        }

        private void iconButton6_Click(object sender, EventArgs e)
        {

        }

        private void iconButton7_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Click(object sender, EventArgs e)
        {

        }

        private void iconButton4_Click(object sender, EventArgs e)
        {

        }

        private void iconButton5_Click(object sender, EventArgs e)
        {

        }

        private void poisonComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void parrotButton1_Click(object sender, EventArgs e)
        {
            if (waveIn == null)
            {
                waveIn = new WaveInEvent();
                waveIn.WaveFormat = new WaveFormat(44100, 1); // Configura a taxa de amostragem e número de canais

                outputPath = "C:\\Users\\Leonel\\Videos\\Antigos\\output.wav"; // Caminho e nome do arquivo de saída

                writer = new WaveFileWriter(outputPath, waveIn.WaveFormat);

                waveIn.DataAvailable += WaveIn_DaataAvailable;

                waveIn.StartRecording();

                parrotButton1.Text = "Parar Gravação";
            }
            else
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;

                writer.Close();
                writer = null;

                parrotButton1.Text = "Iniciar Gravação";
            }
        }

        private void WaveIn_DaataAvailable(object sender, WaveInEventArgs e)
        {
            // Calcula o nível de volume médio
            float level = CalculateSoundLevel(e.Buffer);

            // Atualiza o tamanho da PictureBox com base no nível de volume
            UpdatePictureBoxSize(level);
        }

        private float CalculateSoundLevel(byte[] buffer)
        {
            float sum = 0;
            for (int i = 0; i < buffer.Length; i += 2)
            {
                short sample = (short)((buffer[i + 1] << 9) | buffer[i]);
                sum += Math.Abs(sample);
            }

            // Calcula a média do nível de volume
            float average = sum / (buffer.Length / 2);

            // Normaliza o valor para um intervalo entre 0 e 1
            float normalized = average / short.MaxValue;

            return normalized;
        }

        private void UpdatePictureBoxSize(float level)
        {
            // Define o tamanho máximo da PictureBox quando o áudio estiver alto
            int maxSize = 400;

            // Define o tamanho mínimo da PictureBox quando o áudio estiver baixo
            int minSize = 10;

            // Define o limiar de volume para determinar se o áudio é considerado baixo
            float volumeThreshold = 0.025f;

            // Define a posição vertical inicial da PictureBox
            int initialY = pictureBox1.Location.Y + pictureBox1.Height;

            // Verifica se o nível de volume está abaixo do limiar
            if (level < volumeThreshold)
            {
                // Define o tamanho mínimo para a imagem
                int size = minSize;

                // Calcula a nova posição vertical da PictureBox
                int newY = initialY - size;

                // Atualiza a posição e o tamanho da PictureBox
                pictureBox1.Height = size;
                pictureBox1.Location = new Point(pictureBox1.Location.X, newY);
            }
            else
            {
                // Calcula o tamanho com base no nível de volume
                int size = (int)((level*1.2) * (maxSize - minSize)) + minSize;

                // Calcula a nova posição vertical da PictureBox
                int newY = initialY - size;

                // Atualiza a posição e o tamanho da PictureBox
                pictureBox1.Height = size;
                pictureBox1.Location = new Point(pictureBox1.Location.X, newY);
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Calcula o nível de volume médio
            float level = CalculateSoundLevel(e.Buffer);

            // Atualiza o tamanho da PictureBox com base no nível de volume
            UpdatePictureBoxSize(level);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (waveIn != null)
            {
                waveIn.StopRecording();
                waveIn.Dispose();
                waveIn = null;
            }

            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
        }

        private void parrotSlider1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}

      
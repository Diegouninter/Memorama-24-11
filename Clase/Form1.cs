using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Clase
{
    public partial class Form1 : Form
    {
        private const int GridSize = 6;
        private PictureBox[,] boardPictures;
        private string[] values; // pares mezclados
        private PictureBox firstClicked;
        private PictureBox secondClicked;
        private int moves;
        private int matchesFound;
        private int secondsElapsed;

        // Para imágenes
        private Dictionary<string, Image> faceImages;
        private Image backImage;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartNewGame();
        }

        private void StartNewGame()
        {
            // Detener timers y reset estado
            timerGame.Stop();
            revealTimer.Stop();
            secondsElapsed = 0;
            moves = 0;
            matchesFound = 0;
            firstClicked = null;
            secondClicked = null;
            labelMoves.Text = "Movimientos: 0";
            labelTime.Text = "Tiempo: 00:00";

            // Preparar valores (18 pares)
            var baseValues = new List<string>();
            for (char c = 'A'; c < 'A' + (GridSize * GridSize / 2); c++)
                baseValues.Add(c.ToString());

            values = baseValues.Concat(baseValues).ToArray(); // duplicar
            Shuffle(values);

            // Crear imágenes para cada símbolo (y reverso)
            CreateFaceImages();

            // Crear PictureBox dinámicamente y añadir al TableLayoutPanel
            tableLayoutPanelBoard.Controls.Clear();
            tableLayoutPanelBoard.ColumnCount = GridSize;
            tableLayoutPanelBoard.RowCount = GridSize;

            boardPictures = new PictureBox[GridSize, GridSize];

            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    var pb = new PictureBox();
                    pb.Dock = DockStyle.Fill;
                    pb.Margin = new Padding(6);
                    pb.SizeMode = PictureBoxSizeMode.StretchImage;
                    pb.Tag = values[r * GridSize + c];
                    pb.Image = backImage;
                    pb.Cursor = Cursors.Hand;
                    pb.Click += CardPicture_Click;
                    boardPictures[r, c] = pb;
                    tableLayoutPanelBoard.Controls.Add(pb, c, r);
                }
            }

            // Iniciar timer del juego
            timerGame.Start();
        }

        private void CreateFaceImages()
        {
            // Liberar si existían previamente
            if (faceImages != null)
            {
                foreach (var img in faceImages.Values) img.Dispose();
                faceImages.Clear();
            }
            if (backImage != null)
            {
                backImage.Dispose();
                backImage = null;
            }

            faceImages = new Dictionary<string, Image>();

            // Tamaño base para las imágenes; ImageLayout.Stretch las ajustará al control
            Size imgSize = new Size(200, 200);

            // Generar imagen de reverso (color uniforme)
            backImage = new Bitmap(imgSize.Width, imgSize.Height);
            using (Graphics g = Graphics.FromImage(backImage))
            {
                g.Clear(Color.SlateGray);
                using (var pen = new Pen(Color.DimGray, 6))
                {
                    g.DrawRectangle(pen, 6, 6, imgSize.Width - 12, imgSize.Height - 12);
                }
            }

            // Generar una imagen distintiva por cada símbolo (A..R)
            var rng = new Random();
            var colors = new[] {
                Color.FromArgb(0xE6,0x4A,0x19), Color.FromArgb(0x34,0x98,0xDB),
                Color.FromArgb(0x2E,0xCC,0x71), Color.FromArgb(0x9B,0x59,0xB6),
                Color.FromArgb(0xF1,0xC4,0x0F), Color.FromArgb(0xE7,0x4C,0x3C),
                Color.FromArgb(0x1F,0x8A,0xD6), Color.FromArgb(0x27,0xAE,0x60),
                Color.FromArgb(0x8E,0x44,0xAD), Color.FromArgb(0xF3,0x9C,0x12),
                Color.FromArgb(0xC0,0x39,0x2B), Color.FromArgb(0x16,0xA0,0x85),
                Color.FromArgb(0x2C,0x3E,0x50), Color.FromArgb(0xD4,0xAC,0x0D),
                Color.FromArgb(0xD3,0x54,0x00), Color.FromArgb(0x2E,0xCC,0x71),
                Color.FromArgb(0x7F,0x8C,0x8D), Color.FromArgb(0x95,0xA5,0xA6)
            };

            int pairCount = GridSize * GridSize / 2;
            for (int i = 0; i < pairCount; i++)
            {
                string symbol = ((char)('A' + i)).ToString();
                Color bg = colors[i % colors.Length];
                Bitmap bmp = new Bitmap(imgSize.Width, imgSize.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(bg);
                    // dibujar la letra grande en el centro
                    string s = symbol;
                    using (var f = new Font("Segoe UI", 96, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var sb = new SolidBrush(Color.White))
                    {
                        var sz = g.MeasureString(s, f);
                        g.DrawString(s, f, sb, (imgSize.Width - sz.Width) / 2f, (imgSize.Height - sz.Height) / 2f);
                    }
                }
                faceImages[symbol] = bmp;
            }
        }

        private void Shuffle<T>(T[] array)
        {
            var rng = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = array[i];
                array[i] = array[j];
                array[j] = tmp;
            }
        }

        private void CardPicture_Click(object sender, EventArgs e)
        {
            if (revealTimer.Enabled)
                return; // esperando para ocultar la pareja no coincidente

            var clicked = sender as PictureBox;
            if (clicked == null)
                return;

            // ignorar si ya está descubierta (control deshabilitado)
            if (!clicked.Enabled)
                return;

            // ignorar si ya está mostrando la cara
            if (clicked.Image != backImage)
                return;

            // revelar (poner la imagen de la cara)
            var sym = clicked.Tag.ToString();
            if (faceImages.ContainsKey(sym))
                clicked.Image = faceImages[sym];
            else
                clicked.Image = backImage;

            if (firstClicked == null)
            {
                firstClicked = clicked;
                return;
            }

            if (clicked == firstClicked)
                return;

            secondClicked = clicked;
            moves++;
            labelMoves.Text = $"Movimientos: {moves}";

            // comprobar coincidencia
            if (firstClicked.Tag.ToString() == secondClicked.Tag.ToString())
            {
                // coincidencia: dejar visibles y deshabilitar (evita nuevos clicks)
                firstClicked.Enabled = false;
                secondClicked.Enabled = false;
                firstClicked = null;
                secondClicked = null;
                matchesFound += 1;
                if (matchesFound >= (GridSize * GridSize / 2))
                {
                    timerGame.Stop();
                    MessageBox.Show($"¡Felicidades! Terminaste en {moves} movimientos y {FormatTime(secondsElapsed)}.", "Juego completo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // no coincidencia: iniciar temporizador para ocultar
                revealTimer.Start();
            }
        }

        private void revealTimer_Tick(object sender, EventArgs e)
        {
            revealTimer.Stop();

            if (firstClicked != null)
            {
                firstClicked.Image = backImage;
            }
            if (secondClicked != null)
            {
                secondClicked.Image = backImage;
            }

            firstClicked = null;
            secondClicked = null;
        }

        private void timerGame_Tick(object sender, EventArgs e)
        {
            secondsElapsed++;
            labelTime.Text = $"Tiempo: {FormatTime(secondsElapsed)}";
        }

        private string FormatTime(int totalSeconds)
        {
            int mins = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return $"{mins:00}:{secs:00}";
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {
            StartNewGame();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Liberar imágenes creadas dinámicamente
            if (faceImages != null)
            {
                foreach (var img in faceImages.Values) img.Dispose();
                faceImages = null;
            }
            if (backImage != null)
            {
                backImage.Dispose();
                backImage = null;
            }
            base.OnFormClosing(e);
        }
    }
}

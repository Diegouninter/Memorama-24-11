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
        private Button[,] boardButtons;
        private string[] values; // pares mezclados
        private Button firstClicked;
        private Button secondClicked;
        private int moves;
        private int matchesFound;
        private int secondsElapsed;

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
            // Reset estado
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
            // Usamos letras A..R (18 símbolos)
            for (char c = 'A'; c < 'A' + (GridSize * GridSize / 2); c++)
                baseValues.Add(c.ToString());

            values = baseValues.Concat(baseValues).ToArray(); // duplicar
            Shuffle(values);

            // Crear botones dinámicamente y añadir al TableLayoutPanel
            tableLayoutPanelBoard.Controls.Clear();
            tableLayoutPanelBoard.ColumnCount = GridSize;
            tableLayoutPanelBoard.RowCount = GridSize;

            boardButtons = new Button[GridSize, GridSize];

            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    var btn = new Button();
                    btn.Dock = DockStyle.Fill;
                    btn.Margin = new Padding(6);
                    btn.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
                    btn.BackColor = Color.LightSlateGray;
                    btn.ForeColor = btn.BackColor; // oculto
                    btn.Tag = values[r * GridSize + c];
                    btn.Text = ""; // oculto inicialmente
                    btn.Click += CardButton_Click;
                    boardButtons[r, c] = btn;
                    tableLayoutPanelBoard.Controls.Add(btn, c, r);
                }
            }

            // Iniciar timer del juego
            timerGame.Start();
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

        private void CardButton_Click(object sender, EventArgs e)
        {
            if (revealTimer.Enabled)
                return; // esperando para ocultar la pareja no coincidente

            var clicked = sender as Button;
            if (clicked == null)
                return;

            // ignorar si ya está descubierta
            if (clicked.ForeColor == Color.Black)
                return;

            // revelar
            clicked.Text = clicked.Tag.ToString();
            clicked.ForeColor = Color.Black;

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
                // coincidencia: dejar visibles y deshabilitar
                firstClicked.Enabled = false;
                secondClicked.Enabled = false;
                firstClicked = null;
                secondClicked = null;
                matchesFound += 1;
                // Cada match incrementa en 1 (pero hay 18 pares)
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
                firstClicked.Text = "";
                firstClicked.ForeColor = firstClicked.BackColor;
            }
            if (secondClicked != null)
            {
                secondClicked.Text = "";
                secondClicked.ForeColor = secondClicked.BackColor;
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
    }
}

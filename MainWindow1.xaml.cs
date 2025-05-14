using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
\\БЕЗ КОМЕНТОВ
namespace GoGame
{
    public partial class MainWindow : Window
    {
        private const int BoardSize = 9;
        private const int CellSize = 40;
        private bool isBlackTurn = true;
        private Ellipse[,] stones = new Ellipse[BoardSize, BoardSize];
        private List<string> boardHistory = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            DrawBoard();
        }

        private void DrawBoard()
        {
            canvas.Children.Clear();

            for (int i = 0; i < BoardSize; i++)
            {
                canvas.Children.Add(new Line()
                {
                    X1 = i * CellSize + CellSize / 2,
                    X2 = i * CellSize + CellSize / 2,
                    Y1 = CellSize / 2,
                    Y2 = (BoardSize - 1) * CellSize + CellSize / 2,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                });

                canvas.Children.Add(new Line()
                {
                    X1 = CellSize / 2,
                    X2 = (BoardSize - 1) * CellSize + CellSize / 2,
                    Y1 = i * CellSize + CellSize / 2,
                    Y2 = i * CellSize + CellSize / 2,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                });
            }

            if (BoardSize == 9)
            {
                DrawHoshiPoint(2, 2);
                DrawHoshiPoint(6, 2);
                DrawHoshiPoint(2, 6);
                DrawHoshiPoint(6, 6);
                DrawHoshiPoint(4, 4);
            }
        }

        private void DrawHoshiPoint(int x, int y)
        {
            canvas.Children.Add(new Ellipse()
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Black,
                Margin = new Thickness(x * CellSize + CellSize / 2 - 3,
                                     y * CellSize + CellSize / 2 - 3, 0, 0)
            });
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(canvas);

            int x = (int)Math.Round((pos.X - CellSize / 2) / CellSize);
            int y = (int)Math.Round((pos.Y - CellSize / 2) / CellSize);

            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
                return;

            if (stones[x, y] != null)
                return;

            Ellipse stone = new Ellipse()
            {
                Width = CellSize - 6,
                Height = CellSize - 6,
                Fill = isBlackTurn ? Brushes.Black : Brushes.White,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            Canvas.SetLeft(stone, x * CellSize + 3);
            Canvas.SetTop(stone, y * CellSize + 3);
            canvas.Children.Add(stone);
            stones[x, y] = stone;

            if (IsSuicideMove(x, y))
            {
                MessageBox.Show("Нельзя делать самоубийственный ход!");
                canvas.Children.Remove(stone);
                stones[x, y] = null;
                return;
            }

            if (IsKoViolation())
            {
                MessageBox.Show("Этот ход нарушает правило ко!");
                canvas.Children.Remove(stone);
                stones[x, y] = null;
                return;
            }

            CheckCaptures(x, y);

            isBlackTurn = !isBlackTurn;
            UpdateHistory();
        }

        private bool IsSuicideMove(int x, int y)
        {
            int liberties = 0;
            if (x > 0 && stones[x - 1, y] == null) liberties++;
            if (x < BoardSize - 1 && stones[x + 1, y] == null) liberties++;
            if (y > 0 && stones[x, y - 1] == null) liberties++;
            if (y < BoardSize - 1 && stones[x, y + 1] == null) liberties++;

            return liberties == 0;
        }

        private bool IsKoViolation()
        {
            string currentState = GetBoardState();
            return boardHistory.Contains(currentState);
        }

        private void CheckCaptures(int x, int y)
        {
            CheckStoneCapture(x - 1, y);
            CheckStoneCapture(x + 1, y);
            CheckStoneCapture(x, y - 1);
            CheckStoneCapture(x, y + 1);
        }

        private void CheckStoneCapture(int x, int y)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize || stones[x, y] == null)
                return;

            int liberties = 0;
            if (x > 0 && stones[x - 1, y] == null) liberties++;
            if (x < BoardSize - 1 && stones[x + 1, y] == null) liberties++;
            if (y > 0 && stones[x, y - 1] == null) liberties++;
            if (y < BoardSize - 1 && stones[x, y + 1] == null) liberties++;

            if (liberties == 0)
            {
                canvas.Children.Remove(stones[x, y]);
                stones[x, y] = null;
            }
        }

        private string GetBoardState()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    sb.Append(stones[x, y] == null ? "0" : (stones[x, y].Fill == Brushes.Black ? "1" : "2"));
                }
            }
            return sb.ToString();
        }

        private void UpdateHistory()
        {
            string currentState = GetBoardState();
            boardHistory.Add(currentState);
            if (boardHistory.Count > 2)
                boardHistory.RemoveAt(0);
        }
    }
}

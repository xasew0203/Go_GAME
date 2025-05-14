using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GoGame
{
    public partial class MainWindow : Window
    {
        private int BoardSize = 9;
        private int CellSize;
        private int StoneRadius;

        private bool isBlackTurn = true;
        private Ellipse[,] stones;
        private int[,] board;
        private int blackScore = 0;
        private int whiteScore = 0;

        private Brush boardColor = (Brush)new BrushConverter().ConvertFrom("#FFD9B382");
        private Brush lineColor = Brushes.Black;
        private Brush blackStoneColor = Brushes.Black;
        private Brush whiteStoneColor = Brushes.White;
        private Brush blackStoneBorder = Brushes.Black;
        private Brush whiteStoneBorder = Brushes.Black;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Установка размера доски из ComboBox
            switch (BoardSizeComboBox.SelectedIndex)
            {
                case 0: BoardSize = 9; break;
                case 1: BoardSize = 13; break;
                case 2: BoardSize = 19; break;
            }

            // Расчет размера клетки в зависимости от размера доски
            CellSize = BoardSize <= 9 ? 50 : BoardSize <= 13 ? 35 : 25;
            StoneRadius = (int)(CellSize * 0.45);

            stones = new Ellipse[BoardSize, BoardSize];
            board = new int[BoardSize, BoardSize];
            isBlackTurn = true;

            DrawBoard();
            UpdateStatus();
        }

        private void DrawBoard()
        {
            BoardCanvas.Children.Clear();
            BoardCanvas.Width = CellSize * (BoardSize + 1);
            BoardCanvas.Height = CellSize * (BoardSize + 1);

            // Рисуем линии доски
            for (int i = 0; i < BoardSize; i++)
            {
                // Горизонтальные линии
                Line horizontalLine = new Line
                {
                    X1 = CellSize / 2,
                    Y1 = CellSize / 2 + i * CellSize,
                    X2 = CellSize / 2 + (BoardSize - 1) * CellSize,
                    Y2 = CellSize / 2 + i * CellSize,
                    Stroke = lineColor,
                    StrokeThickness = 1
                };
                BoardCanvas.Children.Add(horizontalLine);

                // Вертикальные линии
                Line verticalLine = new Line
                {
                    X1 = CellSize / 2 + i * CellSize,
                    Y1 = CellSize / 2,
                    X2 = CellSize / 2 + i * CellSize,
                    Y2 = CellSize / 2 + (BoardSize - 1) * CellSize,
                    Stroke = lineColor,
                    StrokeThickness = 1
                };
                BoardCanvas.Children.Add(verticalLine);
            }

            // Рисуем звездные точки
            DrawStarPoints();
        }

        private void DrawStarPoints()
        {
            int[] starPoints;

            if (BoardSize == 9)
                starPoints = new[] { 2, 4, 6 };
            else if (BoardSize == 13)
                starPoints = new[] { 3, 6, 9 };
            else // 19x19
                starPoints = new[] { 3, 9, 15 };

            foreach (int x in starPoints)
            {
                foreach (int y in starPoints)
                {
                    // Пропускаем углы для досок больше 9x9
                    if (BoardSize > 9 && (x == starPoints[0] || x == starPoints[starPoints.Length - 1]) &&
                        (y == starPoints[0] || y == starPoints[starPoints.Length - 1]))
                        continue;

                    Ellipse starPoint = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = Brushes.Black,
                        Margin = new Thickness(CellSize / 2 + x * CellSize - 2.5,
                                              CellSize / 2 + y * CellSize - 2.5, 0, 0)
                    };
                    BoardCanvas.Children.Add(starPoint);
                }
            }
        }

        private void BoardCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(BoardCanvas);
            int x = (int)Math.Round((clickPoint.X - CellSize / 2) / CellSize);
            int y = (int)Math.Round((clickPoint.Y - CellSize / 2) / CellSize);

            if (x >= 0 && x < BoardSize && y >= 0 && y < BoardSize && board[x, y] == 0)
            {
                PlaceStone(x, y);
            }
        }

        private void PlaceStone(int x, int y)
        {
            // Временная копия доски для проверки
            int[,] tempBoard = (int[,])board.Clone();
            tempBoard[x, y] = isBlackTurn ? 1 : 2;

            // Проверка на самоубийство
            if (!HasLiberties(tempBoard, x, y))
            {
                MessageBox.Show("Нельзя ставить камень без дыханий!", "Недопустимый ход",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ставим камень
            board[x, y] = isBlackTurn ? 1 : 2;
            DrawStone(x, y);

            // Захватываем камни противника
            int captured = CaptureOpponentStones(x, y);

            // Обновляем счет
            if (isBlackTurn) blackScore += captured;
            else whiteScore += captured;

            // Меняем ход
            isBlackTurn = !isBlackTurn;
            UpdateStatus();
        }

        private bool HasLiberties(int[,] boardState, int x, int y)
        {
            int stoneColor = boardState[x, y];
            bool[,] visited = new bool[BoardSize, BoardSize];
            return CheckLibertiesDFS(boardState, x, y, stoneColor, visited);
        }

        private bool CheckLibertiesDFS(int[,] boardState, int x, int y, int stoneColor, bool[,] visited)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize || visited[x, y])
                return false;

            visited[x, y] = true;

            if (boardState[x, y] == 0)
                return true;

            if (boardState[x, y] != stoneColor)
                return false;

            return CheckLibertiesDFS(boardState, x - 1, y, stoneColor, visited) ||
                   CheckLibertiesDFS(boardState, x + 1, y, stoneColor, visited) ||
                   CheckLibertiesDFS(boardState, x, y - 1, stoneColor, visited) ||
                   CheckLibertiesDFS(boardState, x, y + 1, stoneColor, visited);
        }

        private int CaptureOpponentStones(int x, int y)
        {
            int opponentColor = isBlackTurn ? 2 : 1;
            int captured = 0;

            TryCaptureGroup(x - 1, y, opponentColor, ref captured);
            TryCaptureGroup(x + 1, y, opponentColor, ref captured);
            TryCaptureGroup(x, y - 1, opponentColor, ref captured);
            TryCaptureGroup(x, y + 1, opponentColor, ref captured);

            return captured;
        }

        private void TryCaptureGroup(int x, int y, int opponentColor, ref int captured)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize || board[x, y] != opponentColor)
                return;

            bool[,] visited = new bool[BoardSize, BoardSize];
            if (!HasLiberties(board, x, y))
            {
                captured += RemoveGroup(x, y, opponentColor, new bool[BoardSize, BoardSize]);
            }
        }

        private int RemoveGroup(int x, int y, int groupColor, bool[,] visited)
        {
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize || visited[x, y] || board[x, y] != groupColor)
                return 0;

            visited[x, y] = true;
            board[x, y] = 0;
            if (stones[x, y] != null)
            {
                BoardCanvas.Children.Remove(stones[x, y]);
                stones[x, y] = null;
            }

            int count = 1;
            count += RemoveGroup(x - 1, y, groupColor, visited);
            count += RemoveGroup(x + 1, y, groupColor, visited);
            count += RemoveGroup(x, y - 1, groupColor, visited);
            count += RemoveGroup(x, y + 1, groupColor, visited);

            return count;
        }

        private void DrawStone(int x, int y)
        {
            if (stones[x, y] != null)
            {
                BoardCanvas.Children.Remove(stones[x, y]);
            }

            Ellipse stone = new Ellipse
            {
                Width = StoneRadius * 2,
                Height = StoneRadius * 2,
                Fill = isBlackTurn ? blackStoneColor : whiteStoneColor,
                Stroke = isBlackTurn ? blackStoneBorder : whiteStoneBorder,
                StrokeThickness = 1
            };

            Canvas.SetLeft(stone, CellSize / 2 + x * CellSize - StoneRadius);
            Canvas.SetTop(stone, CellSize / 2 + y * CellSize - StoneRadius);

            BoardCanvas.Children.Add(stone);
            stones[x, y] = stone;
        }

        private void UpdateStatus()
        {
            StatusLabel.Content = isBlackTurn ? "Чёрные ходят" : "Белые ходят";
            BlackScoreLabel.Content = $"Чёрные: {blackScore}";
            WhiteScoreLabel.Content = $"Белые: {whiteScore}";
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            isBlackTurn = !isBlackTurn;
            UpdateStatus();
            MessageBox.Show(isBlackTurn ? "Белые пропустили ход" : "Чёрные пропустили ход",
                          "Ход пропущен", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResignButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                isBlackTurn ? "Чёрные сдаются? Белые побеждают!" : "Белые сдаются? Чёрные побеждают!",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (isBlackTurn) whiteScore += 10; // Бонус за сдачу
                else blackScore += 10;

                UpdateStatus();
                MessageBox.Show($"Игра окончена!\nЧёрные: {blackScore}\nБелые: {whiteScore}",
                              "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Начать новую игру? Текущий прогресс будет потерян.",
                "Новая игра",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                blackScore = 0;
                whiteScore = 0;
                InitializeGame();
            }
        }
    }
}

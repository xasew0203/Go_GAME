using System;
using System.Collections.Generic; // Для работы с коллекциями (List, Dictionary)
using System.Windows;               // Основные классы WPF (Window, Point)
using System.Windows.Controls;      // Работа с элементами управления (Canvas, Line)
using System.Windows.Input;         // События мыши
using System.Windows.Media;         // Цвета, кисти, фигуры
using System.Windows.Shapes;        // Фигуры (Ellipse, Line)

namespace GoGame
{
    public partial class MainWindow : Window
    {
        // Константы игры
        private const int BoardSize = 9; // Размер доски: 9x9 клеток
        private const int CellSize = 40; // Размер одной клетки в пикселях

        // Текущий игрок: true - черные, false - белые
        private bool isBlackTurn = true;

        // Двумерный массив для хранения камней на доске
        private Ellipse[,] stones = new Ellipse[BoardSize, BoardSize];

        // История состояний доски для проверки правила ко
        private List<string> boardHistory = new List<string>();

        // Конструктор окна
        public MainWindow()
        {
            InitializeComponent(); // Загрузка XAML-интерфейса
            DrawBoard();           // Рисуем доску при запуске
        }

        // Метод рисует сетку доски и точки хоси
        private void DrawBoard()
        {
            canvas.Children.Clear(); // Очищаем Canvas перед новой отрисовкой

            // Рисуем линии доски (вертикальные и горизонтальные)
            for (int i = 0; i < BoardSize; i++)
            {
                // Вертикальная линия
                canvas.Children.Add(new Line()
                {
                    X1 = i * CellSize + CellSize / 2, // Начало линии по X
                    X2 = i * CellSize + CellSize / 2, // Конец линии по X
                    Y1 = CellSize / 2,                // Начало линии по Y
                    Y2 = (BoardSize - 1) * CellSize + CellSize / 2, // Конец по Y
                    Stroke = Brushes.Black,           // Цвет линии
                    StrokeThickness = 1               // Толщина линии
                });

                // Горизонтальная линия
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

            // Если размер доски 9x9, рисуем точки хоси
            if (BoardSize == 9)
            {
                DrawHoshiPoint(2, 2);
                DrawHoshiPoint(6, 2);
                DrawHoshiPoint(2, 6);
                DrawHoshiPoint(6, 6);
                DrawHoshiPoint(4, 4);
            }
        }

        // Рисует точку хоси (маленькую чёрную точку)
        private void DrawHoshiPoint(int x, int y)
        {
            canvas.Children.Add(new Ellipse()
            {
                Width = 6,                     // Ширина точки
                Height = 6,                    // Высота точки
                Fill = Brushes.Black,          // Цвет точки
                Margin = new Thickness(        // Позиционируем точку
                    x * CellSize + CellSize / 2 - 3, // Сдвиг по X
                    y * CellSize + CellSize / 2 - 3, // Сдвиг по Y
                    0, 0)
            });
        }

        // Обработчик клика мыши на Canvas
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем координаты клика относительно Canvas
            Point pos = e.GetPosition(canvas);

            // Определяем, на какое пересечение попал пользователь
            int x = (int)Math.Round((pos.X - CellSize / 2) / CellSize);
            int y = (int)Math.Round((pos.Y - CellSize / 2) / CellSize);

            // Проверяем, не вышел ли клик за границы доски
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
                return;

            // Если на этой точке уже есть камень — ничего не делаем
            if (stones[x, y] != null)
                return;

            // Создаём новый камень (черный или белый в зависимости от игрока)
            Ellipse stone = new Ellipse()
            {
                Width = CellSize - 6,                  // Размер камня чуть меньше клетки
                Height = CellSize - 6,
                Fill = isBlackTurn ? Brushes.Black : Brushes.White,
                Stroke = Brushes.Gray,                 // Граница камня
                StrokeThickness = 1
            };

            // Устанавливаем позицию камня на Canvas
            Canvas.SetLeft(stone, x * CellSize + 3);
            Canvas.SetTop(stone, y * CellSize + 3);
            canvas.Children.Add(stone); // Добавляем камень на Canvas
            stones[x, y] = stone;       // Сохраняем его в массиве

            // Проверяем, является ли ход самоубийственным
            if (IsSuicideMove(x, y))
            {
                MessageBox.Show("Нельзя делать самоубийственный ход!");
                canvas.Children.Remove(stone); // Удаляем камень
                stones[x, y] = null;           // Освобождаем место
                return;
            }

            // Проверяем нарушение правила ко
            if (IsKoViolation())
            {
                MessageBox.Show("Этот ход нарушает правило ко!");
                canvas.Children.Remove(stone);
                stones[x, y] = null;
                return;
            }

            // Проверяем, были ли захвачены камни противника
            CheckCaptures(x, y);

            // Меняем игрока после успешного хода
            isBlackTurn = !isBlackTurn;

            // Обновляем историю состояний доски
            UpdateHistory();
        }

        // Проверяет, является ли ход "самоубийственным"
        private bool IsSuicideMove(int x, int y)
        {
            int liberties = 0; // Свободные соседние точки

            // Смотрим вверх, вниз, влево, вправо
            if (x > 0 && stones[x - 1, y] == null) liberties++;
            if (x < BoardSize - 1 && stones[x + 1, y] == null) liberties++;
            if (y > 0 && stones[x, y - 1] == null) liberties++;
            if (y < BoardSize - 1 && stones[x, y + 1] == null) liberties++;

            return liberties == 0; // Если нет свобод — это самоубийство
        }

        // Проверяет, не повторяется ли предыдущее состояние доски (правило ко)
        private bool IsKoViolation()
        {
            string currentState = GetBoardState(); // Получаем текущее состояние
            return boardHistory.Contains(currentState); // Проверяем в истории
        }

        // Проверяет, нужно ли убрать какие-то камни вокруг
        private void CheckCaptures(int x, int y)
        {
            CheckStoneCapture(x - 1, y); // Лево
            CheckStoneCapture(x + 1, y); // Право
            CheckStoneCapture(x, y - 1); // Вверх
            CheckStoneCapture(x, y + 1); // Вниз
        }

        // Проверяет, можно ли убрать камень противника на указанной позиции
        private void CheckStoneCapture(int x, int y)
        {
            // Если координаты неверны или там нет камня — выходим
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize || stones[x, y] == null)
                return;

            int liberties = 0; // Свободные точки вокруг камня

            if (x > 0 && stones[x - 1, y] == null) liberties++;
            if (x < BoardSize - 1 && stones[x + 1, y] == null) liberties++;
            if (y > 0 && stones[x, y - 1] == null) liberties++;
            if (y < BoardSize - 1 && stones[x, y + 1] == null) liberties++;

            // Если камень противника без свобод — убираем его
            if (liberties == 0)
            {
                canvas.Children.Remove(stones[x, y]); // Удаляем с экрана
                stones[x, y] = null;                  // Освобождаем позицию
            }
        }

        // Преобразует текущее состояние доски в строку для сохранения
        private string GetBoardState()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Проходим по всем клеткам и записываем:
            // 0 - пусто, 1 - черный, 2 - белый
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    if (stones[x, y] == null)
                        sb.Append('0');
                    else if (stones[x, y].Fill == Brushes.Black)
                        sb.Append('1');
                    else
                        sb.Append('2');
                }
            }

            return sb.ToString();
        }

        // Обновляет историю состояний доски
        private void UpdateHistory()
        {
            string currentState = GetBoardState();
            boardHistory.Add(currentState); // Добавляем новое состояние

            // Храним только последние 2 состояния
            if (boardHistory.Count > 2)
                boardHistory.RemoveAt(0);
        }
    }
}

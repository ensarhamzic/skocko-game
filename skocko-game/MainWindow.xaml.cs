using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Timers;

namespace skocko_game
{
    public partial class MainWindow : Window
    {
        private int currentRow; // row that player is currently on
        private int[] winningCombination; // combination player needs to guess
        private int[] userCombination; // combination user played in one turn
        Timer gameTimer;
        int time; // time left

        public MainWindow()
        {
            InitializeComponent();
            time = 60;
            currentRow = 0;
            winningCombination = new int[4];
            userCombination = new int[4];
            for(int i = 0; i < userCombination.Length; i++)
                userCombination[i] = -1; // -1 means no user choice for symbol at the position
            GenerateCombination();
            gameTimer = new Timer(1000);
            gameTimer.Elapsed += TimerTick;
            gameTimer.AutoReset = true;
            gameTimer.Enabled = true;
        }

        private void TimerTick(object sender, ElapsedEventArgs e) // Every second visually simulate clock ticking
        {
            this.Dispatcher.Invoke(() =>
            {
                Elapsed.Height += 7;
                ToElapse.Height -= 7;
            });
            time--;
            if(time == 0)
            {
                gameTimer.Enabled = false;
                ShowCombination();
                MessageBox.Show("You did not manage to finish the game in time.", "Time's up");
                RemoveCombination();
                RestartGame();
            }
        }

        private void GenerateCombination() // Generates combination of 4 symbols that player needs to guess
        {
            Random rand = new Random();
            for(int i = 0; i < winningCombination.Length; i++)
                winningCombination[i] = rand.Next(0, 6);
        }

        private void SymbolClick(object sender, MouseButtonEventArgs e) // Click on symbol to make choice
        {
            for(int i = 0; i < userCombination.Length; i++)
            {
                if(userCombination[i] == -1) // puts clicked symbol on first empty space
                {
                    Grid g = sender as Grid;
                    int symbolNum = int.Parse(g.Name.Substring(1));
                    userCombination[i] = symbolNum; // registers players choice on that position
                    Grid targetGrid = GameGrid.FindName($"r{currentRow}g{i}") as Grid; // gets place where new symbol needs to be placed
                    Image img = new Image();
                    img.Source = (g.Children[1] as Image).Source; // Copy same img source like clicked symbol
                    img.Width = (g.Children[1] as Image).Width;
                    targetGrid.Children.Add(img);
                    break;
                }
            }

            bool full = true;
            Image rowImg = GameGrid.FindName($"r{currentRow}i") as Image;

            // To show arrow fully on filled all fields
            for (int i = 0; i < userCombination.Length; i++)
                if (userCombination[i] == -1)
                    full = false;
            if (full)
                rowImg.Opacity = 1;
            else
                rowImg.Opacity = 0.5;
        }

        private void RemoveSymbol(object sender, MouseButtonEventArgs e) // Upon clicking on symbol on the left, remove it and make able for player to delete choice and put another symbol at that place
        {
            Grid clickedGrid = sender as Grid;
            string clickedGridName = clickedGrid.Name;
            int clickedRow = int.Parse(clickedGridName.Substring(1,1));
            int clickedField = int.Parse(clickedGridName.Substring(3));
            if (clickedRow != currentRow || userCombination[clickedField] == -1) return;

            userCombination[clickedField] = -1; // Resets user choice on clicked position
            clickedGrid.Children.Remove(clickedGrid.Children[1]); // removes symbol
            (GameGrid.FindName($"r{currentRow}i") as Image).Opacity = 0.5;
        }

        private void SubmitCombination(object sender, MouseButtonEventArgs e) // Upon clicking on arrow, checks if there is no empty fields and proceeds to check if player won the game
        {
            Image clickedImage = sender as Image;
            int imageRow = int.Parse(clickedImage.Name.Substring(1, 1));
            bool full = true; ;
            for(int i = 0; i < userCombination.Length; i++)
                if (userCombination[i] == -1)
                    full = false;
            if (!full || currentRow != imageRow) return;
            CheckWin();
        }

        private void CheckWin() // Winning logic
        {
            int correct = 0; // correct symbol on correct position
            int semiCorrect = 0; // correct symbol but not on correct position
            int[] winComb = new int[4]; // copy of winning combination to do some changes in between calculating number of correct and semiCorrect symbols

            for(int i = 0; i < 4; i++)
                winComb[i] = winningCombination[i];

            for(int i = 0; i < 4; i++) // Checking correct symbol on correct place
                if(userCombination[i] == winComb[i])
                {
                    correct++;
                    userCombination[i] = -1; // this is done to prevent false semiCorrect value
                    winComb[i] = -1;
                }

            for (int i = 0; i < 4; i++) // Checking symbol that is correct but not on its correct position
            {
                if (userCombination[i] == -1) continue;

                for(int j = 0; j < 4; j++)
                {
                    if (winComb[j] == -1) continue;
                    if(userCombination[i] == winComb[j])
                    {
                        semiCorrect++;
                        winComb[j] = -1;
                        userCombination[i] = -1;
                    }
                }
            }

            // Visually showing player number of correct and semi correct symbols
            for (int i = 0; i < correct; i++)
            {
                Grid g = GameGrid.FindName($"t{currentRow}g{i}") as Grid;
                (g.Children[0] as Ellipse).Fill = Brushes.Red;
            }
            for(int i = correct; i < correct+semiCorrect; i++)
            {
                Grid g = GameGrid.FindName($"t{currentRow}g{i}") as Grid;
                (g.Children[0] as Ellipse).Fill = Brushes.Yellow;
            }

            (GameGrid.FindName($"r{currentRow}i") as Image).Opacity = 0;
            if(correct == 4)
            {
                gameTimer.Enabled = false;
                MessageBox.Show("You won!", "Congratulations!");
                RestartGame();
                return;
            }
            if (currentRow < 5) // Not win but not game over
            {
                currentRow++;
                (GameGrid.FindName($"r{currentRow}i") as Image).Opacity = 0.5;
            } else if (correct != 4) // Game over (it was last player turn and player didn't guess the combination
            {
                ShowCombination();
                gameTimer.Enabled = false;
                MessageBox.Show("Better luck next time!", "You lost!");
                RemoveCombination();
                RestartGame();
                return;
            }
           
            for (int i = 0; i < 4; i++)
                userCombination[i] = -1; // resetting user choice for next turn
        }

        private void ShowCombination() // Shows combination that player failed to guess
        {
            string[] symbols =
                {
                    "circle.png",
                    "club.png",
                    "spade.png",
                    "heart.png",
                    "diamond.png",
                    "star.png"
                };
            for (int i = 0; i < 4; i++)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Grid g = GameGrid.FindName($"a{i}") as Grid;
                    Image img = new Image();
                    img.Width = 50;
                    img.Source = new BitmapImage(new Uri($"images/{symbols[winningCombination[i]]}", UriKind.RelativeOrAbsolute));
                    g.Children.Add(img);
                });
            }
        }

        private void RemoveCombination() // Just removes shown combination that player failed to guess
        {
            for (int i = 0; i < 4; i++)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Grid g = GameGrid.FindName($"a{i}") as Grid;
                    g.Children.Remove(g.Children[1]);
                });
            }
        }

        private void RestartGame() // Resetting everything
        {
            time = 60;
            gameTimer.Enabled = true;
            this.Dispatcher.Invoke(() => {
                Elapsed.Height = 0;
                ToElapse.Height = 420;
            });
            GenerateCombination();
            for (int i = 0; i <= currentRow; i++)
                for (int j = 0; j < 4; j++)
                    this.Dispatcher.Invoke(() =>
                    {
                        Grid leftGrid = GameGrid.FindName($"r{i}g{j}") as Grid;
                        Grid rightGrid = GameGrid.FindName($"t{i}g{j}") as Grid;
                        int num = 0;
                        foreach (object child in leftGrid.Children)
                            num++;
                        if (num > 1)
                            leftGrid.Children.Remove(leftGrid.Children[1]);
                        (rightGrid.Children[0] as Ellipse).Fill = Brushes.Transparent;
                    });
            for (int i = 0; i < 6; i++)
                this.Dispatcher.Invoke(() =>
                {
                    (GameGrid.FindName($"r{i}i") as Image).Opacity = 0;
                });
            currentRow = 0;
            for (int i = 0; i < 4; i++)
                userCombination[i] = -1;
        }
    }
}

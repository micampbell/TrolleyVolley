using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrolleyVolley
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int round = 0;
        private bool firstRoll = true;
        private bool blueIsOffense = true;
        public int blueScore = 0;
        public int redScore = 0;
        private GameStates current;
        private int[] offenseDice = new int[4];
        private int[] defenseDice = new int[2];
        Random random = new Random();
        Button[,] diceButtons = new Button[6, 6];
        private GameStates[] gameStatesArray =
            Enum.GetValues(typeof(GameStates)).Cast<GameStates>().ToArray();
        private Brush offenseColor;
        private Brush defenseColor;
        private bool offenseStillDeciding = true;
        int[] histogram = new int[6];
        private bool computerIsBlue = false;
        private bool computerIsRed = true;

        public MainWindow()
        {
            InitializeComponent();
            diceButtons[0, 0] = one1Button;
            diceButtons[0, 1] = one2Button;
            diceButtons[0, 2] = one3Button;
            diceButtons[0, 3] = one4Button;
            diceButtons[0, 4] = one5Button;
            diceButtons[0, 5] = one6Button;
            diceButtons[1, 0] = two1Button;
            diceButtons[1, 1] = two2Button;
            diceButtons[1, 2] = two3Button;
            diceButtons[1, 3] = two4Button;
            diceButtons[1, 4] = two5Button;
            diceButtons[1, 5] = two6Button;
            diceButtons[2, 0] = three1Button;
            diceButtons[2, 1] = three2Button;
            diceButtons[2, 2] = three3Button;
            diceButtons[2, 3] = three4Button;
            diceButtons[2, 4] = three5Button;
            diceButtons[2, 5] = three6Button;
            diceButtons[3, 0] = four1Button;
            diceButtons[3, 1] = four2Button;
            diceButtons[3, 2] = four3Button;
            diceButtons[3, 3] = four4Button;
            diceButtons[3, 4] = four5Button;
            diceButtons[3, 5] = four6Button;
            diceButtons[4, 0] = five1Button;
            diceButtons[4, 1] = five2Button;
            diceButtons[4, 2] = five3Button;
            diceButtons[4, 3] = five4Button;
            diceButtons[4, 4] = five5Button;
            diceButtons[4, 5] = five6Button;
            diceButtons[5, 0] = six1Button;
            diceButtons[5, 1] = six2Button;
            diceButtons[5, 2] = six3Button;
            diceButtons[5, 3] = six4Button;
            diceButtons[5, 4] = six5Button;
            diceButtons[5, 5] = six6Button;
            updateScore();
            offenseColor = new SolidColorBrush(Colors.CornflowerBlue);
            defenseColor = new SolidColorBrush(Colors.PaleVioletRed);
            report("Ready to play? Yeah!");
            displayDice();
        }

        private void report(string reportString)
        {
            ResultBorder.Visibility = Visibility.Visible;
            ReportResult.Content = reportString;
            //Thread.Sleep(1000);
            //ResultBorder.Visibility = Visibility.Collapsed;
        }

        private void updateScore()
        {
            firstRoll = true;
            if (current == GameStates.Turnover) return;
            var score = 2 * ((int)current - 1) + 1;
            if (blueIsOffense) blueScore += score;
            else redScore += score;

            redScoreLabel.Content = redScore;
            blueScoreLabel.Content = blueScore;
        }

        private void defenseRollButton_Click(object sender, RoutedEventArgs e)
        {
            if (firstRoll) startNewRound();
            else makeSecondRoll();
        }

        private void makeSecondRoll()
        {
            #region Roll Dice

            var newValues = new List<int>(offenseDice);
            foreach (var offenseRerollsItem in offenseRerolls.Items)
                newValues.Remove((int)offenseRerollsItem);
            for (int i = newValues.Count; i < 4; i++)
                newValues.Add(RollDice());
            offenseDice = newValues.ToArray();
            newValues = new List<int>(defenseDice);
            foreach (var defenseRerollsItem in defenseRerolls.Items)
                newValues.Remove((int)defenseRerollsItem);
            for (int i = newValues.Count; i < 2; i++)
                newValues.Add(RollDice());
            defenseDice = newValues.ToArray();
            offenseRerolls.Items.Clear();
            defenseRerolls.Items.Clear();
            #endregion
            displayDice();
            current = evaluateRound(offenseDice, defenseDice);
            report(current.ToString());
            updateScore();
            if (current == GameStates.Turnover)
            {
                var tempColor = defenseColor;
                defenseColor = offenseColor;
                offenseColor = tempColor;
                offenseGrid.Background = offenseColor;
                defenseGrid.Background = defenseColor;
                blueIsOffense = !blueIsOffense;
            }

            if (blueScore >= 21 || redScore >= 21) endGame();
        }

        private void endGame()
        {
            var winner = blueScore >= 21 ? "blue" : "red";
            report(winner + " is the winner!");
            offenseDoneButton.IsEnabled = defenseRollButton.IsEnabled = false;
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    diceButtons[i, j].Visibility = Visibility.Visible;

        }

        private GameStates evaluateRound(IList<int> offense, IList<int> defense)
        {
            var dice = new List<int>(offense);
            dice.Add(defense[0]);
            dice.Add(defense[1]);
            dice.Sort();    //    .OrderBy(x => x).ToList();
            for (int i = 0; i < 6; i++)
                histogram[i] = 0;
            for (int i = 0; i < 6; i++)
                histogram[dice[i] - 1]++;
            var maxOfAKind = 0;
            var indexOfMaxOfAKind = 0;
            for (int i = 0; i < 6; i++)
                if (maxOfAKind < histogram[i])
                {
                    maxOfAKind = histogram[i];
                    indexOfMaxOfAKind = i + 1;
                }
            if (maxOfAKind >= 3 && defense.Contains(indexOfMaxOfAKind))
                return GameStates.Turnover;
            var zeroLocations = new List<int>();
            for (int i = 0; i < 6; i++)
                if (histogram[i] == 0) zeroLocations.Add(i);
            if (zeroLocations.Count == 0) return GameStates.Trolley6;
            if (zeroLocations.Count == 1 && (histogram[0] == 0 || histogram[5] == 0))
                return GameStates.Trolley5;
            var run = new List<int>();
            for (int i = 1; i <= 6; i++)
                if (histogram[i - 1] != 0) run.Add(i);
                else if (run.Count >= 3)
                    break;
                else run.Clear();
            if (!run.Intersect(defense).Any()) return GameStates.Turnover;
            if (run.Count <= 2) return GameStates.Turnover;
            return gameStatesArray[run.Count - 2];
        }

        private void startNewRound()
        {
            offenseStillDeciding = true;
            this.Title = "Trolley Volley: Round #" + ++round;
            for (int i = 0; i < 4; i++)
                offenseDice[i] = RollDice();
            for (int i = 0; i < 2; i++)
                defenseDice[i] = RollDice();
            firstRoll = false;
            displayDice();
            defenseRollButton.IsEnabled = false;
            offenseDoneButton.IsEnabled = true;
            if ((blueIsOffense && computerIsBlue)
                || (!blueIsOffense && computerIsRed))
                computerOpponentOffense();
        }

        private void computerOpponentOffense()
        {
            var index = 0;
            var choices = new int[16][];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        for (int m = 0; m < 2; m++)
                        {
                            var keepers = new List<int>();
                            if (i == 0) keepers.Add(offenseDice[0]);
                            if (j == 0) keepers.Add(offenseDice[1]);
                            if (k == 0) keepers.Add(offenseDice[2]);
                            if (m == 0) keepers.Add(offenseDice[3]);
                            choices[index++] = computerOpponentDefense(keepers,defenseDice);
                        }
            int choice = determineBestOffense(choices);
            var rows = new int[6];
            if (choice >= 8)
            {
                choice -= 8;
                offenseDiceButton_Click(diceButtons[offenseDice[0] - 1, rows[offenseDice[0] - 1]++], null);
            }
            if (choice >= 4)
            {
                choice -= 4;
                offenseDiceButton_Click(diceButtons[offenseDice[1] - 1, rows[offenseDice[1] - 1]++], null);
            }
            if (choice >= 2)
            {
                choice -= 2;
                offenseDiceButton_Click(diceButtons[offenseDice[2] - 1, rows[offenseDice[2] - 1]++], null);
            }
            if (choice >= 1)
            {
                offenseDiceButton_Click(diceButtons[offenseDice[3] - 1, rows[offenseDice[3] - 1]++], null);
            }
        }

        private void displayDice()
        {
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 6; j++)
                    diceButtons[i, j].Visibility = Visibility.Collapsed;
            histogram = new[] { 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                var value = offenseDice[i] - 1;
                if (value == -1) continue;
                diceButtons[value, histogram[value]].Visibility = Visibility.Visible;
                histogram[value]++;
            }
            if (defenseDice[0] == 0) return;
            diceButtons[defenseDice[0] - 1, 4].Visibility = Visibility.Visible;
            if (defenseDice[1] == defenseDice[0])
                diceButtons[defenseDice[1] - 1, 5].Visibility = Visibility.Visible;
            else
                diceButtons[defenseDice[1] - 1, 4].Visibility = Visibility.Visible;

        }

        int RollDice()
        {
            return random.Next(1, 7);
        }

        private void offenseDiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!offenseStillDeciding) return;
            ((Button)sender).Visibility = Visibility.Collapsed;
            offenseRerolls.Items.Add(int.Parse(((Button)sender).Tag.ToString()));
        }
        private void defenseDiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (offenseStillDeciding) return;
            ((Button)sender).Visibility = Visibility.Collapsed;
            defenseRerolls.Items.Add(int.Parse(((Button)sender).Tag.ToString()));
        }

        private void offenseDoneButton_Click(object sender, RoutedEventArgs e)
        {
            offenseDoneButton.IsEnabled = false;
            defenseRollButton.IsEnabled = true;
            offenseStillDeciding = false;
            var newValues = new List<int>(offenseDice);
            foreach (var offenseRerollsItem in offenseRerolls.Items)
                newValues.Remove((int)offenseRerollsItem);
            if ((!blueIsOffense && computerIsBlue)
                || (blueIsOffense && computerIsRed))
                computerOpponentDefense(newValues, defenseDice);
        }

        private int[] computerOpponentDefense(IList<int> offenseGiven, IList<int> defenseGiven)
        {
            var choices = new[]
            {
                evaluatePossibilities(offenseGiven, defenseGiven),
                evaluatePossibilities(offenseGiven, new[] { defenseGiven[0]}),
                evaluatePossibilities(offenseGiven, new[] { defenseGiven[1]}),
                evaluatePossibilities(offenseGiven, new int[0])
            };
            int choice = determineBestDefense(choices);
            if (choice == 1)
                defenseDiceButton_Click(diceButtons[defenseGiven[1] - 1, 4], null);
            else if (choice == 2)
                defenseDiceButton_Click(diceButtons[defenseGiven[0] - 1, 4], null);
            else if (choice == 3)
            {
                defenseDiceButton_Click(diceButtons[defenseGiven[0] - 1, 4], null);
                if (defenseGiven[0] == defenseGiven[1])
                    defenseDiceButton_Click(diceButtons[defenseGiven[1] - 1, 5], null);
                else defenseDiceButton_Click(diceButtons[defenseGiven[1] - 1, 4], null);
            }

            return choices[choice];
        }

        private int determineBestDefense(int[][] choices)
        {
            var scores = choices.Select(calcScore).ToArray();
            var bestChoice = 0;
            var maxChanceTurnover = 0.0;
            var winAverge = 0.0;
            for (int i = 0; i < 4; i++)
            {
                if (scores[i].Item1 > maxChanceTurnover)
                {
                    maxChanceTurnover = scores[i].Item1;
                    winAverge = scores[i].Item2;
                    bestChoice = i;
                }
                if (scores[i].Item1 == maxChanceTurnover && scores[i].Item2 < winAverge)
                {
                    winAverge = scores[i].Item2;
                    bestChoice = i;
                }
            }
            return bestChoice;
        }

        private int determineBestOffense(int[][] choices)
        {
            var scores = choices.Select(calcScore).ToArray();
            var bestChoice = 0;
            var minChanceTurnover = 2.0;
            var winAverge = 0.0;
            for (int i = 0; i < 16; i++)
            {
                if (scores[i].Item1 < minChanceTurnover)
                {
                    minChanceTurnover = scores[i].Item1;
                    winAverge = scores[i].Item2;
                    bestChoice = i;
                }
                if (scores[i].Item1 == minChanceTurnover && scores[i].Item2 > winAverge)
                {
                    winAverge = scores[i].Item2;
                    bestChoice = i;
                }
            }
            return bestChoice;
        }

        Tuple<double, double> calcScore(int[] choice)
        {
            var sum = (double)choice.Sum();
            return new Tuple<double, double>(choice[0] / sum,
               (choice[1] + 3 * choice[2] + 5 * choice[3] + 7 * choice[4]) / (sum - choice[0]));
        }

        private int[] evaluatePossibilities(IList<int> offenseGiven, IList<int> defenseGiven)
        {
            var result = new int[5];
            var offense = new int[4];
            for (int i = 0; i < offenseGiven.Count; i++)
                offense[3 - i] = offenseGiven[i];
            var numOffenseToTry = 4 - offenseGiven.Count;
            for (int i = 1; i < numOffenseToTry; i++)
                offense[i] = 1;
            var index = 0;
            if (numOffenseToTry == 0) return evaluateDefensePossibilities(offense, defenseGiven);
            while (true)
            {
                offense[index]++;
                if (offense[index] == 7)
                {
                    if (++index == numOffenseToTry) break;
                    for (int i = 0; i < index; i++)
                        offense[i] = offense[index] + 1;
                }
                else
                {
                    int numUnique = determineUnique(offense, numOffenseToTry);
                    if (numUnique == 4) numUnique = 24;
                    else if (numUnique == 3) numUnique = 6;
                    var defScores = evaluateDefensePossibilities(offense, defenseGiven);
                    for (int i = 0; i < 5; i++)
                        result[i] += numUnique * defScores[i];
                    index = 0;
                }
            }
            return result;
        }

        private int[] evaluateDefensePossibilities(IList<int> offenseGiven, IList<int> defenseGiven)
        {
            var result = new int[5];
            if (defenseGiven.Count == 2)
            {
                var outcome = evaluateRound(offenseGiven, defenseGiven);
                result[(int)outcome] = 1;
                return result;
            }
            if (defenseGiven.Count == 1)
            {
                var fixedDefDice = defenseGiven[0];
                for (int i = 1; i < 7; i++)
                {
                    var outcome = evaluateRound(offenseGiven, new[] { fixedDefDice, i });
                    result[(int)outcome] += 1;
                }
                return result;
            }
            for (int i = 1; i < 7; i++)
            {
                for (int j = i; j < 7; j++)
                {
                    var outcome = evaluateRound(offenseGiven, new[] { j, i });
                    if (i == j) result[(int)outcome] += 1;
                    else result[(int)outcome] += 2;
                }
            }
            return result;
        }


        private int determineUnique(int[] offense, int numToTry)
        {
            var result = 0;
            for (int i = 0; i < numToTry; i++)
            {
                bool unique = true;
                for (int j = 0; j < i; j++)
                {
                    if (offense[j] == offense[i])
                    {
                        unique = false;
                        break;
                    }
                }

                if (unique) result++;
            }
            return result;
        }
    }
}

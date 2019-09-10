using Microsoft.Win32;
using RuleExtraction.Algo;
using RuleExtraction.Algo.Dataset;
using RuleExtraction.Algo.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RuleExtraction.GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        public bool CanRun
        {
            get => (bool)GetValue(CanRunProperty);
            set => SetValue(CanRunProperty, value);
        }
        public static readonly DependencyProperty CanRunProperty =
            DependencyProperty.Register("CanRun", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public bool CanCancel
        {
            get => (bool)GetValue(CanCancelProperty);
            set => SetValue(CanCancelProperty, value);
        }
        public static readonly DependencyProperty CanCancelProperty =
            DependencyProperty.Register("CanCancel", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        #region ProgressData

        public int ProgressValue
        {
            get => (int)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }
        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register("ProgressValue", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public int MaximumProgressValue
        {
            get => (int)GetValue(MaximumProgressValueProperty);
            set => SetValue(MaximumProgressValueProperty, value);
        }
        public static readonly DependencyProperty MaximumProgressValueProperty =
            DependencyProperty.Register("MaximumProgressValue", typeof(int), typeof(MainWindow), new PropertyMetadata(100));

        public string TimeFinished
        {
            get => (string)GetValue(TimeFinishedProperty);
            set => SetValue(TimeFinishedProperty, value);
        }
        public static readonly DependencyProperty TimeFinishedProperty =
            DependencyProperty.Register("TimeFinished", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public string TextLog
        {
            get => (string)GetValue(TextLogProperty);
            set => SetValue(TextLogProperty, value);
        }
        public static readonly DependencyProperty TextLogProperty =
            DependencyProperty.Register("TextLog", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));
        #endregion

        #region AlgorithmParameters
        public string Filename
        {
            get => (string)GetValue(FilenameProperty);
            set => SetValue(FilenameProperty, value);
        }
        public static readonly DependencyProperty FilenameProperty =
            DependencyProperty.Register("Filename", typeof(string), typeof(MainWindow), new PropertyMetadata(string.Empty));

        public bool IsParallel
        {
            get => (bool)GetValue(IsParallelProperty);
            set => SetValue(IsParallelProperty, value);
        }
        public static readonly DependencyProperty IsParallelProperty =
            DependencyProperty.Register("IsParallel", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public double SupportValue
        {
            get => (double)GetValue(SupportValueProperty);
            set => SetValue(SupportValueProperty, value);
        }
        public static readonly DependencyProperty SupportValueProperty =
            DependencyProperty.Register("SupportValue", typeof(double), typeof(MainWindow), new PropertyMetadata(0.5));

        public int ColNumValue
        {
            get => (int)GetValue(ColNumValueProperty);
            set => SetValue(ColNumValueProperty, value);
        }
        public static readonly DependencyProperty ColNumValueProperty =
            DependencyProperty.Register("ColNumValue", typeof(int), typeof(MainWindow), new PropertyMetadata(200));

        public int RowNumValue
        {
            get => (int)GetValue(RowNumValueProperty);
            set => SetValue(RowNumValueProperty, value);
        }
        public static readonly DependencyProperty RowNumValueProperty =
            DependencyProperty.Register("RowNumValue", typeof(int), typeof(MainWindow), new PropertyMetadata(5000));
        #endregion
        #endregion
        private CancellationTokenSource cancellation;

        public MainWindow() => InitializeComponent();

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".arff",
                Filter = "Arff данные|*.arff"
            };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                ProgressValue = 0;
                TimeFinished = string.Empty;
                Filename = dialog.FileName;
                CanRun = true;
            }
        }

        private void ExitClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void StopProcessingClick(object sender, RoutedEventArgs e) =>
            cancellation.Cancel();

        private async void StartProcessingClick(object sender, RoutedEventArgs e)
        {
            ProgressValue = 0;
            TextLog = string.Empty;
            if (SupportValue < 0 || SupportValue >= 1)
            {
                MessageBox.Show("Значение поддержки должно быть от 0 до 1");
                return;
            }
            MaximumProgressValue = 100;
            CanRun = false;
            try
            {
                var dataset = ArffParser.Parse(Filename, ColNumValue, RowNumValue);
                MaximumProgressValue = dataset.columnNames.Count;
                var r = IsParallel ? (IRulesExtractor)
                    new TrueEclatParallel(dataset) :
                    new TrueEclatSequental(dataset);
                r.ProgressReportEvent += ProgressReport;
                var sup = SupportValue;
                cancellation = new CancellationTokenSource();
                CanCancel = true;
                var timeRules = await Task.Run(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var list = r.StartProcess(sup, cancellation.Token);
                    sw.Stop();
                    return new Tuple<double, List<Rule>>(
                        sw.ElapsedMilliseconds, list);
                });
                r.ProgressReportEvent -= ProgressReport;
                TimeFinished = $"{timeRules.Item1} мс";
                TextLog = await Task.Run(() => RulesAsString(timeRules.Item2, dataset.columnNames));
                CanCancel = false;
                cancellation.Dispose();
            }
            catch (Exception)
            {
                MessageBox.Show("Набор данных имеет некорректный формат");
            }
            finally
            {
                CanRun = true;
                ProgressValue = 100;
                MaximumProgressValue = 100;
            }
        }

        private void ProgressReport(object sender, ProgressChangedArgs e) => Dispatcher.Invoke(() => ProgressValue += e.ProgressAdd);

        private static string RulesAsString(List<Rule> rules, List<string> columnNames)
        {
            rules.Sort(Rule.CompareBySupport);
            var sb = new StringBuilder();
            foreach (var item in rules)
                sb.AppendLine(item.ToStringNamedColumns(columnNames));
            return sb.ToString();
        }
    }
}

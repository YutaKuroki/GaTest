using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DistanceGA
{

    public partial class MainWindow : Window
    {

        /// <summary>
        /// 都市の数設定
        /// </summary>
        private const int CITY_COUNT = 100; 

        /// <summary>
        /// 都市リスト
        /// </summary>
        private List<City> m_Cities = [];

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunGeneticAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            StartGA();
        }

        /// <summary>
        /// GA開始
        /// </summary>
        private async void StartGA()
        {
            // 都市描画
            DrawCities();

            var ga = new GA(m_Cities);

            await Task.Run(() =>
            {
                ga.Run(
                    generationCount: 2000,
                    populationMin: 300,
                    populationMax: 400,
                    onGeneration: (route , generation) =>
                    {
                        var bestPath = route.Select(index => m_Cities[index]).ToList();
                        var distance = CalculateTotalDistance(bestPath);
                        Dispatcher.Invoke(() =>
                        {
                            DrawCities();
                            DrawPath(bestPath);
                            GenerationTextBlock.Text = $"世代: {generation}";
                            DistanceTextBlock.Text = $"距離: {distance:F2}";
                        });

                        Thread.Sleep(10);
                    }
                );
            });

            //// GA実行
            //var ga = new GA(m_Cities);
            //var bestRoute = ga.Run(generationCount: 500, populationMin: 100, populationMax: 200);
            //var bestPath = bestRoute.Select(index => m_Cities[index]).ToList();

            // 最適経路描画
            //DrawPath(bestPath);
        }

        private double CalculateTotalDistance(List<City> path)
        {
            double total = 0;
            for (int i = 0; i < path.Count; i++)
            {
                var a = path[i];
                var b = path[(i + 1) % path.Count];
                total += a.DistanceTo(b);
            }
            return total;
        }

        #region <<<< 初期化関連処理

        private void InitializeParameters()
        {

            m_Cities.Clear();
            GenerateCities(100); // 固定シードで都市生成
        }

        private void GenerateCities(int seed)
        {
            var rand = new Random(seed);
            m_Cities.Clear();
            for (int i = 0; i < CITY_COUNT; i++)
            {
                double x = rand.NextDouble() * MyCanvas.ActualWidth;
                double y = rand.NextDouble() * MyCanvas.ActualHeight;
                m_Cities.Add(new City(i, x, y));
            }
        }
        #endregion

        #region <<<< 処理関連


        #endregion

        #region <<<< 描画関連処理

        /// <summary>
        /// 都市座標を描画
        /// </summary>
        private void DrawCities()
        {
            MyCanvas.Children.Clear();
            foreach (var city in m_Cities)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(ellipse, city.X - 3);
                Canvas.SetTop(ellipse, city.Y - 3);
                MyCanvas.Children.Add(ellipse);
            }
        }

        /// <summary>
        /// 都市間の経路を描画
        /// </summary>
        /// <param name="path"></param>
        private void DrawPath(List<City> path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                var cityA = path[i];
                var cityB = path[(i + 1) % path.Count];
                Line line = new Line
                {
                    X1 = cityA.X,
                    Y1 = cityA.Y,
                    X2 = cityB.X,
                    Y2 = cityB.Y,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 1
                };
                MyCanvas.Children.Add(line);
            }
        }
        #endregion

        private void Initialize_Click(object sender, RoutedEventArgs e)
        {
            // 初期化
            InitializeParameters();

            MyCanvas.Children.Clear();

            // 都市描画
            DrawCities();

            // 経路描画を仮で都市順に描画
            DrawPath(m_Cities);
        }
    }

    /// <summary>
    /// 座標管理クラス
    /// </summary>
    /// <param name="id">都市コード</param>
    /// <param name="x">X座標</param>
    /// <param name="y">Y座標</param>
    public class City(int id, double x, double y)
    {
        /// <summary>
        /// 都市コード
        /// </summary>
        public int Id { get; set; } = id;

        /// <summary>
        /// X座標
        /// </summary>
        public double X { get; set; } = x;

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y { get; set; } = y;

        /// <summary>
        /// 距離計算
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double DistanceTo(City other)
        {
            double deltaX = X - other.X;
            double deltaY = Y - other.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
    }
}
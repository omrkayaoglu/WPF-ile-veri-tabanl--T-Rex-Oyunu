using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using System.Data.OleDb;

namespace T_Rex_Game
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Veri tabanı için OleDbCONNECTİON sınıfından ve OleDbCommand sınıfından eleman çağırıldı
        /// </summary>
        OleDbConnection baglanti = new OleDbConnection("Provider=Microsoft.ACE.oleDb.12.0; Data Source=Skor.accdb ");
        OleDbCommand komut = new OleDbCommand();

        DispatcherTimer gameTimer = new DispatcherTimer();
        /// <summary>
        /// rectangel larara isim verdik
        /// </summary>
        Rect playerHitBox; 
        Rect groundHitBox;
        Rect obstacleHitBox;

        /// <summary>
        /// zıplama hız güç gibi şeyler belirledik
        /// </summary>
        bool jumping;

        int force = 20;//güç
        int speed = 5;

        Random rnd = new Random();

        bool gameOver;

        double spriteIndex = 0; //hareketli grafik

        /// <summary>
        /// Hareketli koşucu ve engel koyuldu
        /// </summary>
        ImageBrush playerSprite = new ImageBrush();
        ImageBrush backgroundSprite = new ImageBrush();
        ImageBrush obstacleSprite = new ImageBrush();

        /// <summary>
        /// engelin pozisyonları belilendi
        /// </summary>
        int[] obstaclePosition = { 320, 310, 300, 305, 315 };

        int score = 0;
        /// <summary>
        /// arka plan resmi ve oyun moturu oluşturuldu
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            MyCanvas.Focus();

            gameTimer.Tick += GameEngine;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            backgroundSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/beyaz_arkaPlan.png"));
            background.Fill = backgroundSprite;
            background2.Fill = backgroundSprite;

            StartGame();
        }

        private void GameEngine(object sender, EventArgs e)
        {
            /// <summary>
            /// arka plan pozisyonları iki rectangle ye de ayarlandı
            /// </summary>

            Canvas.SetLeft(background, Canvas.GetLeft(background) - 3);
            Canvas.SetLeft(background2, Canvas.GetLeft(background2) - 3);

            /// <summary>
            /// rectangle lerin dışına çıkmasın diye ayarlandı
            /// </summary>
            if (Canvas.GetLeft(background) < -1262)
            {
                Canvas.SetLeft(background, Canvas.GetLeft(background2) + background2.Width);
            }

            if (Canvas.GetLeft(background2) < -1262)
            {
                Canvas.SetLeft(background2, Canvas.GetLeft(background) + background.Width);
            }


            /// <summary>
            /// oyuncunun hızı ayarladnı
            /// </summary>
            Canvas.SetTop(player, Canvas.GetTop(player) + speed);
            Canvas.SetLeft(obstacle, Canvas.GetLeft(obstacle) - 12);

            /// <summary>
            /// skor ekrana yazdırıldı
            /// </summary>
            scoretext.Content = "Skor: " + score;

            /// <summary>
            /// ne zaman skor artacağı ayarlandı
            /// </summary>
            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width - 15, player.Height);
            obstacleHitBox = new Rect(Canvas.GetLeft(obstacle), Canvas.GetTop(obstacle), obstacle.Width, obstacle.Height);
            groundHitBox = new Rect(Canvas.GetLeft(ground), Canvas.GetTop(ground), ground.Width, ground.Height);

 
            /// <summary>
            /// oyuncu ve engelin kesiştiği yerde yapılacak ayarlar
            /// </summary>
            if (playerHitBox.IntersectsWith(groundHitBox))
            {
                speed = 0;

                Canvas.SetTop(player, Canvas.GetTop(ground) - player.Height);

                jumping = false;

                spriteIndex += .5;

                if (spriteIndex > 8)
                {
                    spriteIndex = 1;
                }

                RunSprite(spriteIndex);

            }

            /// <summary>
            /// zıplayınca hız ve güç ayarlandı
            /// </summary>
            if (jumping == true)
            {
                speed = -9;

                force -= 1;
            }
            else
            {
                speed = 12;
            }

            if (force < 0)
            {
                jumping = false;
            }

            /// <summary>
            /// engelle karşılaşınca yapılması gerekenler ayarlandı
            /// </summary>
            if (Canvas.GetLeft(obstacle) < -50)
            {
                Canvas.SetLeft(obstacle, 950);

                Canvas.SetTop(obstacle, obstaclePosition[rnd.Next(0, obstaclePosition.Length)]);

                score += 1;
            }
            /// <summary>
            /// oyuncu ve engelin kesiştiği yerde yapılacak ayarlar
            /// </summary>
            if (playerHitBox.IntersectsWith(obstacleHitBox))
            {

                gameOver = true;

                gameTimer.Stop();
            }
            /// <summary>
            /// oyun bitince gösterilecek ekran
            /// </summary>
            if (gameOver == true)
            {
                obstacle.Stroke = Brushes.Black;
                obstacle.StrokeThickness = 1;

                player.Stroke = Brushes.Red;
                player.StrokeThickness = 1;

                scoretext.Content = "Skor: " + score + " Tekrar Başlamak İçin Enter'e basınız";
            }
            else
            {
                player.StrokeThickness = 0;
                obstacle.StrokeThickness = 0;
            }
        }

        /// <summary>
        /// enter a basınca oyun baştan başlasın
        /// </summary>
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && gameOver == true)
            {

                /// <summary>
                /// Veri tabanına kayıt işlemleri yapıldı
                /// </summary>
                baglanti.Open();
                komut.Connection = baglanti;
                komut.CommandText = "INSERT INTO Skor(Skor) VALUES('"+ score +"')";
                komut.ExecuteNonQuery();
                
                StartGame();
            }
        }

        /// <summary>
        /// space ye basınca zıplasın ve güç ile hız ayarlansın
        /// </summary>
        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && jumping == false && Canvas.GetTop(player) > 260)
            {
                jumping = true;
                force = 15;
                speed = -12;

                /// <summary>
                /// oyuncu resmi eklendl
                /// </summary>
                playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
            }
        }

        /// <summary>
        /// oyun başlatılınca yapılacak ayarlar
        /// </summary>
        private void StartGame()
        {
            Canvas.SetLeft(background, 0);
            Canvas.SetLeft(background2, 1262);

            Canvas.SetLeft(player, 110);
            Canvas.SetTop(player, 140);

            Canvas.SetLeft(obstacle, 950);
            Canvas.SetTop(obstacle, 310);

            RunSprite(1);

            obstacleSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/obstacle-1.gif"));
            obstacle.Fill = obstacleSprite;

            jumping = false;
            gameOver = false;
            score = 0;

            scoretext.Content = "Skor: " + score;

            gameTimer.Start();

        }

        /// <summary>
        /// Koşarlen oyuncu gifinin hareket ettirlimesini ayarlar
        /// </summary>
        private void RunSprite(double i)
        {
            switch (i)
            {
                case 1:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
                case 2:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
                case 3:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
                case 4:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
                case 5:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
                case 6:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
                case 7:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
                case 8:
                    playerSprite.ImageSource = new BitmapImage(new Uri("C:/Users/Ömer Kayaoğlu/source/repos/T-Rex_Game/T-Rex_Game/Resimler/running.gif"));
                    break;
            }

            player.Fill = playerSprite;
        }
    }
}

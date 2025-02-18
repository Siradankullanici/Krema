using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Krema
{
    public partial class Krema : Form
    {
        private readonly Timer hareketZamani;
        private readonly Timer taklaZamani;
        private readonly Timer teleportZamani;
        private readonly Timer teleportSureZamani;
        private readonly Timer ozellikZamani;
        private int xHiz = 5;
        private int yHiz = 5;
        private double taklaAcisi = 0;
        private double taklaDonusHedef = 0;
        private Point orijinalMerkez;
        private const double taklaArtis = 10 * Math.PI / 180;
        private const int taklaYaricap = 50;
        private readonly SoundPlayer oynatici;
        private int hizliTiklamaSayisi = 0;
        private DateTime sonTiklamaZamani = DateTime.MinValue;
        private bool teleportModuAktif = false;
        private readonly Random rastgele = new Random();

        public Krema()
        {
            InitializeComponent();
            hareketZamani = new Timer { Interval = 10 };
            hareketZamani.Tick += HareketZamaniTik;
            taklaZamani = new Timer { Interval = 10 };
            taklaZamani.Tick += TaklaZamaniTik;
            teleportZamani = new Timer { Interval = 200 };
            teleportZamani.Tick += TeleportZamaniTik;
            teleportSureZamani = new Timer { Interval = 10000 };
            teleportSureZamani.Tick += TeleportSureZamaniTik;
            ozellikZamani = new Timer { Interval = 7000 };
            ozellikZamani.Tick += OzellikZamaniTik;
            Load += FormYuklendi;
            Click += FormTiklandi;
            oynatici = new SoundPlayer(new MemoryStream(Properties.Resources.wait_wait_wait_what_the_hell));
        }

        private void FormYuklendi(object sender, EventArgs e)
        {
            hareketZamani.Start();
            ozellikZamani.Start();
            oynatici.PlayLooping();
        }

        private void HareketZamaniTik(object sender, EventArgs e)
        {
            if (teleportModuAktif || taklaZamani.Enabled) return;
            Rectangle ekran = Screen.PrimaryScreen.Bounds;
            Left += xHiz;
            Top += yHiz;
            if (Left <= 0 || Right >= ekran.Width)
                xHiz = -xHiz;
            if (Top <= 0 || Bottom >= ekran.Height)
                yHiz = -yHiz;
        }

        private void FormTiklandi(object sender, EventArgs e)
        {
            if (teleportModuAktif || taklaZamani.Enabled) return;
            TimeSpan fark = DateTime.Now - sonTiklamaZamani;
            if (fark.TotalMilliseconds < 500)
                hizliTiklamaSayisi++;
            else
                hizliTiklamaSayisi = 1;
            sonTiklamaZamani = DateTime.Now;
            if (hizliTiklamaSayisi >= 5)
            {
                TeleportModunuBaslat();
                hizliTiklamaSayisi = 0;
                return;
            }
            // Tıklayınca 3 tam tur dönecek şekilde takla modunu başlat
            hareketZamani.Stop();
            taklaAcisi = 0;
            taklaDonusHedef = 2 * Math.PI * 3;
            orijinalMerkez = new Point(Left + Width / 2, Top + Height / 2);
            taklaZamani.Start();
        }

        private void TaklaZamaniTik(object sender, EventArgs e)
        {
            if (teleportModuAktif) return;
            taklaAcisi += taklaArtis;
            if (taklaAcisi >= taklaDonusHedef)
            {
                taklaZamani.Stop();
                hareketZamani.Start();
                return;
            }
            int ofsetX = (int)(taklaYaricap * Math.Cos(taklaAcisi));
            int ofsetY = (int)(taklaYaricap * Math.Sin(taklaAcisi));
            int yeniMerkezX = orijinalMerkez.X + ofsetX;
            int yeniMerkezY = orijinalMerkez.Y + ofsetY;
            Left = yeniMerkezX - Width / 2;
            Top = yeniMerkezY - Height / 2;
        }

        private void TeleportZamaniTik(object sender, EventArgs e)
        {
            Rectangle ekran = Screen.PrimaryScreen.Bounds;
            int yeniX = rastgele.Next(ekran.Width - Width);
            int yeniY = rastgele.Next(ekran.Height - Height);
            Left = yeniX;
            Top = yeniY;
        }

        private void TeleportSureZamaniTik(object sender, EventArgs e)
        {
            TeleportModunuDurdur();
        }

        private void OzellikZamaniTik(object sender, EventArgs e)
        {
            int secim = rastgele.Next(5);
            switch (secim)
            {
                case 0:
                    // Arka plan rengini değiştir
                    BackColor = Color.FromArgb(rastgele.Next(256), rastgele.Next(256), rastgele.Next(256));
                    break;
                case 1:
                    // Form boyutunu değiştir (min 200, max 500)
                    int yeniGenislik = rastgele.Next(200, 500);
                    int yeniYukseklik = rastgele.Next(200, 500);
                    Size = new Size(yeniGenislik, yeniYukseklik);
                    break;
                case 2:
                    // Opaklığı değiştir (0.5 ile 1.0 arasında)
                    Opacity = rastgele.NextDouble() * 0.5 + 0.5;
                    break;
                case 3:
                    // Rastgele mesaj göster
                    string[] mesajlar = { "Selam!", "Merhaba!", "Nasılsın?", "Ne yapıyorsun?", "Hadi eğlenelim!" };
                    Text = mesajlar[rastgele.Next(mesajlar.Length)];
                    break;
                case 4:
                    // Mini takla: 1 tam tur hızlıca dön
                    if (!taklaZamani.Enabled)
                    {
                        taklaAcisi = 0;
                        taklaDonusHedef = 2 * Math.PI;
                        orijinalMerkez = new Point(Left + Width / 2, Top + Height / 2);
                        taklaZamani.Start();
                    }
                    break;
            }
        }

        private void TeleportModunuBaslat()
        {
            teleportModuAktif = true;
            hareketZamani.Stop();
            taklaZamani.Stop();
            teleportZamani.Start();
            teleportSureZamani.Start();
        }

        private void TeleportModunuDurdur()
        {
            teleportModuAktif = false;
            teleportZamani.Stop();
            teleportSureZamani.Stop();
            hareketZamani.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBox.Show(new Form { TopMost = true }, "Krema", "Krema", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MessageBox.Show(new Form { TopMost = true }, "Kanala Abone Ol", "Kanala Abone Ol", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Krema
{
    public partial class Krema : Form
    {
        private static int aktifFormSayisi = 0;
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
        private readonly Timer shrinkTimer;
        private readonly Timer coverScreenTimer;
        private readonly Timer shapeMorphTimer;
        private readonly Timer rotateShrinkTimer;
        private readonly Timer shakeTimer;
        private readonly Timer blinkTimer;
        private int shrinkStep = 5;
        private Size orjinalBoyut;
        private Rectangle ekranSinirlari;
        private int coverStep = 20;
        private int shapeMorphSayisi = 0;
        private bool elipsMi = false;
        private double rotateAngle = 0;
        private int rotateShrinkDelta = 10;
        private int shakeSayisi = 0;
        private const int maksShakeSayisi = 10;
        private Point orijinalKonum;
        private int blinkSayisi = 0;
        private const int maksBlinkSayisi = 6;

        public Krema()
        {
            InitializeComponent();
            aktifFormSayisi++;
            hareketZamani = new Timer { Interval = 10 };
            hareketZamani.Tick += HareketZamaniTik;
            taklaZamani = new Timer { Interval = 10 };
            taklaZamani.Tick += TaklaZamaniTik;
            teleportZamani = new Timer { Interval = 200 };
            teleportZamani.Tick += TeleportZamaniTik;
            teleportSureZamani = new Timer { Interval = 10000 };
            teleportSureZamani.Tick += TeleportSureZamaniTik;
            ozellikZamani = new Timer { Interval = 3000 };
            ozellikZamani.Tick += OzellikZamaniTik;
            shrinkTimer = new Timer { Interval = 50 };
            shrinkTimer.Tick += ShrinkTimerTick;
            coverScreenTimer = new Timer { Interval = 50 };
            coverScreenTimer.Tick += CoverScreenTimerTick;
            shapeMorphTimer = new Timer { Interval = 1000 };
            shapeMorphTimer.Tick += ShapeMorphTimerTick;
            rotateShrinkTimer = new Timer { Interval = 50 };
            rotateShrinkTimer.Tick += RotateShrinkTimerTick;
            shakeTimer = new Timer { Interval = 50 };
            shakeTimer.Tick += ShakeTimerTick;
            blinkTimer = new Timer { Interval = 200 };
            blinkTimer.Tick += BlinkTimerTick;
            Load += FormYuklendi;
            Click += FormTiklandi;
            oynatici = new SoundPlayer(new MemoryStream(Properties.Resources.wait_wait_wait_what_the_hell));
        }

        private void FormYuklendi(object gonderen, EventArgs e)
        {
            hareketZamani.Start();
            ozellikZamani.Start();
            oynatici.PlayLooping();
        }

        private void HareketZamaniTik(object gonderen, EventArgs e)
        {
            if (teleportModuAktif || taklaZamani.Enabled)
                return;
            Rectangle ekran = Screen.PrimaryScreen.Bounds;
            Left += xHiz;
            Top += yHiz;
            if (Left <= 0 || Right >= ekran.Width)
                xHiz = -xHiz;
            if (Top <= 0 || Bottom >= ekran.Height)
                yHiz = -yHiz;
        }

        private void FormTiklandi(object gonderen, EventArgs e)
        {
            if (teleportModuAktif || taklaZamani.Enabled)
                return;
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
            hareketZamani.Stop();
            taklaAcisi = 0;
            taklaDonusHedef = 2 * Math.PI * 3;
            orijinalMerkez = new Point(Left + Width / 2, Top + Height / 2);
            taklaZamani.Start();
        }

        private void TaklaZamaniTik(object gonderen, EventArgs e)
        {
            if (teleportModuAktif)
                return;
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

        private void TeleportZamaniTik(object gonderen, EventArgs e)
        {
            Rectangle ekran = Screen.PrimaryScreen.Bounds;
            int yeniX = rastgele.Next(ekran.Width - Width);
            int yeniY = rastgele.Next(ekran.Height - Height);
            Left = yeniX;
            Top = yeniY;
        }

        private void TeleportSureZamaniTik(object gonderen, EventArgs e)
        {
            TeleportModunuDurdur();
        }

        private void OzellikZamaniTik(object gonderen, EventArgs e)
        {
            int secim = rastgele.Next(12);
            switch (secim)
            {
                case 0:
                    BackColor = Color.FromArgb(rastgele.Next(256), rastgele.Next(256), rastgele.Next(256));
                    break;
                case 1:
                    int yeniGenislik = rastgele.Next(200, 500);
                    int yeniYukseklik = rastgele.Next(200, 500);
                    Size = new Size(yeniGenislik, yeniYukseklik);
                    break;
                case 2:
                    Opacity = rastgele.NextDouble() * 0.5 + 0.5;
                    break;
                case 3:
                    string[] mesajlar = { "Selam!", "Merhaba!", "Nasılsın?", "Ne yapıyorsun?", "Hadi eğlenelim!" };
                    Text = mesajlar[rastgele.Next(mesajlar.Length)];
                    break;
                case 4:
                    if (!taklaZamani.Enabled)
                    {
                        taklaAcisi = 0;
                        taklaDonusHedef = 2 * Math.PI;
                        orijinalMerkez = new Point(Left + Width / 2, Top + Height / 2);
                        taklaZamani.Start();
                    }
                    break;
                case 5:
                    BasilarakKuculEffectBaslat();
                    break;
                case 6:
                    EkraniKaplaEffectBaslat();
                    break;
                case 7:
                    SekilDegistirEffectBaslat();
                    break;
                case 8:
                    ParcalaraBolEffectBaslat();
                    break;
                case 9:
                    DonKuculEffectBaslat();
                    break;
                case 10:
                    SarsilEffectBaslat();
                    break;
                case 11:
                    YanipSonEffectBaslat();
                    break;
            }
        }

        private void BasilarakKuculEffectBaslat()
        {
            hareketZamani.Stop();
            taklaZamani.Stop();
            teleportZamani.Stop();
            ozellikZamani.Stop();
            shrinkStep = 5;
            orjinalBoyut = Size;
            shrinkTimer.Start();
        }

        private void ShrinkTimerTick(object gonderen, EventArgs e)
        {
            if (Width > shrinkStep * 2 && Height > shrinkStep * 2)
            {
                int yeniGenislik = Width - shrinkStep;
                int yeniYukseklik = Height - shrinkStep;
                int merkezX = Left + Width / 2;
                int merkezY = Top + Height / 2;
                Size = new Size(yeniGenislik, yeniYukseklik);
                Left = merkezX - yeniGenislik / 2;
                Top = merkezY - yeniYukseklik / 2;
            }
            else
            {
                shrinkTimer.Stop();
                SpawnMultipleKrema(5);
                hareketZamani.Start();
                ozellikZamani.Start();
            }
        }

        private void SpawnMultipleKrema(int adet)
        {
            Rectangle ekran = Screen.PrimaryScreen.Bounds;
            for (int i = 0; i < adet; i++)
            {
                if (aktifFormSayisi < 10)
                {
                    Krema yeniKrema = new Krema();
                    yeniKrema.StartPosition = FormStartPosition.Manual;
                    yeniKrema.Left = rastgele.Next(ekran.Width - yeniKrema.Width);
                    yeniKrema.Top = rastgele.Next(ekran.Height - yeniKrema.Height);
                    yeniKrema.Show();
                }
            }
        }

        private void EkraniKaplaEffectBaslat()
        {
            hareketZamani.Stop();
            taklaZamani.Stop();
            teleportZamani.Stop();
            ozellikZamani.Stop();
            ekranSinirlari = new Rectangle(100, 100, 800, 600);
            orjinalBoyut = Size;
            coverScreenTimer.Start();
        }

        private void CoverScreenTimerTick(object gonderen, EventArgs e)
        {
            int hedefX = 0;
            int hedefY = 0;
            int hedefGenislik = ekranSinirlari.Width;
            int hedefYukseklik = ekranSinirlari.Height;
            bool degisti = false;
            if (Left > hedefX)
            {
                Left = Math.Max(Left - coverStep, hedefX);
                degisti = true;
            }
            if (Top > hedefY)
            {
                Top = Math.Max(Top - coverStep, hedefY);
                degisti = true;
            }
            if (Width < hedefGenislik)
            {
                Width = Math.Min(Width + coverStep, hedefGenislik);
                degisti = true;
            }
            if (Height < hedefYukseklik)
            {
                Height = Math.Min(Height + coverStep, hedefYukseklik);
                degisti = true;
            }
            if (!degisti)
            {
                coverScreenTimer.Stop();
                Timer geriYukleTimer = new Timer { Interval = 2000 };
                geriYukleTimer.Tick += (s, ev) =>
                {
                    geriYukleTimer.Stop();
                    Size = orjinalBoyut;
                    StartPosition = FormStartPosition.CenterScreen;
                    Left = (ekranSinirlari.Width - Width) / 2;
                    Top = (ekranSinirlari.Height - Height) / 2;
                    hareketZamani.Start();
                    ozellikZamani.Start();
                };
                geriYukleTimer.Start();
            }
        }

        private void SekilDegistirEffectBaslat()
        {
            hareketZamani.Stop();
            taklaZamani.Stop();
            teleportZamani.Stop();
            ozellikZamani.Stop();
            shapeMorphSayisi = 0;
            elipsMi = false;
            shapeMorphTimer.Start();
        }

        private void ShapeMorphTimerTick(object gonderen, EventArgs e)
        {
            if (!elipsMi)
            {
                GraphicsPath yol = new GraphicsPath();
                yol.AddEllipse(0, 0, Width, Height);
                Region = new Region(yol);
            }
            else
            {
                Region = null;
            }
            elipsMi = !elipsMi;
            shapeMorphSayisi++;
            if (shapeMorphSayisi >= 6)
            {
                shapeMorphTimer.Stop();
                Region = null;
                hareketZamani.Start();
                ozellikZamani.Start();
            }
        }

        private void ParcalaraBolEffectBaslat()
        {
            hareketZamani.Stop();
            taklaZamani.Stop();
            teleportZamani.Stop();
            ozellikZamani.Stop();
            ParcalaraBol();
            hareketZamani.Start();
            ozellikZamani.Start();
        }

        private void ParcalaraBol()
        {
            if (BackgroundImage == null)
                return;
            Bitmap orjinalResim = new Bitmap(BackgroundImage);
            int parcaGenislik = orjinalResim.Width / 2;
            int parcaYukseklik = orjinalResim.Height / 2;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Bitmap parca = new Bitmap(parcaGenislik, parcaYukseklik);
                    using (Graphics g = Graphics.FromImage(parca))
                    {
                        g.DrawImage(orjinalResim,
                            new Rectangle(0, 0, parcaGenislik, parcaYukseklik),
                            new Rectangle(j * parcaGenislik, i * parcaYukseklik, parcaGenislik, parcaYukseklik),
                            GraphicsUnit.Pixel);
                    }
                    Form parcaForm = new Form
                    {
                        FormBorderStyle = FormBorderStyle.None,
                        StartPosition = FormStartPosition.Manual,
                        Size = new Size(parcaGenislik, parcaYukseklik),
                        BackgroundImage = parca
                    };
                    parcaForm.Left = Left + j * parcaGenislik;
                    parcaForm.Top = Top + i * parcaYukseklik;
                    parcaForm.Show();
                }
            }
        }

        private void DonKuculEffectBaslat()
        {
            hareketZamani.Stop();
            taklaZamani.Stop();
            teleportZamani.Stop();
            ozellikZamani.Stop();
            rotateAngle = 0;
            rotateShrinkTimer.Start();
        }

        private void RotateShrinkTimerTick(object gonderen, EventArgs e)
        {
            rotateAngle += 0.2;
            if (Width > rotateShrinkDelta * 2 && Height > rotateShrinkDelta * 2)
            {
                int yeniGenislik = Width - rotateShrinkDelta;
                int yeniYukseklik = Height - rotateShrinkDelta;
                int merkezX = Left + Width / 2;
                int merkezY = Top + Height / 2;
                Size = new Size(yeniGenislik, yeniYukseklik);
                Left = merkezX - yeniGenislik / 2;
                Top = merkezY - yeniYukseklik / 2;
            }
            else
            {
                rotateShrinkTimer.Stop();
                SpawnMultipleKrema(5);
                hareketZamani.Start();
                ozellikZamani.Start();
                return;
            }
            Rectangle dikdortgen = new Rectangle(0, 0, Width, Height);
            Point merkez = new Point(Width / 2, Height / 2);
            Point[] noktalar = new Point[4];
            noktalar[0] = NoktaDon(new Point(0, 0), merkez, rotateAngle);
            noktalar[1] = NoktaDon(new Point(Width, 0), merkez, rotateAngle);
            noktalar[2] = NoktaDon(new Point(Width, Height), merkez, rotateAngle);
            noktalar[3] = NoktaDon(new Point(0, Height), merkez, rotateAngle);
            GraphicsPath yol = new GraphicsPath();
            yol.AddPolygon(noktalar);
            Region = new Region(yol);
        }

        private Point NoktaDon(Point nokta, Point pivot, double aci)
        {
            double sin = Math.Sin(aci);
            double cos = Math.Cos(aci);
            int x = nokta.X - pivot.X;
            int y = nokta.Y - pivot.Y;
            int yeniX = (int)(x * cos - y * sin);
            int yeniY = (int)(x * sin + y * cos);
            return new Point(yeniX + pivot.X, yeniY + pivot.Y);
        }

        private void SarsilEffectBaslat()
        {
            hareketZamani.Stop();
            ozellikZamani.Stop();
            shakeSayisi = 0;
            shakeTimer.Start();
        }

        private void ShakeTimerTick(object gonderen, EventArgs e)
        {
            if (shakeSayisi == 0)
            {
                orijinalKonum = Location;
            }
            if (shakeSayisi < maksShakeSayisi)
            {
                int offsetX = rastgele.Next(-5, 6);
                int offsetY = rastgele.Next(-5, 6);
                Location = new Point(orijinalKonum.X + offsetX, orijinalKonum.Y + offsetY);
                shakeSayisi++;
            }
            else
            {
                shakeTimer.Stop();
                Location = orijinalKonum;
                shakeSayisi = 0;
                hareketZamani.Start();
                ozellikZamani.Start();
            }
        }

        private void YanipSonEffectBaslat()
        {
            hareketZamani.Stop();
            ozellikZamani.Stop();
            blinkSayisi = 0;
            blinkTimer.Start();
        }

        private void BlinkTimerTick(object gonderen, EventArgs e)
        {
            Visible = !Visible;
            blinkSayisi++;
            if (blinkSayisi >= maksBlinkSayisi)
            {
                blinkTimer.Stop();
                Visible = true;
                blinkSayisi = 0;
                hareketZamani.Start();
                ozellikZamani.Start();
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
            blinkTimer.Stop();
            Timer mesajTimer = new Timer { Interval = 5000 };
            mesajTimer.Tick += (s, ev) =>
            {
                mesajTimer.Stop();
                Environment.Exit(0);
            };
            MessageBox.Show(new Form { TopMost = true }, "Krema", "Krema", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MessageBox.Show(new Form { TopMost = true }, "Kanala Abone Ol", "Kanala Abone Ol", MessageBoxButtons.OK, MessageBoxIcon.Information);
            mesajTimer.Start();
        }
    }
}

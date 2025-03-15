using System;
using System.Windows.Forms;

namespace Krema
{
    public partial class Krema1 : Form
    {
        public Krema1()
        {
            InitializeComponent();
        }

        // Form yüklendiğinde çalışır
        private void Krema1_Load(object sender, EventArgs e)
        {
        }

        // pictureBox1 tıklandığında çalışır
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // İndirme tuşuna basıldığında uygulamanın yeni bir örneğini göster
            Krema kremaForm = new Krema();
            kremaForm.Show();
        }
    }
}

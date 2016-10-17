using System;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Net;

namespace exportDPSonline
{
    public partial class parametri : Form
    {
        public parametri()

        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists("c:/file_dpsonline/") != true)
            {
                Directory.CreateDirectory("c:/file_dpsonline/");
            }
            using (StreamWriter scrivi = new StreamWriter("c:/file_dpsonline/ricarico.txt"))
            {
                scrivi.WriteLine(textBox1.Text);
            }
        }

        private void parametri_Load(object sender, EventArgs e)
        {
            if (File.Exists("c:/file_dpsonline/ricarico.txt") == false)
            {
                File.Create("c:/file_dpsonline/ricarico.txt");
            }
            if (this.Visible == true)
            {
                timer1.Enabled = true;
            }
            using (StreamReader lettore=new StreamReader("c:/file_dpsonline/ricarico.txt"))
            {
                textBox1.Text = lettore.ReadLine().ToString();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {           
            Form apri = new Form1();
            timer1.Enabled = false;
            apri.Show();            
        }
        private void btnEsci_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnInvia_Click(object sender, EventArgs e)
        {
            Form apri = new Form1();
            timer1.Enabled = false;
            apri.Show();
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            ftp();           
        }
        public void ftp()
        {
            string filename = "c:/file_dpsonline/estratto.csv";
            string ftpServerIP = "ftp.dpsonline.it";
            string ftpUserName = "farmaciapiaggio_gestionale";
            string ftpPassword = "P14gg10";

            FileInfo objFile = new FileInfo(filename);
            FtpWebRequest objFTPRequest;

            // Create FtpWebRequest object 
            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + objFile.Name));

            // Set Credintials
            objFTPRequest.Credentials = new NetworkCredential(ftpUserName, ftpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            objFTPRequest.KeepAlive = false;

            // Set the data transfer type.
            objFTPRequest.UseBinary = true;

            // Set content length
            objFTPRequest.ContentLength = objFile.Length;

            // Set request method
            objFTPRequest.Method = WebRequestMethods.Ftp.UploadFile;

            // Set buffer size
            int intBufferLength = 16 * 1024;
            byte[] objBuffer = new byte[intBufferLength];

            // Opens a file to read
            FileStream objFileStream = objFile.OpenRead();

            try
            {
                // Get Stream of the file
                Stream objStream = objFTPRequest.GetRequestStream();

                int len = 0;

                while ((len = objFileStream.Read(objBuffer, 0, intBufferLength)) != 0)
                {
                    // Write file Content 
                    objStream.Write(objBuffer, 0, len);

                }

                objStream.Close();
                objFileStream.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
    }
}

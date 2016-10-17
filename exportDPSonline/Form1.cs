using System;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;
using System.Net;


namespace exportDPSonline
{
    public partial class Form1 : Form
    {
        int contatore_ricarico = 0;
        int contatore_bancadati = 0;
        string idsessione;
        string descrizioneBreve;
        string codiceSitoLogistico;
        string codice;
        string esitoServizio;
        string url;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Visible = true;
            Login();
            start();
            messaggio();
            this.Close();
        }

        private void Login()
        {

            string leggoFileRicarico = "";
            double ricarico = 0;
            string urlFck = "";
            string userFck = "";
            string passwordFck = "";
            using (StreamReader leggi = new StreamReader("c:/file_dpsonline/ricarico.txt"))
            {
                while ((leggoFileRicarico = leggi.ReadLine()) != null)
                {
                    ricarico = double.Parse(leggoFileRicarico);
                    urlFck = leggi.ReadLine() + 1;
                    userFck = leggi.ReadLine() + 2;
                    passwordFck = leggi.ReadLine() + 3;
                }
            }
            fckLogin.LoginInputBean login = new fckLogin.LoginInputBean();
            login.userName = "FCK00017453";
            login.password = "MXH3GM5YZN";
            login.nomeTerminale = "SERVER";

            fckLogin.LoginOutputBean output = new fckLogin.LoginOutputBean();

            fckLogin.farmaclick2010001Service webservice = new fckLogin.farmaclick2010001Service();
            webservice.Url = ("https://secure.infarmaclick.com/public_server_pro/Farmaclick2010001FCKLogin");

            output = webservice.FCKLogin(login);

            idsessione = output.arrayFornitori[0].IDSessione;
            descrizioneBreve = output.arrayFornitori[0].descrizioneBreve;
            codiceSitoLogistico = output.arrayFornitori[0].codiceSitoLogistico;
            codice = output.arrayFornitori[0].codice;
            esitoServizio = output.esitoServizio.ToString();
            var servizi = output.arrayFornitori[0].arrayServizi;
            for (int i = 0; i < servizi.Length - 1; i++)
            {
                var url1 = servizi[i].url1;
                url = servizi[19].url1;
            }

        }

        private void messaggio()
        {
            MessageBox.Show("Export Completato!Prezzi con ricarico:" + contatore_ricarico + ",prezzi banca dati:" + contatore_bancadati);
            DialogResult messaggio = MessageBox.Show("Invio il File?", "", MessageBoxButtons.YesNo);
            if (messaggio == DialogResult.No)
            {
                Close();
            }
            if (messaggio == DialogResult.Yes)
            {
                ftp();
            }

        }

        public void start()
        {
            label1.Text = "Sto Estraendo il file";
            export();
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
        public void export()
        {

            string leggoFileRicarico = "";
            double ricarico = 0;
            //leggo il ricarico
            using (StreamReader leggi = new StreamReader("c:/file_dpsonline/ricarico.txt"))
            {
                while ((leggoFileRicarico = leggi.ReadLine()) != null)
                {
                    ricarico = double.Parse(leggoFileRicarico);
                }
            }
            try
            {
                MySqlDataReader lettore = null;
                MySqlConnection conn = new MySqlConnection("Server=localhost;Port=3306;Uid=root;Pwd=phs2012;Database=bancadati;");
                conn.Open();
                MySqlCommand query = new MySqlCommand("select bf2000.bfmagazzino.Descrizione," +
                                                      "bancadati.anagraficabancadati.kean," +
                                                      "bancadati.anagraficabancadati.tabiva," +
                                                      "bancadati.anagraficabancadati.MOTIVOINVENDIBILITA," +
                                                      "bancadati.anagraficabancadati.TABDEGRASSI," +
                                                      "bancadati.tabelladegrassi.Descrizione," +
                                                      "bancadati.anagraficagmp.Descrizione," +
                                                      "bancadati.anagraficabancadati.PrezzoEuro1," +
                                                      "bf2000.bfMagazzino.PrezzoDiscrezionale," +
                                                      "(select bf2000.bfmagazzino2.GiacenzaAttuale from bf2000.bfmagazzino2 where bf2000.bfmagazzino2.CodiceProdotto=bf2000.bfMagazzino.CodiceProdotto)," +
                                                      "bf2000.bfMagazzino.PrezzoDiscrezionalePrevalente," +
                                                      "(select bancadati.anagraficaditte.ragionesociale from bancadati.anagraficaditte where bancadati.anagraficaditte.codice = bancadati.anagraficabancadati.KDIT1)," +
                                                      "bf2000.tabellalistini.CodiceProdotto," +
                                                      "max(PrezzoEuro)," +
                                                      "bf2000.bfmagazzino.CostoMedio " +
                                                      "from bf2000.tabellalistini " +
                                                      "inner join bf2000.bfmagazzino " +
                                                      "on (bf2000.tabellalistini.CodiceProdotto = bf2000.bfmagazzino.CodiceProdotto) " +
                                                      "inner join bancadati.anagraficabancadati " +
                                                      "on (bf2000.tabellalistini.CodiceProdotto = bancadati.anagraficabancadati.km10) " +
                                                      "inner join bancadati.tabelladegrassi " +
                                                      "on (bancadati.tabelladegrassi.Codice = bancadati.anagraficabancadati.TABDEGRASSI) " +
                                                      "inner join bancadati.anagraficagmp " +
                                                      "on (bancadati.anagraficagmp.codice = bancadati.anagraficabancadati.Kgm) " +
                                                      "where bancadati.anagraficabancadati.TABDEGRASSI <>'1151' " +
                                                      "group by 1, 2, 3, 4, 5, 6, 7,8,9,10,11,12,13", conn);


                lettore = query.ExecuteReader();
                using (StreamWriter scriviFile = new StreamWriter("c:/file_dpsonline/estratto.csv"))
                {
                    string testa_descrizione = "PRODOTTO";
                    string testa_ean = "EAN";
                    string testa_minsan = "MINSAN";
                    string testa_prezzo_Bancadati = "PREZZO BDF";
                    string testa_prezzo = "PREZZO WEB";
                    string testa_costo = "COSTO";
                    string testa_disp = "DISP";
                    string testa_codice_degrassi = "CODICE DEGRASSI";
                    string testa_degrassi = "DEGRASSI";
                    string testa_iva = "IVA";
                    string testa_revoca = "REVOCA";
                    string testa_gmp = "GMP";
                    string testa_ditta = "DITTA";
                    string disponibiltà = "DISP";

                    scriviFile.WriteLine("{0,-41};{1,-13};{2,-13};{3,-10};{4,-10};{5,-8};{6,-4};{7,-15};{8,-41};{9,-3};{10,-6};{11,-70};{12,-50};{13,-4}", testa_descrizione, testa_ean, testa_minsan, testa_prezzo_Bancadati, testa_prezzo, testa_costo, testa_disp, testa_codice_degrassi, testa_degrassi, testa_iva, testa_revoca, testa_gmp, testa_ditta,disponibiltà);
                    while (lettore.Read())
                    {
                        string descrizione = lettore.GetValue(0).ToString();
                        string ean = lettore.GetValue(1).ToString();
                        string minsan = lettore.GetValue(12).ToString();
                        int disponibile=0;
                        dispo(minsan, disponibile);
                        string iva = lettore.GetValue(2).ToString();
                        if (iva == "20")
                        {
                            iva = "22";
                        }
                        if (iva == "21")
                        {
                            iva = "22";
                        }
                        var costo = double.Parse(lettore.GetValue(13).ToString());
                        var costo_medio = double.Parse(lettore.GetValue(14).ToString());
                        /*if (costo_medio > 0)
                        {
                            costo = costo_medio;
                        }*/
                        var prezzo_calcolato = (costo * ((ricarico / 100) + 1.00)) * (((double.Parse(iva)) / 100) + 1.00);
                        var prezzo = double.Parse(lettore.GetValue(7).ToString());
                        if (prezzo_calcolato < prezzo)
                        {
                            contatore_ricarico += 1;
                        }
                        else if (prezzo == 0.00)
                        {
                            contatore_ricarico += 1;
                        }
                        else if (prezzo_calcolato > prezzo)
                        {
                            prezzo_calcolato = prezzo;
                            contatore_bancadati += 1;
                        }
                        else if (prezzo_calcolato == prezzo)
                        {
                            prezzo_calcolato = prezzo;
                            contatore_bancadati += 1;
                        }
                        string gmp = lettore.GetValue(6).ToString();
                        var prezzoDiscrezionale = double.Parse(lettore.GetValue(8).ToString());
                        string prezzodiscrezionaleprevalente = lettore.GetValue(10).ToString();
                        string ditta = lettore.GetValue(11).ToString();
                        string disponibilita = lettore.GetValue(9).ToString();
                        string codiceDegrassi = lettore.GetValue(4).ToString();
                        string descrizioneDegrassi = lettore.GetValue(5).ToString();
                        string revoca = lettore.GetValue(3).ToString();
                        if (costo == 0.00)
                            if (prezzo_calcolato == 0.00)
                            {
                                continue;
                            }

                        scriviFile.WriteLine("{0,-41};{1,13};{2,13};{3,10};{4,10};{5,8};{6,4};{7,15};{8,-41};{9,3};{10,6};{11,70};{12,50};{13,1}", descrizione, ean, minsan, prezzo.ToString("0.00"), prezzo_calcolato.ToString("0.00"), costo.ToString("0.00"), disponibilita, codiceDegrassi, descrizioneDegrassi, iva, revoca, gmp, ditta,disponibile);
                    }
                    scriviFile.Close();
                }
                lettore.Close();
                conn.Close();
            }
            catch
            {

            }
        }

        private void dispo(string minsan, int disponibile)
        {
            FCKInfo.InfoComInputBean inbean = new FCKInfo.InfoComInputBean();
            FCKInfo.ArticoloInputBean[] articolibean = new FCKInfo.ArticoloInputBean[1];
            FCKInfo.ArticoloInputBean articolo = new FCKInfo.ArticoloInputBean();

            inbean.IDSessione = idsessione;
            inbean.codiceFornitore = codice;
            inbean.descrizioneMotivazioneMancanza = false;
            inbean.descrizioneMotivazioneMancanza = false;
            inbean.riferimentoOrdineFarmacia = "123456";


            articolibean[0] = new FCKInfo.ArticoloInputBean();
            articolibean[0].codiceProdotto = minsan;
            articolibean[0].quantitaRichiesta = 1;



            inbean.arrayArticoliInput = articolibean;

            FCKInfo.farmaclick2010001Service webservice2 = new FCKInfo.farmaclick2010001Service();
            webservice2.Url = url;
            FCKInfo.InfoComOutputBean outbean = webservice2.FCKInfoCom(inbean);

            //MessageBox.Show(outbean.esitoServizio.ToString());


            if (outbean.esitoServizio == 0)

                disponibile=int.Parse(outbean.arrayArticoli[0].quantitaConsegnata.ToString());
            else
                MessageBox.Show("va no");


        }

      
    }
}





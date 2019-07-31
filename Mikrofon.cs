 public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        public WaveIn wavein = null;
        public WaveOut waveout = null;
        public BufferedWaveProvider bwp = null;
        public Thread kanal;
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Microphone Aç")
            {                
                if (wavein != null)//Mikrofon doluysa geri dön.
                {
                    return;
                }
                pictureBox1.BackColor = Color.PaleGreen;//mikrofon işaretini yak
                button1.Text = "Microphone Kapat";//buton yazısını değiştir.
                wavein = new WaveIn(this.Handle);//yeni mikrofon oluştur.
                wavein.BufferMilliseconds = 25;//buffer saniyesi
                wavein.DataAvailable += wadein_DataAvaliable;//Data oldukça çalışacak metod.
                bwp = new BufferedWaveProvider(wavein.WaveFormat);//Buffer mikrofona byte'ları yüklemek için.
                waveout = new WaveOut();//Hoparlor.
                waveout.DesiredLatency = 100;
                waveout.Init(bwp);//hoparlor byte'ları doldur.
                wavein.StartRecording();//mikrofon dinlemeye başla
                waveout.Volume = 0.0f;//horparlor ses 0;
                waveout.Play();
            }
            else
            {
                pictureBox1.BackColor = Color.Red;//mikrofon işaretini söndür
                button1.Text = "Microphone Aç";//buton yazısını değiştir
                if (wavein != null)//mikrofon doluysa
                {
                    wavein.StopRecording();//dinlemeyi bırak
                    wavein.Dispose();//yoket
                    wavein = null;//içini boşal
                    bwp = null;//içini boşal
                    waveout.Stop();//durdur
                    waveout = null;//içini boşal
                }
            }
            
        }

        public bool gonder = false;
        private void wadein_DataAvaliable(object sender, WaveInEventArgs e)//Buffer byte ile dolunca çağırılan metod
        {
            if (bwp != null)//buffer boş değil ise
            {
                bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);//byte'ları doldur
                buffer = new byte[e.BytesRecorded];//server'da göndermek için yeni byte[] oluşturuyoruz
                buffer = e.Buffer;//server  byte'larını dolduruyoruz.
                gonder = true;//gonder seçeneğini açtık                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }
        public byte[] buffer;//server byte'ları
        public TcpListener dinlel;//server dinleyici
        public Socket socket;//veri gönderen.
        public List<Socket> tcpclientlist = new List<Socket>();//gelen client(bağlanan) tuttuğumuz yer.
        private void dinle()//server Başlat
        {
            dinlel = new TcpListener(10240);//10240 portunu dinlemeye başlat
            //dinlel.Server.MulticastLoopback = true;
            dinlel.Server.NoDelay = true;//beklemek yok
            dinlel.Server.ReceiveTimeout = 0;//bekleme 0
            dinlel.Server.SendTimeout = 0;//bekleme 0
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)//ip adresimizi bulup textbox'a yazıyoruz.
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBox1.Text = ip.ToString();
                }
            }
            bool don = true;
            dinlel.Start();//server'ı başlat.
            BrodCast();//bağlanan kişilere veri gönderen arkaplan çalışan metod.
            while (don)//dongu
            {
                if (kanal == null)//eğer kanal boş ise durdur
                {
                    don = false;
                    return;
                }
                socket = dinlel.AcceptSocket();//server'a bağlantı bekle
                socket.NoDelay = true;//beklemek yok.
                socket.ReceiveTimeout = 0;//bekleme 0
                socket.SendTimeout = 0; //bekleme 0               
                System.Net.IPEndPoint end = socket.RemoteEndPoint as System.Net.IPEndPoint;
                listBox1.Items.Add("BAĞLANDI:" + end.Address);//listbox'a bağlanan ip yaz
                tcpclientlist.Add(socket);//bağlananlar listesine ekle
                
            }
        }

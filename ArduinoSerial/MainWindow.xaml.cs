/*  ASM -  Arduino Serial Monitor
 *  
 *      aktualni verze: v1.1
 *  
 *  13.04.2020  -  14.04.2020   v1.0
 *  15.04.2020  -  xx.xx.xxxx   v1.1
 * 
 *  Jednoducha aplikace slouzici k zobrazeni seriove
 *  komunikace arduina (ci jineho zarizeni).
 * 
 *  Aplikace urcite obsahuje chyby, je to muj prvni
 *  opravdovy program v C# ktery mel za ukol realne
 *  otestovat me cerstve naucene znalosti C# a XAML.
 * 
 *  Je srandovni kdyz jsem si myslel ze to bude
 *  jednoduche. Abych tento program udelal, je
 *  potreba umet 3 jazyky. C (arduino), C# a XAML.
 * 
 * 
 *  Celkovy straveny cas programovanim: 1 hodina
 *  
 *  Celkovy straveny cas hledanim problemu ktery mam
 *  jenom ja a nejaky indicky programator: 8 hodin
 *  
 *  Celkovy cas napsani programu: cca 10 hodin
 * 
 *  Pro dalsi useless programy muzete jit tady
 *  http://www.davak.cz
 * 
 */

 /* Seznam změn
  * 
  * v1.1
  * Zmena rozmeru tlacitek.
  * 
  * 
  * v1.0
  * Vytvoren program. Zakladni komunikace přes predem dane parametry.
  * 
  *
  */

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
using System.IO.Ports;
using System.Threading;

namespace ArduinoSerial
{
    public partial class MainWindow : Window
    {

        bool isConnected = false;                                                                                    // stav jestli jsme pripojeni k arduinu
        public static string[] foundPorts;                                                                           // pole kde se ulozi seznam COM portu
        static SerialPort _serialPort;                                                                               //jeste jsem neprisel co to dela
        //SerialPort port;

        int[] baudrateValues = { 300, 600, 1200, 2400,4800,9600,14400,19200,288800,38400,57600,115200};
        int[] databitsValues = { 8 };
        string[] parityValues = { "None" };
        string[] stopbitsValues = { "One" };
        string[] handshakeValues = { "None" };

        int defaultBaudrate = 9600;
        int defaultDatabits = 8;
        string defaultParity = "None";
        string defaultStopbits = "One";
        string defaultHandshake = "None";

        public MainWindow()
        {
            InitializeComponent();
            findComPorts();
            _serialPort = new SerialPort();                                                                          //vytvori objekt SerialPort s parametry ktere se doplni po kliknuti
            
            foreach(int rate in baudrateValues)
            {
                cbBaudrate.Items.Add(rate);
            }
            foreach(int databits in databitsValues)
            {
                cbDatabits.Items.Add(databits);
            }
            foreach (string parity in parityValues)
            {
                cbParity.Items.Add(parity);
            }
            foreach (string stopbits in stopbitsValues)
            {
                cbStopbits.Items.Add(stopbits);
            }
            foreach (string handshake in handshakeValues)
            {
                cbHandshake.Items.Add(handshake);
            }
            cbBaudrate.SelectedItem = defaultBaudrate;
            cbDatabits.SelectedItem = defaultDatabits;
            cbStopbits.SelectedItem = defaultStopbits;
            cbParity.SelectedItem = defaultParity;
            cbHandshake.SelectedItem = defaultHandshake;


        }

        private void btnOnOff_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                connectToCOM();
            }
            else
            {
                disconnectFromCOM();
            }
        }

        //Method that finds available COM ports
        void findComPorts()
        {
            statusBarText.Text = "Searching COM ports";
            foundPorts = SerialPort.GetPortNames();
            cbComport.Items.Clear();
            foreach (string portName in foundPorts)                                                                        // for every COM port, it will made a loop
                {
                    cbComport.Items.Add(portName);                                                                         // adds found COM ports in combobox
                    if (foundPorts[0] != null)                                                                             // if first item in array is not null, it will select it
                    {
                    cbComport.SelectedItem = foundPorts[0];
                    }
            
            }
            //If no COM ports are found
            if (foundPorts.Length < 1)
            {
                statusBarText.Text = "No COM ports found";
            }
            //If we found some COM ports
            else if(foundPorts.Length > 0)
            {
                string myFoundPorts = foundPorts.Length.ToString();
                if(foundPorts.Length == 1)//is equal to 1 we print port
                    statusBarText.Text = (myFoundPorts + " COM port found");
                if (foundPorts.Length > 1)//if we have more than 1, we print ports
                    statusBarText.Text = (myFoundPorts + " COM ports found");
            }
        }

        private void connectToCOM()
        {
            if (cbComport.SelectedItem != null)
            {
                _serialPort.PortName = cbComport.Text;
                _serialPort.BaudRate = Int32.Parse(cbBaudrate.Text);
                _serialPort.DataBits = Int32.Parse(cbDatabits.Text);
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                try
                {
                    _serialPort.Open();
                    isConnected = true;
                    statusBarText.Text = "Connected";
                    statusBarRect.Fill = Brushes.LimeGreen;
                    btnDisconnect.Content = "Disconnect";
                }
                catch (Exception)
                {
                    //MessageBox.Show(ex.ToString());   //create a dialog window with exception info
                    statusBarText.Text = "Oops, something went wrong";
                    statusBarRect.Fill = Brushes.DarkBlue;
                }
            }
            


        }

        /*
         *
            if (SerialPort.GetPortNames().Length > 0)
            {

            }
            else
            {
                disconnectFromCOM();
            }
        */

        //Recieves data from serial communication
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;

            //if (sp.IsOpen)
            //{
                string indata = sp.ReadExisting();
                writeRecievedData(indata);
            //}
            //else
            //{
            //    disconnectFromCOM();
            //}
        }

        // Takes data from different thread, adding time and display it in textbox
        private void writeRecievedData(string text)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                //time
                string localTime = DateTime.Now.ToString("HH:mm:ss");

                //string textWithTime = ($"[{localTime}] {text}");
                //tbCommunication.AppendText(textWithTime);
                tbCommunication.AppendText(text);
                tbCommunication.ScrollToEnd();

            }));
        }

        private void disconnectFromCOM()
        {
            isConnected = false;
            _serialPort.Close();
            statusBarText.Text = "Disconnected";
            statusBarRect.Fill = System.Windows.Media.Brushes.Red;
            btnDisconnect.Content = "Connect";
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            findComPorts();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            tbCommunication.Clear();
        }

        //private void textboxInformation(string text)
        //{
        //    tbCommunication.AppendText("====== " + text + Environment.NewLine);
        //}

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string dataToSend = userInputText.ToString();
            _serialPort.Write(dataToSend);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        //==============================SMAZAT
        private void btnSendA(object sender, RoutedEventArgs e)
        {
            _serialPort.Write("A");
        }

        private void btnSendB(object sender, RoutedEventArgs e)
        {
            _serialPort.Write("B");
        }
    }
}

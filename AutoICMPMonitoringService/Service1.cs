using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Auto_ICMP_Monitoring.Data;
using System.Net;
using Aspose.Cells;
using System.Threading;
using Newtonsoft.Json;


namespace AutoICMPMonitoringService
{
    public partial class AutoICMPMonitoringService : ServiceBase
    {

        public const string pathWorkDir = "C:/Auto ICMP monitoring service";
        public const string pathLog = "C:/Auto ICMP monitoring service/Logs.txt";
        public const string basePathConfig = pathWorkDir + "/Config.json";
        static List<Device> devices;
        public AutoICMPMonitoringService()
        {
            InitializeComponent();
        }
        bool isServiceReady = false;
        protected override async void OnStart(string[] args)
        {
            DeviceMonitoring.TGSendMessage("⌛ Инициализация службы Auto ICMP monitoring");
            //рабочая папка на диске
            //проверка файла
            devices = new List<Device>();
            //путь текущей программы

            //проверка наличии папки
            if (!Directory.Exists(pathWorkDir))
            {
                Directory.CreateDirectory(pathWorkDir);
            }

            //сериализация дефолтного конфига
            string defJsonConfig = JsonConvert.SerializeObject(new Config());

            
            WriteLog(defJsonConfig);
            //загрузка конфига, если нет создаём дефолтный
            if (File.Exists("configMonitoring.json"))
            {
                string jsonFromFile = File.ReadAllText(basePathConfig);
                //дессериализация из файла
                Config config = JsonConvert.DeserializeObject<Config>(jsonFromFile);
                WriteLog(Config.tokenBot);
                DeviceMonitoring.TGSendMessage("⌛ Конфигурация " + jsonFromFile);
            }
            else
            {
                File.WriteAllText(basePathConfig, defJsonConfig);
                DeviceMonitoring.TGSendMessage("⌛ Конфигурация " + defJsonConfig);
            }
            //перебрать содержимое папки и найти таблицу xlsx
            string[] filesNames = Directory.GetFiles(pathWorkDir, "*.xlsx");

            switch (filesNames.Length)
            {
                case 0:
                    DeviceMonitoring.TGSendMessage("⛔ Auto ICMP monitoring Error. There is no xlsx table file in the program folder!");
                    WriteLog("Error. There is no xlsx table file in the program folder!");
                    break;
                case 1:
                    string[] arrPathPartsDevices = filesNames[0].Split('/');
                    string nameTestDevicesFile = arrPathPartsDevices[arrPathPartsDevices.Length - 1];
                    DeviceMonitoring.TGSendMessage("⌛ Auto ICMP monitoring Ok. Table " + nameTestDevicesFile + " initialization completed");
                    WriteLog("Ok. Table " + nameTestDevicesFile + " initialization completed");
                    using (FileStream fstream1 = new FileStream(filesNames[0], FileMode.Open))
                    {
                        Workbook divecesWorkbook = new Workbook(fstream1);
                        int indexLastRow = divecesWorkbook.Worksheets[0].Cells.MaxRow;
                        for (int i = 0; i < indexLastRow; i++)
                        {
                            if (IPAddress.TryParse(divecesWorkbook.Worksheets[0].Cells[i, 2].Value.ToString(), out IPAddress deviceAddress))
                            {
                                WriteLog("[" + DateTime.Now.ToString() + "] " + divecesWorkbook.Worksheets[0].Cells[i, 0].Value.ToString() + " " + divecesWorkbook.Worksheets[0].Cells[i, 1].Value.ToString() + " " + deviceAddress.ToString() + " " + Device.DeviceStatus.Unknown.ToString() + " " + Config.recheckCount.ToString());
                                devices.Add(new Device(divecesWorkbook.Worksheets[0].Cells[i, 0].Value.ToString() + " " + divecesWorkbook.Worksheets[0].Cells[i, 1].Value.ToString(), deviceAddress, Device.DeviceStatus.Unknown, Config.recheckCount));
                            }
                        }
                    }
                    if (devices.Count>0)
                    {
                        DeviceMonitoring.TGSendMessage("▶🔂 Auto ICMP monitoring start completed. Number of devices: " + devices.Count);
                        isServiceReady = true;

                        while (isServiceReady)
                        {
                            //WriteLog("[" + DateTime.Now.ToString() + "] интерация");
                            for (int i = 0; i < devices.Count; i++)
                            {

                                devices[i].Check();

                            }
                            
                            await Task.Delay(Config.tickTimeSec*1000);
                        }

                        //TimerCallback tm = new TimerCallback(TestTick);
                        //// создаем таймер
                        //Timer timer = new Timer(tm, true, 0, Config.tickTimeSec * 1000);
                    }

                    
                    break;
                default:
                    WriteLog("Error. More than 1 xlsx file is not allowed!");
                    DeviceMonitoring.TGSendMessage("⛔ Auto ICMP monitoring Error. More than 1 xlsx file is not allowed!");
                    break;
            }


        }

        //public static void TestTick(object obj)
        //{
            
        //}

        public static void WriteLog(string msg)
        {
            File.AppendAllText(pathLog, msg+"\r\n", Encoding.UTF8);
        }

        protected override void OnStop()
        {
            isServiceReady = false;
            DeviceMonitoring.TGSendMessage("⛔ Auto ICMP monitoring was stopped");
        }
    }
}

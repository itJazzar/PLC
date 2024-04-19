using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyModbus;


namespace PLC
{
    class PLC
    {
        //4. Реализация решения на ModbusTCP
        //5. Задание списка тэгов ModbusTag
        //7. Реализация Watchdog
        //8. Методы записи и чтения тэгов
        //9. Реализация проверки соединения
        //10. Возможность выбора типа соединения(постоянное соединение,
        //подключение по необходимости)
        //11. Методы для реализации Event-системы
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int StartAddress { get; set; }
        public int Quantity { get; set; }
        public int TimeoutConnection { get; set; }
        public PLCTypes PLCType { get; set; }
        private ModbusClient _modbusClient;
        private CancellationTokenSource _cancellationTokenSource;
        public enum PLCTypes
        {
            Shnider,
            Omron,
            Siemens,
            EKF,
            Delta
        }
        public PLC(string ipAddress = "12.12.0.10", int port = 102)
        {
            Port = port;
            IpAddress = ipAddress;
            _modbusClient = new ModbusClient();
            ModbusTag modbusTag = new ModbusTag();
        }
        private void Connect()
        {
            if (_modbusClient != null && !_modbusClient.Connected)
            {
                _modbusClient.Connect(IpAddress, Port);
                if (_modbusClient.Connected)
                {
                    Console.WriteLine("Connection established.\n");
                }
            }
        }
        private async Task CheckConnection()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                if (!(_modbusClient != null && _modbusClient.Connected))
                {
                    Connect();
                }
            }
        }
        //private async Task PingThread()
        //{
        //    while (!_modbusClient.Connected)
        //    {
        //        if (!PingDevice())
        //        {
        //            Console.WriteLine("Ping failed. Reconnecting...\n");
        //            Connect();
        //            Connect();
        //        }
        //        await Task.Delay(TimeSpan.FromSeconds(1));
        //    }
        //}
        //private bool PingDevice()
        //{
        //    using (Ping ping = new Ping())
        //    {
        //        try
        //        {
        //            PingReply reply = ping.Send(IpAddress);
        //            return reply.Status == IPStatus.Success;
        //        }
        //        catch (PingException)
        //        {
        //            return false;
        //        }
        //    }
        //}
        private void Disconnect()
        {
            _modbusClient?.Disconnect();
            if (!_modbusClient.Connected)
            {
                Console.WriteLine("Disconnestion was done successful.\n");
            }
        }
        private bool[] ReadData()
        {
            var result = _modbusClient.ReadCoils(StartAddress, Quantity); //error
            return result;
        }
        private void Write(int tag, bool value)
        {
            _modbusClient.WriteSingleCoil(tag, value);
        }

        static async Task Main(string[] args)
        {
            PLC plc = new PLC();
            plc.IpAddress = "12.12.0.50";
            plc.Port = 502; //102 порт симёна ??; 502
            plc.PLCType = PLCTypes.Siemens;
            plc.TimeoutConnection = 1000;
            plc.StartAddress = 0;
            plc.Quantity = 7;

            plc.Connect();

            //await plc.CheckConnection();
            //Task pingTask = plc.PingThread();
            //await pingTask;

            //plc.Write(0, true);
            //plc.Write(1, true);
            //plc.Write(2, true);
            //plc.Write(3, false);
            //plc.Write(4, true);
            //plc.Write(5, true);
            //plc.Write(6, true);

            var data = plc.ReadData();

            for (int i = 0; i < data.Length; i++)
            {
                Console.WriteLine($"Value of Coil {i} : {data[i]}");
            }

            plc.Disconnect();




            Console.ReadKey();
        }
    }
    class ModbusTag
    {
        //1. Принадлежность к ПЛК
        //2. Адрес тэга
        //3. тип данных
        //4. Последнее полученное или заданное значение
        //5. Автоматический опрос
        //6. Эвент изменения значения
        //7. Методы записи и чтения по данному тэгу

        public event EventHandler TagChanged;




        public enum ModbusDataType
        {
            Bool,         // Bit
            Byte,         // 8-bit целое число
            SInt,         // 8-bit знаковое целое число
            USInt,        // 8-bit беззнаковое целое число
            Word,         // 16-bit целое число
            Int,          // 16-bit знаковое целое число
            UInt,         // 16-bit беззнаковое целое число
            DWord,        // 32-bit целое число
            DInt,         // 32-bit знаковое целое число
            Real,         // 32-bit число с плавающей точкой
            UDInt,        // 32-bit беззнаковое целое число
            Char,         // символ (8 бит)
            Date,         // дата (16 бит)
            Time,         // время (32 бита)
            Time_Of_Day,  // время дня (32 бита)
            Array,        // массив
            Struct        // структура
        }

        public ModbusTag()
        {

        }
    }
}

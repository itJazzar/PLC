using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public enum PLCTypes
        {
            Shnider,
            Omron,
            Siemens,
            EKF,
            Delta
        }
        public PLC()
        {
            _modbusClient = new ModbusClient(IpAddress, Port);
        }
        private void Connect()
        {
            _modbusClient.Connect(IpAddress, Port);
            if (_modbusClient.Connected)
            {
                Console.WriteLine("Connection established.");
            }
        }
        private void Disconnect()
        {
            _modbusClient?.Disconnect();
            if (!_modbusClient.Connected)
            {
                Console.WriteLine("Disconnestion was done successful.");
            }
        }
        private bool[] ReadData()
        {
            var result = _modbusClient.ReadDiscreteInputs(StartAddress, Quantity); //error
            return result;
        }
        private void SetCoils(bool value)
        {
            _modbusClient.WriteSingleCoil(StartAddress, value);
        }





        static void Main(string[] args)
        {
            PLC plc = new PLC();
            plc.PLCType = PLCTypes.Siemens;
            plc.IpAddress = "12.12.0.11";
            plc.Port = 102; //102 порт симёна; 502
            plc.TimeoutConnection = 1000;
            plc.StartAddress = 0;
            plc.Quantity = 1;

            plc.Connect();


            //plc.SetCoils(true);

            var data = plc.ReadData();

            for (int i = 0; i < data.Length; i++)
            {
                Console.WriteLine($"Value of Discrete Input {i} : {data[i].ToString()}");
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

        public ModbusTag()
        {

        }
    }
}

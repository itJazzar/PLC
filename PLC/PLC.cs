using static System.Console;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using EasyModbus;
using System.Net.Sockets;


namespace PLC
{
    class PLC
    {
        //4. Реализация решения на ModbusTCP +
        //5. Задание списка тэгов ModbusTag
        //7. Реализация Watchdog
        //8. Методы записи и чтения тэгов +
        //9. Реализация проверки соединения +
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
        public ModbusTag Tag { get; set; }
        public List<ModbusTag> ModbusTags = new List<ModbusTag>();

        public enum PLCTypes
        {
            Shnider,
            Omron,
            Siemens,
            EKF,
            Delta
        }
        public PLC(string ipAddress = "12.12.0.10", int port = 102, string name = "", int address = 0, ModbusTag.ModbusDataType dataType = ModbusTag.ModbusDataType.Bool)
        {
            Port = port;
            IpAddress = ipAddress;
            _modbusClient = new ModbusClient();

            Tag = new ModbusTag(name, address, dataType);
            Tag.Address = address;
            Tag.DataType = dataType;
            Tag.Name = name;

            ModbusTags.Add(Tag);

            //_modbusClient.ReceiveDataChanged += new ModbusClient.ReceiveDataChangedHandler(UpdateReceiveData);
            //_modbusClient.SendDataChanged += new ModbusClient.SendDataChangedHandler(UpdateSendData);
            //_modbusClient.ConnectedChanged += new ModbusClient.ConnectedChangedHandler(UpdateConnectedChanged);
        }
        private void Connect()
        {
            if (_modbusClient != null && !_modbusClient.Connected)
            {
                _modbusClient.ConnectionTimeout = TimeoutConnection;
                _modbusClient.Connect(IpAddress, Port);
                if (_modbusClient.Connected)
                {
                    WriteLine("Connection established.\n");
                }
            }
        }
        public void PingPLC(int pingTimes, int timeout, int delayBetweenPings)
        {
            //SetCursorPosition(0, 15);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                //string to hold our return message
                string returnMessage = string.Empty;

                //set the ping options, TTL 128
                PingOptions pingOptions = new PingOptions(128, true);

                //create a new ping instance
                Ping ping = new Ping();

                //32 byte buffer (create empty)
                byte[] buffer = new byte[32];

                //first make sure we actually have an internet connection
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    //here we will ping the host pingTimes (standard)
                    for (int i = 0; i < pingTimes; i++)
                    {
                        try
                        {
                            //send the ping pingTimes to the host and record the returned data.
                            //The Send() method expects 4 items:
                            //1) The IPAddress we are pinging
                            //2) The timeout value
                            //3) A buffer (our byte array)
                            //4) PingOptions
                            PingReply pingReply = ping.Send(IpAddress, timeout, buffer, pingOptions);

                            //make sure we dont have a null reply
                            if (!(pingReply == null))
                            {
                                switch (pingReply.Status)
                                {
                                    case IPStatus.Success:
                                        returnMessage = string.Format("Reply from {0}: bytes={1} time={2}ms TTL={3}", pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options.Ttl);
                                        //WriteLine(returnMessage);
                                        break;
                                    case IPStatus.TimedOut:
                                        returnMessage = "Connection has timed out...";
                                        WriteLine(returnMessage);
                                        break;
                                    default:
                                        returnMessage = string.Format("Ping failed: {0}", pingReply.Status.ToString());
                                        WriteLine(returnMessage);
                                        break;
                                }
                            }
                            else
                            {
                                returnMessage = "Connection failed for an unknown reason...";
                                WriteLine(returnMessage);
                            }
                        }
                        catch (PingException ex)
                        {
                            returnMessage = string.Format("Connection Error: {0}", ex.Message);
                            WriteLine(returnMessage);
                        }
                        catch (SocketException ex)
                        {
                            returnMessage = string.Format("Connection Error: {0}", ex.Message);
                            WriteLine(returnMessage);
                        }

                        // Delay between pings
                        Thread.Sleep(delayBetweenPings);
                    }
                    WriteLine("Ping completed\n");
                }
                else
                {
                    returnMessage = "No Internet connection found...";
                    WriteLine(returnMessage);
                }
            });
            //SetCursorPosition(0, 5);
        }
        private void Disconnect()
        {
            _modbusClient?.Disconnect();
            if (!_modbusClient.Connected)
            {
                WriteLine("\nDisconnestion was done successful.\n");
            }
        }
        private bool[] ReadCoils()
        {
            var result = _modbusClient.ReadCoils(StartAddress, Quantity);
            return result;
        }
        private bool[] ReadDiscreteInputs()
        {
            var result = _modbusClient.ReadDiscreteInputs(StartAddress, Quantity);
            return result;
        }
        private int[] ReadHoldingRegisters(int startingAddress, int quantity)
        {
            var result = _modbusClient.ReadHoldingRegisters(startingAddress, quantity);
            return result;
        }
        private int[] ReadInputRegisters()
        {
            var result = _modbusClient.ReadInputRegisters(StartAddress, Quantity);
            return result;
        }
        private int[] ReadWriteMultipleRegisters(int startingAddressRead, int quantity, int startingAddressWrite, int[] values)
        {
            var result = _modbusClient.ReadWriteMultipleRegisters(startingAddressRead, quantity, startingAddressWrite, values);
            return result;
        }
        private void WriteSingleCoil(int startingAddress, bool value)
        {
            _modbusClient.WriteSingleCoil(startingAddress, value);
        }
        private void WriteMultipleCoils(int startAddress, bool[] values)
        {
            _modbusClient.WriteMultipleCoils(startAddress, values);
        }
        private void WriteSingleRegister(int register, int value)
        {
            _modbusClient.WriteSingleRegister(register, value);
        }
        public override string ToString()
        {
            if (Tag != null)
            {
                return $"PLC Info:\nIP Address: {IpAddress}\nPort: {Port}\nTag Info:\nName: {Tag.Name}\nAddress: {Tag.Address}\nData Type: {Tag.DataType}\nValue: {Tag.Value}\n";
            }
            else
            {
                return $"PLC Info:\nIP Address: {IpAddress}\nPort: {Port}\nNo Modbus Tag defined\n";
            }
        }
        static async Task Main(string[] args)
        {
            PLC plc = new PLC();
            plc.Tag.Name = "Start";
            //plc.Tag.Address = 107;
            plc.Tag.DataType = ModbusTag.ModbusDataType.Bool;
            plc.IpAddress = "12.12.0.50";
            plc.Port = 502;
            plc.PLCType = PLCTypes.Siemens;
            plc.TimeoutConnection = 1000;
            plc.StartAddress = 0;
            
            plc.Quantity = 6;
            
            WriteLine(plc.ToString());

            plc.Connect();

            Task.Run(() =>
            {
                plc.PingPLC(100, 3000, 3000);
            });
            


            plc.WriteSingleRegister(0, 555);

            var holdRegs = plc.ReadHoldingRegisters(0, 1);

            foreach (var holdReg in holdRegs)
            {
                WriteLine($"Value of holding register is {holdReg}\n");
            }

            for (int i = 0; i < 6; i++)
            {
                plc.WriteSingleCoil(i, false);
            }
            bool[] coils = plc.ReadCoils();

            for (int i = 0; i < coils.Length; i++)
            {
                WriteLine($"Value of Coil {i} : {coils[i]}");
            }

            plc.Disconnect();

            ReadKey();
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
        //7. Методы записи и чтения по данsному тэгу
        public string Name { get; set; }
        public int Address { get; set; }
        public ModbusDataType DataType { get; set; }
        public bool Value;
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

        public ModbusTag(string name, int address, ModbusDataType dataType)
        {
            Name = name;
            Address = address;
            DataType = dataType;
            Value = false;
        }
    }
}

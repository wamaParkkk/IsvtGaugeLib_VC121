using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;

namespace IsvtGaugeLib_VC121
{
    public class VC121Class
    {
        private SerialPort _serialPort;

        /// <summary>
        /// VC121 클래스 생성자
        /// </summary>
        /// <param name="portName">COM 포트</param>
        /// <param name="baudRate">통신 속도</param>
        /// <param name="Parity">Parity</param>
        /// <param name="DataBits">DataBits</param>
        /// <param name="StopBits">StopBits</param>
        public VC121Class(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.NewLine = "\r";
        }

        /// <summary>
        /// 시리얼 포트 열기
        /// </summary>
        public bool Open()
        {
            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();

                Global.EventLog("Connected to VC121 Controller");
                return true;
            }
            catch (Exception ex)
            {
                Global.EventLog($"[ERROR] opening serial port: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 시리얼 포트 닫기
        /// </summary>
        public void Close()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        private readonly object _serialLock = new object();

        /// <summary>
        /// VC121에 명령을 전송하고 응답을 반환
        /// </summary>
        /// <param name="command">전송할 명령 (예: "R", "A", "B", "U", "S")</param>
        /// <returns>장치에서 받은 응답</returns>
        public string SendCommand(string command)
        {
            lock (_serialLock)  //동기화 처리
            {
                if (!_serialPort.IsOpen)
                {
                    throw new InvalidOperationException("[ERROR] Serial port is closed");
                }

                _serialPort.WriteLine(command);
                return _serialPort.ReadLine().Trim();
            }                
        }        
    }


    public class VC121Controller
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private VC121Class _vc121class;

        public VC121Controller()
        {
            // Ini file read
            StringBuilder sbVal = new StringBuilder();

            GetPrivateProfileString("PortName", "Port", "", sbVal, sbVal.Capacity, string.Format("{0}{1}", Global.serialPortInfoPath, "IsvtVC121PortInfo.ini"));
            string strPortName = sbVal.ToString();

            GetPrivateProfileString("BaudRate", "BaudRate", "", sbVal, sbVal.Capacity, string.Format("{0}{1}", Global.serialPortInfoPath, "IsvtVC121PortInfo.ini"));
            int iBaudRate = Convert.ToInt32(sbVal.ToString());

            GetPrivateProfileString("Parity", "Parity", "", sbVal, sbVal.Capacity, string.Format("{0}{1}", Global.serialPortInfoPath, "IsvtVC121PortInfo.ini"));
            Parity parity = (Parity)Convert.ToInt32(sbVal.ToString());

            GetPrivateProfileString("DataBits", "DataBits", "", sbVal, sbVal.Capacity, string.Format("{0}{1}", Global.serialPortInfoPath, "IsvtVC121PortInfo.ini"));
            int iDataBits = Convert.ToInt32(sbVal.ToString());

            GetPrivateProfileString("StopBits", "StopBits", "", sbVal, sbVal.Capacity, string.Format("{0}{1}", Global.serialPortInfoPath, "IsvtVC121PortInfo.ini"));
            StopBits stopBits = (StopBits)Convert.ToInt32(sbVal.ToString());

            _vc121class = new VC121Class(strPortName, iBaudRate, parity, iDataBits, stopBits);
        }

        public bool Connect() => _vc121class.Open();

        public void Disconnect() => _vc121class.Close();

        /// <summary>
        /// 진공 압력 데이터 읽기
        /// </summary>
        public string ReadPressure()
        {
            return _vc121class.SendCommand("R");
        }

        /// <summary>
        /// SetPoint#1 값 읽기
        /// </summary>
        public string ReadSetPoint1()
        {
            return _vc121class.SendCommand("A");
        }

        /// <summary>
        /// SetPoint#2 값 읽기
        /// </summary>
        public string ReadSetPoint2()
        {
            return _vc121class.SendCommand("B");
        }

        /// <summary>
        /// 단위 읽기 (Torr, mBar, Pascal)
        /// </summary>
        public string ReadUnit()
        {
            return _vc121class.SendCommand("U");
        }

        /// <summary>
        /// 상태 정보 읽기 (센서 및 SP1, SP2 상태)
        /// </summary>
        public string ReadStatus()
        {
            return _vc121class.SendCommand("S");
        }
    }
}

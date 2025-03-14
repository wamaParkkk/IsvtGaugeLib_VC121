using System;
using System.IO.Ports;

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

        /// <summary>
        /// VC121에 명령을 전송하고 응답을 반환
        /// </summary>
        /// <param name="command">전송할 명령 (예: "R", "A", "B", "U", "S")</param>
        /// <returns>장치에서 받은 응답</returns>
        public string SendCommand(string command)
        {
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException("[ERROR] Serial port is closed");
            }

            _serialPort.WriteLine(command);
            return _serialPort.ReadLine().Trim();
        }

        /// <summary>
        /// 진공 압력 데이터 읽기
        /// </summary>
        public string ReadPressure()
        {
            return SendCommand("R");
        }

        /// <summary>
        /// SetPoint#1 값 읽기
        /// </summary>
        public string ReadSetPoint1()
        {
            return SendCommand("A");
        }

        /// <summary>
        /// SetPoint#2 값 읽기
        /// </summary>
        public string ReadSetPoint2()
        {
            return SendCommand("B");
        }

        /// <summary>
        /// 단위 읽기 (Torr, mBar, Pascal)
        /// </summary>
        public string ReadUnit()
        {
            return SendCommand("U");
        }

        /// <summary>
        /// 상태 정보 읽기 (센서 및 SP1, SP2 상태)
        /// </summary>
        public string ReadStatus()
        {
            return SendCommand("S");
        }
    }
}

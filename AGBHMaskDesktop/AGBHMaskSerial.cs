using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace AGBHMask.AGSerialPort
{

    class AGBHMaskSerial
    {

        private SerialPort _serialPort;

        private const char _szTerminator = '#';

        public enum State
        {
            DISCONNECTED,
            CONNECTED
        }

        public State connectedState = State.DISCONNECTED;

        public String serialPort = "";

        public bool Connect()
        {

            if (connectedState == State.CONNECTED)
            {
                throw new Exception("Already Connected");
            }

            _serialPort = new SerialPort();
            _serialPort.PortName = serialPort;
            _serialPort.BaudRate = 9600;

            _serialPort.ReadTimeout = 3000;
            _serialPort.WriteTimeout = 3000;

            try
            {
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                return false;
            }

            connectedState = State.CONNECTED;

            return true;
        }

        public bool DisConnect()
        {
            _serialPort.Close();
            connectedState = State.DISCONNECTED;

            return true;
        }

        public String ReadUntil(String strCommand)
        {
            bool terminated = false;
            SendCommand(strCommand);

            String received = "";

            try
            {
                do
                {
                    received += Convert.ToChar(_serialPort.ReadByte()); ;
                    if (received.Last() == _szTerminator)
                    {
                        terminated = true;
                    }

                } while (!terminated);

            }
            catch (Exception ex)
            {
                return null;
            }
            received = received.Replace("#", "");
            return received;
        }

        public bool SendCommand(String strCommand)
        {
            try
            {
                strCommand += _szTerminator;
                _serialPort.Write(strCommand);
                return true;
            }
            catch (System.InvalidOperationException ex)
            {
                return false;
            }
        }
    }
}

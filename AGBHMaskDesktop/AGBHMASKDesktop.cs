using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AGBHMask.AGSerialPort;

namespace AGBHMaskDesktop
{
    public partial class AGBHMASKDesktop : Form
    {
        private AGBHMaskSerial _serialPort = new AGBHMaskSerial();
        private String _selectedPort = "";
        private String _maskVersion = "0.0.0";

        private enum MaskStates
        {
            CLOSED,
            OPEN
        }
        public AGBHMASKDesktop()
        {
            InitializeComponent();

            InitUI();

            btnRefresh.Font = new Font("Wingdings 3", 8, FontStyle.Bold);
            btnRefresh.Text = Char.ConvertFromUtf32(81); // or 80
        }

        private MaskStates _maskState = MaskStates.CLOSED;

        private void InitUI()
        {
            comboBoxComPort.Items.Clear();
            comboBoxComPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            if (comboBoxComPort.Items.Count == 1)
            {
                comboBoxComPort.SelectedIndex = 0;
            }

            if (comboBoxComPort.Items.Contains(_selectedPort))
            {
                comboBoxComPort.SelectedItem = _selectedPort;
            }

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_serialPort.connectedState == AGBHMaskSerial.State.CONNECTED)
            {
                _serialPort.DisConnect();
                tmrPoll.Stop();
                btnConnect.Text = "Connect";
                lblState.Text = "Unknown";
                btnState.Text = "";
                lblHardwareVersion.Text = "";
            }
            else
            {
                _selectedPort = (string)comboBoxComPort.SelectedItem;
                if (_selectedPort != null)
                {
                    btnConnect.Enabled = false;
                    _serialPort.serialPort = _selectedPort;
                    if (_serialPort.Connect())
                    {
                        btnConnect.Text = "DisConnect";
                        if (GetVersion())
                        {
                            if (GetState())
                            {
                                lblHardwareVersion.Text = "Version: " + _maskVersion;
                                tmrPoll.Start();
                            }
                        }
                    }
                    else
                    {
                        string message = "Unable to open the serial port - " + _selectedPort;
                        string title = "Connection Error";
                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                        DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
                    }
                    btnConnect.Enabled = true;
                }
            }
        }

        private void ClosePort()
        {
            tmrPoll.Stop();
            if (_serialPort.connectedState == AGBHMaskSerial.State.CONNECTED)
            {
                _serialPort.DisConnect();
            }
            btnConnect.Text = "Connect";
            lblState.Text = "Unknown";
            btnState.Text = "";
            InitUI();
            string message = "No Mask Hardware found on serial port";
            string title = "Connection Error";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Error);
        }


        private void tmrPoll_Tick(object sender, EventArgs e)
        {
            GetState();
        }

        private bool GetState()
        {
            if (_serialPort.connectedState == AGBHMaskSerial.State.CONNECTED)
            {
                String maskState = _serialPort.ReadUntil("STATE");

                if (maskState != null)
                {
                    if (maskState == "0")
                    {
                        _maskState = MaskStates.OPEN;
                        lblState.Text = "OPEN";
                        btnState.Text = "Close";
                    }
                    else
                    {
                        _maskState = MaskStates.CLOSED;
                        lblState.Text = "CLOSED";
                        btnState.Text = "Open";
                    }

                    return true;
                }
                ClosePort();
                return false;
            }
            ClosePort();
            return false;
        }

        private void btnState_Click(object sender, EventArgs e)
        {
            if (_serialPort.connectedState == AGBHMaskSerial.State.CONNECTED)
            {
                if (_maskState == MaskStates.OPEN)
                {
                    SendCommand("CLOSE");
                }
                else
                {
                    SendCommand("OPEN");
                }
            }
        }

        private void AGBHMask_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serialPort.connectedState == AGBHMaskSerial.State.CONNECTED)
            {
                _serialPort.DisConnect();
            }
        }

        private bool GetVersion()
        {
            if (_serialPort.connectedState == AGBHMaskSerial.State.CONNECTED)
            {
                String strResult = _serialPort.ReadUntil("AGMASKVER");
                if (strResult != null)
                {
                    _maskVersion = strResult;
                    return true;
                }
                else
                {
                    ClosePort();
                }
            }


            return false;
        }

        private void SendCommand(String strCommand)
        {
            if (!_serialPort.SendCommand(strCommand))
            {
                ClosePort();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            InitUI();
        }

        private void comboBoxComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedPort = (string)comboBoxComPort.SelectedItem;
        }

    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidLibrary;

namespace QuickTestExample
{
    public partial class Form1 : Form
    {
        HidDevices HidDeviceScan;
        HidDevice CurrentHIDDevice;
        IEnumerable<HidDevice> USBDevices = null;


        public Form1()
        {
            InitializeComponent();
            HidDeviceScan = new HidDevices();
            HidDeviceScan.HidNewDeviceArrived += ScanForNewHidDevices;
            HidDeviceScan.StartScanning();
        }

        private void OnReport(HidReport report)
        {
            if (CurrentHIDDevice.IsConnected == false) { return; }

            // process your data here
            if (report.Data[0] != 0)
            {
            this.InvokeEx(x => x.textBox1.AppendText("Report ID: " + report.ReportId.ToString()));
            this.InvokeEx(x => x.textBox1.AppendText(Environment.NewLine));
            this.InvokeEx(x => x.textBox1.AppendText(BitConverter.ToString(report.Data).Replace('-', ' ')));
            this.InvokeEx(x => x.textBox1.AppendText(Environment.NewLine));
            this.InvokeEx(x => x.textBox1.AppendText(Environment.NewLine));
            this.InvokeEx(x => x.CurrentHIDDevice.IsOpen.ToString());

            }
            // we need to start listening again for more data
            this.InvokeEx(x => x.CurrentHIDDevice.ReadReport(OnReport));
            //CurrentHIDDevice.ReadReport(OnReport);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte command = byte.Parse(comboBox1.Text);
            
            if (CurrentHIDDevice != null)
            {
                if (CurrentHIDDevice.IsConnected)
                {
                    if (!CurrentHIDDevice.IsOpen)
                    {
                        this.InvokeEx(x => x.CurrentHIDDevice.ReadReport(OnReport));
                        CurrentHIDDevice.OpenDevice();
                    }
                    try 
                    { 
                        //CurrentHIDDevice.Write(new byte[] { 0, 2, command });
                        //byte[] data = new byte[3];
                        //data[0] = 0;
                        //data[1] = 2;
                        //data[2] = byte.Parse(comboBox1.Text);
                        //HidReport report = new HidReport(3, new HidDeviceData(data,
                        //    HidDeviceData.ReadStatus.Success));
                        if (checkBox1.Checked)
                        {
                            byte[] data = new byte[3];
                            data[0] = 0;
                            data[1] = 2;
                            data[2] = byte.Parse(comboBox1.Text);
                            HidReport report = new HidReport(data.Length, new HidDeviceData(data,
                                HidDeviceData.ReadStatus.Success));
                            CurrentHIDDevice.WriteReportAsync(report);
                        }
                        else
                        {
                            byte[] data = new byte[2];
                            data[0] = 0;
                            data[1] = byte.Parse(comboBox1.Text);
                            HidReport report = new HidReport(data.Length, new HidDeviceData(data,
                                HidDeviceData.ReadStatus.Success));
                            CurrentHIDDevice.WriteReportAsync(report);
                        }

                        if (command == 5)
                        {
                            //var result = CurrentHIDDevice.Read().Data;
                            //textBox1.AppendText(BitConverter.ToString(result).Replace("-", " "));
                            //textBox1.AppendText(Environment.NewLine);
                            //textBox1.AppendText(Environment.NewLine);
                        }
                        
                    }
                    catch { MessageBox.Show("Invalid Command", "", MessageBoxButtons.OK, MessageBoxIcon.Error);  }
                    finally
                    {
                        //if (command != 5)
                            CurrentHIDDevice.CloseDevice();
                        //else
                            //this.InvokeEx(x => x.CurrentHIDDevice.ReadReport(OnReport));
                    }
                }
            }
        }

        private void ScanForNewHidDevices()
        {
            USBDevices = HidDevices.Enumerate(0x04D8);
            //HidDeviceScan.StopScanning();
            if (listBox1.Items.Count == USBDevices.Count())
                return;

            foreach (HidDevice validHIDDevice in listBox1.Items)
            {
                if (validHIDDevice.IsOpen)
                    validHIDDevice.CloseDevice();
                validHIDDevice.Dispose();
            }
            this.InvokeEx(x => x.listBox1.Items.Clear());

            foreach (HidDevice validHIDDevice in USBDevices)
            {
                if (validHIDDevice.IsConnected)
                {
                    this.InvokeEx(x => x.button1.Enabled = true);
                    this.InvokeEx(x => x.listBox1.Items.Add(validHIDDevice));
                }
            }

            foreach (HidDevice validHIDDevice in USBDevices)
            {
                CurrentHIDDevice = validHIDDevice;
                //validHIDDevice.OpenDevice();
                validHIDDevice.Inserted += deviceInserted;
                validHIDDevice.Removed += deviceRemoved;
                validHIDDevice.MonitorDeviceEvents = true;
                //CurrentHIDDevice.ReadReport(OnReport);
                //validHIDDevice.CloseDevice();
            }

            if (listBox1.Items.Count > 0)
            {
                this.InvokeEx(x => x.listBox1.SelectedIndex = 0);
                this.InvokeEx(x => CurrentHIDDevice = x.listBox1.SelectedItem as HidDevice);
            }
        }

        private void deviceInserted()
        {
            this.InvokeEx(x => x.toolStripStatusLabel1.Text = listBox1.Items.Count.ToString() + " device(s) attached");
        }

        private void deviceRemoved()
        {
            this.InvokeEx(x => x.toolStripStatusLabel1.Text = listBox1.Items.Count.ToString() + " device(s) attached");
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox1.Items.Count > 0 && listBox1.SelectedIndex >= 0)
                    CurrentHIDDevice = listBox1.SelectedItem as HidDevice;  
            }
            catch
            {
                MessageBox.Show("Selected item is not a USB device!");
            }
        }

        private void comboBox1_MouseEnter(object sender, EventArgs e)
        {
            comboBox1.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            HidDeviceScan.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
    }
}

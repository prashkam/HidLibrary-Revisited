using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidLibrary;

namespace QuickTestExample
{
    public partial class Form1 : Form
    {
        static HidDevices HidDeviceScan;
        HidDevice CurrentHIDDevice;
        static IEnumerable<HidDevice> USBDevices = null;


        public Form1()
        {
            InitializeComponent();
            HidDeviceScan = new HidDevices();
            HidDeviceScan.HidNewDeviceArrived += ScanForNewHidDevices;
            HidDeviceScan.StartScanning();
        }

        private void ScanForNewHidDevices()
        {
            USBDevices = HidDevices.Enumerate(0x04D8);

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
                validHIDDevice.Inserted += deviceInserted;
                validHIDDevice.Removed += deviceRemoved;
                validHIDDevice.MonitorDeviceEvents = true;
            }

            if (listBox1.Items.Count > 0)
            {
                this.InvokeEx(x => x.listBox1.SelectedIndex = 0);
                this.InvokeEx(x => CurrentHIDDevice = x.listBox1.SelectedItem as HidDevice);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CurrentHIDDevice != null)
            {
                if (CurrentHIDDevice.IsConnected)
                {
                    if (!CurrentHIDDevice.IsOpen) 
                        CurrentHIDDevice.OpenDevice();
                    try 
                    { 
                        byte command = byte.Parse(comboBox1.Text);
                        CurrentHIDDevice.Write(new byte[] { 0, 2, command });
                        if (command == 5)
                        {
                            var result = CurrentHIDDevice.Read();
                            textBox1.AppendText(BitConverter.ToString(result.Data).Replace("-", " "));
                            textBox1.AppendText(Environment.NewLine);
                            textBox1.AppendText(Environment.NewLine);
                        }
                    }
                    catch { MessageBox.Show("Invalid Command", "", MessageBoxButtons.OK, MessageBoxIcon.Error);  }
                    finally
                    {
                        CurrentHIDDevice.CloseDevice();
                    }
                }
            }
        }

        private void deviceInserted()
        {
            this.InvokeEx(x => x.toolStripStatusLabel1.Text = listBox1.Items.Count.ToString() + " device(s) attached");
            //this.InvokeEx(x => x.timer1.Start());
            
        }

        private void deviceRemoved()
        {
            this.InvokeEx(x => x.toolStripStatusLabel1.Text = listBox1.Items.Count.ToString() + " device(s) attached");
            //this.InvokeEx(x => x.timer1.Start());
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
    }
}

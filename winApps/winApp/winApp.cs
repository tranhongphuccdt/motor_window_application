using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;


namespace main1
{
    public partial class winApp : Form
    {
        public winApp()
        {
            InitializeComponent();
        }

        byte[] bytesReceived;

        [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 1)]
        private struct MyStructType
        {
            [FieldOffset(0)]
            public UInt16 Type;
            [FieldOffset(2)]
            public Byte DeviceNumber;
            [FieldOffset(4)]
            public float TableVersion;
            [FieldOffset(8)]
            public float SerialNumber;
            [FieldOffset(12)]
            public float Pi;
            //[FieldOffset(16)]
           // public Byte EndPayload;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] port = SerialPort.GetPortNames();
            cbPortName.Items.AddRange(port);
        }
        
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cbPortName.Text;
                serialPort1.BaudRate = 115200;
                serialPort1.ReadBufferSize = 16;    //must be odd number
                serialPort1.ReadTimeout = 500;
                //serialPort1.ReadBufferSize = 16;
                serialPort1.Open();
            }
            catch (Exception erro)
            {
                MessageBox.Show(erro.ToString());
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //iBuff = serialPort1.ReadChar();
            bytesReceived = new byte[16];
            serialPort1.Read(bytesReceived, 0, 16);
            this.Invoke(new EventHandler(showData));
        }
        private void showData(object sender, EventArgs e)
        {
            MyStructType receivedBuffer = new MyStructType();
            receivedBuffer = StructTools.RawDeserialize<MyStructType>(bytesReceived, 0); // 0 is offset in byte[]
            txtBuff.Text =  receivedBuffer.Type.ToString() + " " +
                            receivedBuffer.DeviceNumber.ToString() + " " +
                            receivedBuffer.TableVersion.ToString() + " " +
                            receivedBuffer.SerialNumber.ToString() + " " +
                            receivedBuffer.Pi.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MyStructType sendBuffer = new MyStructType();
            sendBuffer.Type = 1234;
            sendBuffer.DeviceNumber = 56;
            sendBuffer.TableVersion = 78901234;
            sendBuffer.SerialNumber = 56789012;
            sendBuffer.Pi = 3.14159265358979F;
            byte[] bytes = StructTools.RawSerialize(sendBuffer);
            //string sendString = Encoding.UTF8.GetString(bytes);       //not good method
            //string sendStringAS = Encoding.ASCII.GetString(bytes);    //not good method
            //string sendStringAS2 = Encoding.Unicode.GetString(bytes); //not good method
            //serialPort1.Write(sendString);                            //not good method
            serialPort1.Write(bytes, 0, bytes.Length);                  //OK
        }
    }
}

public static class StructTools
{
    /// <summary>
    /// converts byte[] to struct
    /// </summary>
    public static T RawDeserialize<T>(byte[] rawData, int position)
    {
        int rawsize = Marshal.SizeOf(typeof(T));
        if (rawsize > rawData.Length - position)
            throw new ArgumentException("Not enough data to fill struct. Array length from position: " + (rawData.Length - position) + ", Struct length: " + rawsize);
        IntPtr buffer = Marshal.AllocHGlobal(rawsize);
        Marshal.Copy(rawData, position, buffer, rawsize);
        T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
        Marshal.FreeHGlobal(buffer);
        return retobj;
    }

    /// <summary>
    /// converts a struct to byte[]
    /// </summary>
    public static byte[] RawSerialize(object anything)
    {
        int rawSize = Marshal.SizeOf(anything);
        IntPtr buffer = Marshal.AllocHGlobal(rawSize);
        Marshal.StructureToPtr(anything, buffer, false);
        byte[] rawDatas = new byte[rawSize];
        Marshal.Copy(buffer, rawDatas, 0, rawSize);
        Marshal.FreeHGlobal(buffer);
        return rawDatas;
    }
}
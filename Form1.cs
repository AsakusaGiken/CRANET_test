using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CRANET_test
{
    public partial class Form1 : Form
    {
        //メンバ
        int MQ,MQ2;
        byte[] rxData = new byte[Constants.RXBUF_SIZE];  //受信リングバッファ
        byte[] rxData2 = new byte[Constants.RXBUF_SIZE];  //受信リングバッファ
        int myPos, nowPos,myPos2,nowPos2;
        byte[] myPacket = new byte[20];
        byte[] myPacket2 = new byte[20];

        byte mySum,packetPos,mySum2,packetPos2;
        MyRichTextBox feedbackTextBox = new MyRichTextBox();
        MyRichTextBox feedbackTextBox2 = new MyRichTextBox();

        int eCode;
        byte SEQ;

        //コンストラクタ
        public Form1()
        {
            InitializeComponent();
            serialPort1.BaudRate = 115200;
            serialPort2.BaudRate = 115200;

            MQ = 0;
            myPos = 0;
            nowPos = 0;

            mySum = 0;
            packetPos = 0;

            //生データ表示フィールド
            feedbackTextBox.Location = new Point(280, 25);
            feedbackTextBox.Size = new Size(120, 280);
            Controls.Add(feedbackTextBox);

            //生データ表示フィールド2
            feedbackTextBox2.Location = new Point(280, 330);
            feedbackTextBox2.Size = new Size(120, 100);
            Controls.Add(feedbackTextBox2);

            SEQ = 0;

            

        }

        //data set delegate
        delegate void TextSet(string text);

        //TXからのデータ表示
        private void dispData(byte[] d)
        {
            string s = "";
            s = "COM:" + d[0].ToString() + "\r\n";
            s += "SEQ:" + d[1].ToString() + "\r\n";
            s += "LV1:" + d[2].ToString() + "\r\n";
            s += "LV2:" + d[3].ToString() + "\r\n";
            s += "LV3:" + d[4].ToString() + "\r\n";
            s += "LV4:" + d[5].ToString() + "\r\n";
            s += "LV5:" + d[6].ToString() + "\r\n";
            s += "PD1:" + d[7].ToString() + "\r\n";
            s += "PD2:" + d[8].ToString() + "\r\n";
            s += "PD3:" + d[9].ToString() + "\r\n";
            //s += "LS_MSB:" + d[10].ToString() + "\r\n";
            s += "LS_MSB:" + Convert.ToString(d[10],2).PadLeft(8) + "\r\n";
            s += "LS_LSB:" + Convert.ToString(d[11], 2).PadLeft(8) + "\r\n";
            s += "AC:" + d[12].ToString() + "\r\n";
            s += "SW:" + Convert.ToString(d[13], 2).PadLeft(8) + "\r\n";
            s += "DE:" + d[14].ToString() + "\r\n";
            s += "SUM:" + d[15].ToString() + "\r\n";
            Invoke(new TextSet(feedbackTextBox.SetText), new object[] { s });
        }

        //RXからのデータ表示
        private void dispData2(byte[] d)
        {
            string s = "";
            s = "COM:" + d[0].ToString() + "\r\n";
            s += "SEL:" + d[1].ToString() + "\r\n";
            s += "EMG:" + d[2].ToString() + "\r\n";
            s += "SEQ:" + d[3].ToString() + "\r\n";
            s += "DE:" + d[4].ToString() + "\r\n";
            s += "SUM:" + d[5].ToString() + "\r\n";
            Invoke(new TextSet(feedbackTextBox2.SetText), new object[] { s });
        }

        //TXへの送信
        private void timer1_Tick(object sender, EventArgs e)
        {
            byte[] d = new byte[20];
            byte sum = 0;
            d[0] = 0xFF;
            d[1] = 5;
            d[2] = 0xAE;
            d[3] = SEQ;
            SEQ++;
            if (checkBox1.Checked)
            {
                d[4] = 0x01;
            }
            else
            {
                d[4] = 0x00;
            }
            d[5] = 0x26;
            for (int i = 0; i < 6; i++)
            {
                sum += d[i];
            }
            d[6] = sum;
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(d, 0, 7);
            }
        }

        //RXへ送信
        private void timer2_Tick(object sender, EventArgs e)
        {
            byte[] d = new byte[40];
            byte sum = 0;
            d[0] = 0xFF;  //SYNC
            d[1] = 29;  //LEN
            d[2] = 0xAF;  //COM
            d[3] = SEQ;  //SEQ
            SEQ++;
            d[4] = 4;  //CNT
            d[5] = Convert.ToByte(textBox_A11.Text);
            d[6] = Convert.ToByte(textBox_A12.Text);
            d[7] = Convert.ToByte(textBox_A13.Text);
            d[8] = convLS1();  //LS1
            d[9] = convD1();  //D1
            d[10] = Convert.ToByte(textBox_AC1.Text);

            d[11] = Convert.ToByte(textBox_A21.Text);
            d[12] = Convert.ToByte(textBox_A22.Text);
            d[13] = Convert.ToByte(textBox_A23.Text);
            d[14] = convLS2();  //LS2
            d[15] = convD2();  //D2
            d[16] = Convert.ToByte(textBox_AC2.Text);

            d[17] = Convert.ToByte(textBox_A31.Text);
            d[18] = Convert.ToByte(textBox_A32.Text);
            d[19] = Convert.ToByte(textBox_A33.Text);
            d[20] = convLS3();  //LS3
            d[21] = convD3();  //D3
            d[22] = Convert.ToByte(textBox_AC3.Text);

            d[23] = Convert.ToByte(textBox_A41.Text);
            d[24] = Convert.ToByte(textBox_A42.Text);
            d[25] = Convert.ToByte(textBox_A43.Text);
            d[26] = convLS4();  //LS3
            d[27] = convD4();  //D3
            d[28] = Convert.ToByte(textBox_AC4.Text);


            d[29] = 0x26;  //DE
            //SUM
            for (int i = 0; i < 30; i++)
            {
                sum += d[i];
            }
            d[30] = sum;
            if (serialPort2.IsOpen)
            {
                serialPort2.Write(d, 0, 31);
            }
        }

        //TXのオープン
        private void button_Open_Click(object sender, EventArgs e)
        {
            serialPort1.PortName = textBox1.Text;
            serialPort1.Open();
            serialPort1.DiscardInBuffer();
            timer1.Enabled = true;
        }

        //RXのオープン
        private void button_rxOpen_Click(object sender, EventArgs e)
        {
            serialPort2.PortName = textBox_txPort.Text;
            serialPort2.Open();
            serialPort2.DiscardInBuffer();
            timer2.Enabled = true;
        }

        //TXのクローズ
        private void button_close_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
        }

        //RXのクローズ
        private void button_rxClose_Click(object sender, EventArgs e)
        {
            serialPort2.Close();
        }

        private byte convLS1()
        {
            byte result = 0;
            if (checkBox_LS1_0.Checked) result += 1;
            if (checkBox_LS1_1.Checked) result += 2;
            if (checkBox_LS1_2.Checked) result += 4;
            if (checkBox_LS1_3.Checked) result += 8;
            if (checkBox_LS1_4.Checked) result += 16;
            if (checkBox_LS1_5.Checked) result += 32;
            if (checkBox_LS1_6.Checked) result += 64;
            if (checkBox_LS1_7.Checked) result += 128;
            return result;
        }

        private byte convLS2()
        {
            byte result = 0;
            if (checkBox_LS2_0.Checked) result += 1;
            if (checkBox_LS2_1.Checked) result += 2;
            if (checkBox_LS2_2.Checked) result += 4;
            if (checkBox_LS2_3.Checked) result += 8;
            if (checkBox_LS2_4.Checked) result += 16;
            if (checkBox_LS2_5.Checked) result += 32;
            if (checkBox_LS2_6.Checked) result += 64;
            if (checkBox_LS2_7.Checked) result += 128;
            return result;
        }

        private byte convLS3()
        {
            byte result = 0;
            if (checkBox_LS3_0.Checked) result += 1;
            if (checkBox_LS3_1.Checked) result += 2;
            if (checkBox_LS3_2.Checked) result += 4;
            if (checkBox_LS3_3.Checked) result += 8;
            if (checkBox_LS3_4.Checked) result += 16;
            if (checkBox_LS3_5.Checked) result += 32;
            if (checkBox_LS3_6.Checked) result += 64;
            if (checkBox_LS3_7.Checked) result += 128;
            return result;
        }

        private byte convLS4()
        {
            byte result = 0;
            if (checkBox_LS4_0.Checked) result += 1;
            if (checkBox_LS4_1.Checked) result += 2;
            if (checkBox_LS4_2.Checked) result += 4;
            if (checkBox_LS4_3.Checked) result += 8;
            if (checkBox_LS4_4.Checked) result += 16;
            if (checkBox_LS4_5.Checked) result += 32;
            if (checkBox_LS4_6.Checked) result += 64;
            if (checkBox_LS4_7.Checked) result += 128;
            return result;
        }

        private byte convD1()
        {
            byte result = 0;
            if (checkBox_D1_0.Checked) result += 1;
            if (checkBox_D1_1.Checked) result += 2;
            if (checkBox_D1_2.Checked) result += 4;
            if (checkBox_D1_3.Checked) result += 8;
            if (checkBox_D1_4.Checked) result += 16;
            if (checkBox_D1_5.Checked) result += 32;
            if (checkBox_D1_6.Checked) result += 64;
            if (checkBox_D1_7.Checked) result += 128;
            return result;
        }

        private byte convD2()
        {
            byte result = 0;
            if (checkBox_D2_0.Checked) result += 1;
            if (checkBox_D2_1.Checked) result += 2;
            if (checkBox_D2_2.Checked) result += 4;
            if (checkBox_D2_3.Checked) result += 8;
            if (checkBox_D2_4.Checked) result += 16;
            if (checkBox_D2_5.Checked) result += 32;
            if (checkBox_D2_6.Checked) result += 64;
            if (checkBox_D2_7.Checked) result += 128;
            return result;
        }

        private byte convD3()
        {
            byte result = 0;
            if (checkBox_D3_0.Checked) result += 1;
            if (checkBox_D3_1.Checked) result += 2;
            if (checkBox_D3_2.Checked) result += 4;
            if (checkBox_D3_3.Checked) result += 8;
            if (checkBox_D3_4.Checked) result += 16;
            if (checkBox_D3_5.Checked) result += 32;
            if (checkBox_D3_6.Checked) result += 64;
            if (checkBox_D3_7.Checked) result += 128;
            return result;
        }

        private byte convD4()
        {
            byte result = 0;
            if (checkBox_D4_0.Checked) result += 1;
            if (checkBox_D4_1.Checked) result += 2;
            if (checkBox_D4_2.Checked) result += 4;
            if (checkBox_D4_3.Checked) result += 8;
            if (checkBox_D4_4.Checked) result += 16;
            if (checkBox_D4_5.Checked) result += 32;
            if (checkBox_D4_6.Checked) result += 64;
            if (checkBox_D4_7.Checked) result += 128;
            return result;
        }


        //RX->子機への送信値変更
        private void button_change_Click(object sender, EventArgs e)
        {
            textBox_A11.Text = textBox_A11_in.Text;
            textBox_A12.Text = textBox_A12_in.Text;
            textBox_A13.Text = textBox_A13_in.Text;
            textBox_AC1.Text = textBox_AC1_in.Text;

            textBox_A21.Text = textBox_A21_in.Text;
            textBox_A22.Text = textBox_A22_in.Text;
            textBox_A23.Text = textBox_A23_in.Text;
            textBox_AC2.Text = textBox_AC2_in.Text;

            textBox_A31.Text = textBox_A31_in.Text;
            textBox_A32.Text = textBox_A32_in.Text;
            textBox_A33.Text = textBox_A33_in.Text;
            textBox_AC3.Text = textBox_AC3_in.Text;

            textBox_A41.Text = textBox_A41_in.Text;
            textBox_A42.Text = textBox_A42_in.Text;
            textBox_A43.Text = textBox_A43_in.Text;
            textBox_AC4.Text = textBox_AC4_in.Text;

        }

        //RX側受信
        private void serialPort2_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                //読み込み
                int len = serialPort2.BytesToRead;
                byte[] rsvData = new byte[len];
                byte packetLen=0;
                serialPort2.Read(rsvData, 0, len);
                //リングバッファへコピー
                for (int i = 0; i < len; i++)
                {
                    nowPos2++;
                    if (nowPos2 >= Constants.RXBUF_SIZE) nowPos2 = 0;
                    rxData2[nowPos2] = rsvData[i];
                }
                //パケット解析
                for (int i = 0; i < len; i++)
                {

                    switch (MQ2)
                    {
                        case 0:
                            if (rxData2[myPos2] == 0xFF)
                            {
                                MQ2 = 1;
                                mySum2 = 0xFF;
                            }
                            break;
                        case 1:
                            packetLen = rxData2[myPos2];
                            mySum2 += rxData2[myPos2];
                            packetPos2 = 0;
                            MQ2 = 2;
                            break;
                        case 2:
                            myPacket2[packetPos2] = rxData2[myPos2];
                            mySum2 += rxData2[myPos2];
                            packetPos2++;
                            if (packetPos2 >= packetLen)
                            {
                                dispData2(myPacket2);
                                MQ2 = 0;
                            }
                            break;
                    }
                    myPos2++;
                    if (myPos2 >= Constants.RXBUF_SIZE) myPos2 = 0;
                }

            }
            catch (System.InvalidOperationException e1)
            {
                eCode = 1;
            }
            catch (System.ArgumentException e2)
            {
                eCode = 2;
            }
        }

        //TX側受信
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                //読み込み
                int len = serialPort1.BytesToRead;
                byte[] rsvData = new byte[len];
                byte packetLen;
                serialPort1.Read(rsvData, 0, len);
                //リングバッファへコピー
                for(int i = 0; i < len; i++)
                {
                    nowPos++;
                    if(nowPos >= Constants.RXBUF_SIZE) nowPos = 0;
                    rxData[nowPos] = rsvData[i];  
                }
                //パケット解析
                for (int i = 0; i < len; i++)
                {
                    
                    switch (MQ)
                    {
                        case 0:
                            if(rxData[myPos] == 0xFF)
                            {
                                MQ = 1;
                                mySum = 0xFF;
                            }
                            break;
                        case 1:
                            packetLen = rxData[myPos];
                            mySum += rxData[myPos];
                            packetPos = 0;
                            MQ = 2;
                            break;
                        case 2:
                            myPacket[packetPos] = rxData[myPos];
                            mySum += rxData[myPos];
                            packetPos++;
                            if (packetPos >= 16)
                            {
                                dispData(myPacket);
                                MQ = 0;
                            }
                            break;
                    }
                    myPos++;
                    if(myPos >= Constants.RXBUF_SIZE) myPos = 0;
                }

                //checkDokaSensorData(
            }
            catch (System.InvalidOperationException e1)
            {
                eCode = 1;
            }
            catch (System.ArgumentException e2)
            {
                eCode = 2;
            }
        }


    }

    //定数クラス
    static class Constants
    {
        public const int RXBUF_SIZE = 1000;
    }

    public class MyRichTextBox : RichTextBox
    {
        public MyRichTextBox() { }

        public void SetText(string text)
        {
            this.Text = text;
        }

    }
}

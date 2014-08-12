using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iContrAll.SPIRadio
{
    public class RadioCommunication
    {
        public delegate void RadioMessageArrivedDelegate(RadioMessageEventArgs e);
        public event RadioMessageArrivedDelegate RadioMessageReveived;

        public bool InitRadio()
        {
            if (Init.WiringPiSetup() < 0)
            {
                Console.WriteLine("unable to set wiring pi\n");
                return false;
            }

            GPIO.pinMode(1, (int)GPIO.GPIOpinmode.Output);
            SPI.wiringPiSPISetup(0, 5000000);
            GPIO.pinMode(RadioConstants.PWDN, (int)GPIO.GPIOpinmode.Output);

            for (int i = 0; i < 4; i++)
            {
                GPIO.digitalWrite(RadioConstants.PWDN, 1);
                Thread.Sleep(RadioConstants.DD);
                GPIO.digitalWrite(RadioConstants.PWDN, 0);
                Thread.Sleep(RadioConstants.DD);
            }

            if (PiThreadInterrupts.wiringPiISR(RadioConstants.INT, (int)PiThreadInterrupts.InterruptLevels.INT_EDGE_FALLING, Interrupt0) < 0)
            {
                Console.WriteLine("unable to set interrupt pin\n");
                return false;
            }

            power_up();
            Console.WriteLine("power up");

            config_rf_chip(RadioConstants.P);
            Console.WriteLine("config rf chip");

            Clear_Int_Flags(RadioConstants.P);
            RX_Command(RadioConstants.P, ref state);

            return true;
        }
        
        byte[] data = new byte[RadioConstants.FIX_PACKET_LENGTH];

        public bool SendMessage(string senderId, string targetId, string hexa, 
                            byte[] channels, byte[] chVoltage, byte[]chDim)
        {
            data = new byte[RadioConstants.FIX_PACKET_LENGTH];

            byte[] sendMessageBytes = Encoding.UTF8.GetBytes(senderId+targetId+hexa+"x");

            Array.Copy(sendMessageBytes, 0, data, 0, sendMessageBytes.Length);
            

            // TODO: ez rendes karakteresen jöjjön
            Array.Copy(channels, 0, data, 11, 4);
            
            Array.Copy(chVoltage, 0, data, 15, 4);

            Array.Copy(chDim, 0, data, 19, 4);
            
            
            if (sendMessageBytes.Length > 62) return false;

            // maradék 0
            for (int i = data.Length; i < 62; i++)
            {
                data[i] = 0x08;
            }
            // kocsi vissza
            data[62] = 10;
            data[63] = 13;


            // írás
            Write_Tx_Fifo(RadioConstants.P, data);
            Clear_Int_Flags(RadioConstants.P);
            TX_Command(RadioConstants.P, ref state);

            Thread.Sleep(RadioConstants.DD);

            return true;
        }

        unsafe void CTS()
        {
            byte a = 0;
            while (a != 255)
            {
                byte[] x = new byte[] { RadioConstants.CMD_CTS_READ, 0 };
                fixed (byte* pX = x)
                {
                    SPI.wiringPiSPIDataRW(0, pX, x.Length);
                }
                a = x[1];
            }
        }

        char state = '0';

        

        unsafe void Interrupt0()
        {
            if (state == 'r')
            {
                Console.WriteLine("packet received\n");
                Read_Rx_Fifo(RadioConstants.P, data);
                Clear_Int_Flags(RadioConstants.P);
                RX_Command(RadioConstants.P, ref state);

                //string s = Encoding.UTF8.GetString(data);

                if (RadioMessageReveived!=null)
                {
                    RadioMessageReveived(new RadioMessageEventArgs(data));
                }

                //Console.WriteLine(s);
            }

            if (state == 't')
            {
                Console.WriteLine("packet sent\n");
                GPIO.digitalWrite(RadioConstants.TXRX, 0);
                Clear_Int_Flags(RadioConstants.P);
                RX_Command(RadioConstants.P, ref state);
            }
        }

        unsafe void Read_Rx_Fifo(int p, byte[] x)
        {
            byte[] data = new byte[RadioConstants.FIX_PACKET_LENGTH + 1];
            data[0] = RadioConstants.CMD_RX_FIFO_READ;

            for (int i = 0; i < RadioConstants.FIX_PACKET_LENGTH; i++)
            {
                data[i + 1] = 0;
            }


            fixed (byte* pData = data)
            {
                SPI.wiringPiSPIDataRW(p, pData, RadioConstants.FIX_PACKET_LENGTH + 1);
            }


            for (int i = 0; i < RadioConstants.FIX_PACKET_LENGTH; i++)
            {
                x[i] = data[i + 1];
            }

            CTS();
            byte[] t = new byte[] { 0x15, 0x03 };


            fixed (byte* pT = t)
            {
                SPI.wiringPiSPIDataRW(p, pT, 2);
            }


            CTS();
        }

        unsafe void Clear_Int_Flags(int p)
        {
            byte[] a = new byte[] { RadioConstants.CMD_GET_INT_STATUS, 0, 0, 0 };
            fixed (byte* pA = a)
            {
                SPI.wiringPiSPIDataRW(p, pA, a.Length);
            }
            CTS();
        }

        unsafe void RX_Command(int p, ref char x)
        {
            x = 'r';
            byte[] d = new byte[] { RadioConstants.CMD_START_RX, 0, 0, 0, RadioConstants.FIX_PACKET_LENGTH, 0, 0, 0 };
            fixed (byte* pD = d)
            {
                SPI.wiringPiSPIDataRW(p, pD, d.Length);
            }
            CTS();
        }

        unsafe void power_up()
        {
            GPIO.digitalWrite(RadioConstants.PWDN, 1);
            Thread.Sleep(RadioConstants.D);
            GPIO.digitalWrite(RadioConstants.PWDN, 0);
            Thread.Sleep(RadioConstants.D);
            byte[] pwr = new byte[3] { RadioConstants.CMD_POWER_UP, 1, 0 };
            fixed (byte* pPwr = pwr)
            {
                SPI.wiringPiSPIDataRW(RadioConstants.P, pPwr, 3);
            }
            CTS();
            pwr[0] = RadioConstants.CMD_GET_INT_STATUS;
            fixed (byte* pPwr = pwr)
            {
                SPI.wiringPiSPIDataRW(RadioConstants.P, pPwr, 3);
            }
            CTS();
        }

        unsafe void config_rf_chip(int p)
        {
            //	unsigned char config[30][17];
            int i, j, k;
            byte len;
            byte[] data = new byte[16];
            byte[] ARRAY = RadioConstants.RADIO_CONFIGURATION_DATA_ARRAY;
            k = 0;
            len = 1;
            for (i = 0; len > 0; i++)
            {
                len = ARRAY[i + k];
                // ????????????????????????????
                if (len > 0)
                    for (j = 1; j < (int)len + 1; j++)
                        data[j - 1] = ARRAY[i + j + k];
                if (len > 0)
                {
                    fixed (byte* pData = data)
                    {
                        SPI.wiringPiSPIDataRW(p, pData, len);
                    }
                }
                k += len;
            }
        }

        unsafe void Write_Tx_Fifo(int p, byte[] x)
        {
            byte[] data = new byte[RadioConstants.FIX_PACKET_LENGTH + 1];
            for (int i = 0; i < RadioConstants.FIX_PACKET_LENGTH; i++) data[i + 1] = x[i];
            data[0] = RadioConstants.CMD_TX_FIFO_WRITE;
            fixed (byte* pData = data)
            {
                SPI.wiringPiSPIDataRW(p, pData, RadioConstants.FIX_PACKET_LENGTH + 1);
            }
            CTS();
        }

        unsafe void TX_Command(int p, ref char x)
        {
            x = 't';
            GPIO.digitalWrite(RadioConstants.TXRX, 1);
            byte[] d = new byte[] { RadioConstants.CMD_START_TX, 0, 0, 0, RadioConstants.FIX_PACKET_LENGTH, 0 };
            fixed (byte* pD = d)
            {
                SPI.wiringPiSPIDataRW(p, pD, d.Length);
            }
            CTS();
        }
    }
}

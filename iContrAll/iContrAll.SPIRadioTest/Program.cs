using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.SerialPeripheralInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iContrAll.SPIRadioTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string send = "mentem szarni";
            byte[] sendPacket = Encoding.UTF8.GetBytes(send);

            const ConnectorPin clockPin = ConnectorPin.P1Pin23;
            const ConnectorPin selectSlavePin = ConnectorPin.P1Pin24;
            const ConnectorPin misoPin = ConnectorPin.P1Pin21;
            const ConnectorPin mosiPin = ConnectorPin.P1Pin19;

            
            var pins = new PinConfiguration[] { ConnectorPin.P1Pin22.Input() };

            var settings = new GpioConnectionSettings { Driver = GpioConnectionSettings.DefaultDriver };
            GpioConnection gpioConnection = new GpioConnection(settings, pins);

            gpioConnection.PinStatusChanged += gpioConnection_PinStatusChanged;

            var driver = GpioConnectionSettings.DefaultDriver;

            Console.WriteLine("Init elott");

            var clockBinaryPin = driver.Out(clockPin);
            var selectSlaveBinaryPin = driver.Out(selectSlavePin);
            var misoBinaryPin = driver.In(misoPin);
            var mosiBinaryPin = driver.Out(mosiPin);
            using (SpiConnection connection = new SpiConnection(clockBinaryPin, selectSlaveBinaryPin, misoBinaryPin, mosiBinaryPin, Endianness.LittleEndian))
            {
                bool sendBool = true;
                Console.WriteLine("send elott");
                while (!Console.KeyAvailable)
                {
                    connection.Write(sendBool);
                    sendBool = !sendBool;
                }
                Console.WriteLine("send utan");
            }
            Console.WriteLine("Connection bontva");
        }   

        static void gpioConnection_PinStatusChanged(object sender, PinStatusEventArgs e)
        {
            Console.WriteLine("asdasd:"+ e.Configuration.ToString());
        }
    }
}

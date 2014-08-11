namespace Test.Gpio.MCP3008
{
    internal static class Convert
    {
        #region Methods

        public static decimal ToLux(this decimal variableResistor)
        {
            // See http://learn.adafruit.com/photocells/using-a-photocell
            // and http://www.emant.com/316002.page

            const decimal luxRatio = 500000;
            return luxRatio / variableResistor;
        }

        #endregion
    }
}
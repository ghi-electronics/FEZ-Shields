﻿using GHIElectronics.TinyCLR.Devices.Adc;

namespace SeeedGroveStarterKit {
    public class RotaryAngleSensor {
        private AdcChannel Channel;
        public RotaryAngleSensor(int AdcPinNumber)  => this.Channel = AdcController.GetDefault().OpenChannel(AdcPinNumber);
        
        // between 0 and 100
        public double GetAngle() => this.Channel.ReadRatio() * 100;
    }
}

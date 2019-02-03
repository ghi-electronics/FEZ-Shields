﻿using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Pwm;

namespace SeeedGroveStarterKit {
    public class ServoMotor {
        private PwmChannel servo;
        public ServoMotor(string controller, int PwmPinNumber) {
            var PWM = PwmController.FromName(controller);

            this.servo = PWM.OpenChannel(PwmPinNumber);
            PWM.SetDesiredFrequency(1 / 0.020);
        }
        public double MinPulseCalibration {
            set {
                if (value > 1.5 || value < 0.1)
                    throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5ms");
                this._MinPulseCalibration = value;
            }
        }
        public double MaxPulseCalibration {
            set {
                if (value > 3 || value < 1.6)
                    throw new ArgumentOutOfRangeException("Must be between 1.6 and 3ms");
                this._MaxPulseCalibration = value;
            }
        }
        // min and max pulse width in milliseconds
        private double _MinPulseCalibration = 1.0;
        private double _MaxPulseCalibration = 2.0;

        /// <summary>
        /// Sets the position of the Servo Motor.
        /// </summary>
        /// <param name="position">The position of the servo between 0 and 180 degrees.</param>
        public void SetPosition(double position) {
            if (position < 0 || position > 180) throw new ArgumentOutOfRangeException("degrees", "degrees must be between 0 and 180.");

            // Typically, with 50 hz, 0 degree is 0.05 and 180 degrees is 0.10
            //double duty = ((position / 180.0) * (0.10 - 0.05)) + 0.05;
            var duty = ((position / 180.0) * (this._MaxPulseCalibration / 20 - this._MinPulseCalibration / 20)) + this._MinPulseCalibration / 20;


            this.servo.SetActiveDutyCyclePercentage(duty);
            this.servo.Start();
        }
    }
}

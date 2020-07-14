using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Demos.Properties;
using GHIElectronics.TinyCLR.Devices.Can;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Devices.Storage;
using GHIElectronics.TinyCLR.Drivers.Microchip.Winc15x0;
using GHIElectronics.TinyCLR.IO;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Media;

namespace Demos {
    public class BasicTestWindow : ApplicationWindow {
        private Canvas canvas; // can be StackPanel

        private const string Instruction1 = "This step will do simple test on:";
        private const string Instruction2 = " - User leds";
        private const string Instruction3 = " - User buttons";
        private const string Instruction4 = " - External RAM / Flash";
        private const string Instruction5 = " - Wifi";
        private const string Instruction6 = " - Buzzer";
        private const string Instruction7 = " - Usb Host/ Micro Sd";
        private const string Instruction8 = " - Real time clock crystal";

        private const string MountSuccess = "Mounted successful.";
        private const string BadConnect1 = "Bad device or no connect.";


        private Font font;

        private bool isRunning;

        private TextFlow textFlow;

        private Button testButton;

        private Button nextButton;

        private bool ethernetConnect = false;

        private bool doNext = false;

        public BasicTestWindow(Bitmap icon, string text, int width, int height) : base(icon, text, width, height) {
            this.font = Resources.GetFont(Resources.FontResources.droid_reg11);

            this.testButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Test") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.nextButton = new Button() {
                Child = new GHIElectronics.TinyCLR.UI.Controls.Text(this.font, "Next") {
                    ForeColor = Colors.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = 100,
                Height = 30,
            };

            this.testButton.Click += this.TestButton_Click;
            this.nextButton.Click += this.NextButton_Click;
        }

        private void Initialize() {

            this.textFlow = new TextFlow();

            this.textFlow.TextRuns.Add(Instruction1, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction2, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction3, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction4, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction5, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction6, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction7, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);

            this.textFlow.TextRuns.Add(Instruction8, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(0xFF, 0xFF, 0xFF));
            this.textFlow.TextRuns.Add(TextRun.EndOfLine);


        }

        private void Deinitialize() {

            this.textFlow.TextRuns.Clear();
            this.textFlow = null;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {

                if (!this.isRunning) {
                    this.ClearScreen();

                    this.CreateWindow(false);

                    this.textFlow.TextRuns.Clear();

                    new Thread(this.ThreadTest).Start();
                }
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) {
            if (e.RoutedEvent.Name.CompareTo("TouchUpEvent") == 0) {
                this.doNext = true;
            }
        }


        protected override void Active() {
            // To initialize, reset your variable, design...
            this.Initialize();

            this.canvas = new Canvas();

            this.Child = this.canvas;

            this.isRunning = false;

            this.ClearScreen();
            this.CreateWindow(true);

            this.ethernetConnect = false;
        }

        private void TemplateWindow_OnBottomBarButtonBackTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Back Touch event
            this.Close();

        private void TemplateWindow_OnBottomBarButtonNextTouchUpEvent(object sender, RoutedEventArgs e) =>
            // This is Button Next Touch event
            this.Close();

        protected override void Deactive() {
            this.isRunning = false;

            Thread.Sleep(10);
            // To stop or free, uinitialize variable resource
            this.canvas.Children.Clear();

            this.Deinitialize();
        }

        private void ClearScreen() {
            this.canvas.Children.Clear();

            // Enable TopBar
            if (this.TopBar != null) {
                Canvas.SetLeft(this.TopBar, 0); Canvas.SetTop(this.TopBar, 0);
                this.canvas.Children.Add(this.TopBar);
            }

            // Enable BottomBar - If needed
            if (this.BottomBar != null) {
                Canvas.SetLeft(this.BottomBar, 0); Canvas.SetTop(this.BottomBar, this.Height - this.BottomBar.Height);
                this.canvas.Children.Add(this.BottomBar);

                // Regiter touch event for button back or next
                this.OnBottomBarButtonBackTouchUpEvent += this.TemplateWindow_OnBottomBarButtonBackTouchUpEvent;
                this.OnBottomBarButtonNextTouchUpEvent += this.TemplateWindow_OnBottomBarButtonNextTouchUpEvent;
            }

        }

        private void CreateWindow(bool enablebutton) {
            var startX = 5;
            var startY = 40;

            Canvas.SetLeft(this.textFlow, startX); Canvas.SetTop(this.textFlow, startY);
            this.canvas.Children.Add(this.textFlow);

            if (enablebutton) {
                var buttonY = this.Height - ((this.testButton.Height * 3) / 2);

                Canvas.SetLeft(this.testButton, startX); Canvas.SetTop(this.testButton, buttonY);
                this.canvas.Children.Add(this.testButton);
            }
        }

        private void AddNextButton() {
            var startX = 5;
            var buttonY = this.Height - ((this.testButton.Height * 3) / 2);

            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {

                Canvas.SetLeft(this.nextButton, startX); Canvas.SetTop(this.nextButton, buttonY);
                this.canvas.Children.Add(this.nextButton);

                return null;

            }, null);

            Thread.Sleep(100);

            this.doNext = false;
        }

        private void RemoveNextButton() {
            Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(100), _ => {

                this.canvas.Children.Remove(this.nextButton);

                return null;

            }, null);

            Thread.Sleep(100);

            this.doNext = false;
        }

        private void ThreadTest() {
            this.isRunning = true;

            if (this.DoTestLeds() == true) {
                if (this.isRunning == true && this.DoTestButtons() == true) {
                    if (this.isRunning == true && this.DoTestExternalRam() == true) {
                        if (this.isRunning == true && this.DoTestExternalFlash() == true) {
                            if (this.isRunning == true && this.DoTestWifi() == true) {
                                if (this.isRunning == true && this.DoTestBuzzer() == true) {
                                    if (this.isRunning == true && this.DoTestUsbHost()) {
                                        if (this.isRunning == true && this.DoTestSdcard() == true) {
                                            if (this.isRunning == true && this.DoTestRtc() == true) {

                                                this.UpdateStatusText(Instruction2 + ": Passed by tester.", true, System.Drawing.Color.Yellow);
                                                this.UpdateStatusText(Instruction3 + ": Passed.", false);
                                                this.UpdateStatusText(Instruction4 + ": Passed.", false);
                                                this.UpdateStatusText(Instruction5 + ": Passed.", false);
                                                this.UpdateStatusText(Instruction6 + ": Passed by tester.", false, System.Drawing.Color.Yellow);
                                                this.UpdateStatusText(Instruction7 + ": Passed.", false);
                                                this.UpdateStatusText(Instruction8 + ": Passed.", false);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.isRunning = false;
        }

        private bool DoTestExternalRam() {
            var result = true;

            var externalRam1 = new UnmanagedBuffer(16 * 1024 * 1024);
            var externalRam2 = new UnmanagedBuffer(14 * 1024 * 1024);

            byte[] buf1 = null;
            byte[] buf2 = null;

            var useUnmanagedHeap = false;

            if (GHIElectronics.TinyCLR.Native.Memory.UnmanagedMemory.FreeBytes == 0 &&
                GHIElectronics.TinyCLR.Native.Memory.ManagedMemory.FreeBytes > 512 * 1024) {
                buf1 = new byte[16 * 1024 * 1024];
                buf2 = new byte[14 * 1024 * 1024];
            }
            else {
                buf1 = externalRam1.Bytes;
                buf2 = externalRam2.Bytes;

                useUnmanagedHeap = true;
            }



            var md5 = GHIElectronics.TinyCLR.Cryptography.MD5.Create();

            var hashValue = md5.ComputeHash(buf1); //data is a byte array.

            var rd = new Random(3);

            this.UpdateStatusText("Testing external ram. It will take ~ 2 seconds to get test result.", true, System.Drawing.Color.White);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("External ram test is starting...", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("**If you are waiting more than 5 seconds, meaning RAM test is failed.**", false);

            Thread.Sleep(100);

            rd.NextBytes(buf2);

            try {

                var hashValue2 = md5.ComputeHash(buf1); //data is a byte array.

                for (var i = 0; i < hashValue.Length; i++) {
                    if (hashValue[i] != hashValue2[i]) {
                        result = false;
                    }
                }
            }
            catch {

                result = false;
            }

            if (useUnmanagedHeap) {
                externalRam1.Dispose();
                externalRam2.Dispose();
            }

            if (result)
                this.UpdateStatusText("Test external ram passed.", false);
            else
                this.UpdateStatusText("Test external ram failed.", false, System.Drawing.Color.Red);

            return result;

        }

        private bool DoTestExternalFlash() {
            var storeController = StorageController.FromName(SC20260.StorageController.QuadSpi);
            var drive = storeController.Provider;
            var result = true;



            drive.Open();


            var sectorSize = drive.Descriptor.RegionSizes[0];

            var textWrite = System.Text.UTF8Encoding.UTF8.GetBytes("this is for test");

            var dataRead = new byte[sectorSize];
            var dataWrite = new byte[sectorSize];

            for (var i = 0; i < sectorSize; i += textWrite.Length) {
                Array.Copy(textWrite, 0, dataWrite, i, textWrite.Length);
            }

            var roundTest = 0;
            var startSector = 0;
            var endSector = 8;

_again:
            if (roundTest == 1) {
                startSector = 4088;
                endSector = startSector + 8;
            }

            for (var s = startSector; s < endSector; s++) {

                this.UpdateStatusText("Testing external flash.", true);

                var address = s * sectorSize;
                this.UpdateStatusText("External flash - Erasing sector " + s, false);
                // Erase
                drive.Erase(address, sectorSize, TimeSpan.FromSeconds(100));

                // Read - check for blank
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                for (var idx = 0; idx < sectorSize; idx++) {
                    if (dataRead[idx] != 0xFF) {

                        this.UpdateStatusText("External flash - Erase failed at: " + idx, false);
                        result = false;
                        goto _return;
                    }
                }

                // Write
                this.UpdateStatusText("External flash - Writing sector " + s, false);
                drive.Write(address, sectorSize, dataWrite, 0, TimeSpan.FromSeconds(100));

                this.UpdateStatusText("External flash - Reading sector " + s, false);
                //Read to compare
                drive.Read(address, sectorSize, dataRead, 0, TimeSpan.FromSeconds(100));

                for (var idx = 0; idx < sectorSize; idx++) {
                    if (dataRead[idx] != dataWrite[idx]) {

                        this.UpdateStatusText("External flash - Compare failed at: " + idx, false);
                        result = false;
                        goto _return;
                    }

                }
            }

            roundTest++;

            if (roundTest == 2) {
                this.UpdateStatusText("Tested Quad Spi successful!", false);
            }
            else {
                goto _again;
            }


_return:
            drive.Close();

            return result;
        }

        private bool DoTestWifi() {

            var gpioController = GpioController.GetDefault();

            var resetPin = gpioController.OpenPin(SC20260.GpioPin.PF8);
            var csPin = gpioController.OpenPin(SC20260.GpioPin.PA6);
            var intPin = gpioController.OpenPin(SC20260.GpioPin.PF10);
            var enPin = gpioController.OpenPin(SC20260.GpioPin.PA8);

            enPin.SetDriveMode(GpioPinDriveMode.Output);
            resetPin.SetDriveMode(GpioPinDriveMode.Output);

            enPin.Write(GpioPinValue.Low);
            resetPin.Write(GpioPinValue.Low);
            Thread.Sleep(100);

            enPin.Write(GpioPinValue.High);
            resetPin.Write(GpioPinValue.High);

            var result = true;

            var settings = new GHIElectronics.TinyCLR.Devices.Spi.SpiConnectionSettings() {
                ChipSelectLine = csPin,
                ClockFrequency = 4000000,
                Mode = GHIElectronics.TinyCLR.Devices.Spi.SpiMode.Mode0,
                ChipSelectType = GHIElectronics.TinyCLR.Devices.Spi.SpiChipSelectType.Gpio,
                ChipSelectHoldTime = TimeSpan.FromTicks(10),
                ChipSelectSetupTime = TimeSpan.FromTicks(10)
            };

            var networkCommunicationInterfaceSettings = new SpiNetworkCommunicationInterfaceSettings {
                SpiApiName = SC20260.SpiBus.Spi3,
                GpioApiName = "GHIElectronics.TinyCLR.NativeApis.STM32H7.GpioController\\0",
                SpiSettings = settings,
                InterruptPin = intPin,
                InterruptEdge = GpioPinEdge.FallingEdge,
                InterruptDriveMode = GpioPinDriveMode.InputPullUp,
                ResetPin = resetPin,
                ResetActiveState = GpioPinValue.Low
            };

            var networkInterfaceSetting = new WiFiNetworkInterfaceSettings() {
                Ssid = " ",
                Password = " ",
            };

            networkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 1, 122 });
            networkInterfaceSetting.SubnetMask = new IPAddress(new byte[] { 255, 255, 255, 0 });
            networkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
            networkInterfaceSetting.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 75, 75, 75, 75 }), new IPAddress(new byte[] { 75, 75, 75, 76 }) };

            //networkInterfaceSetting.MacAddress = new byte[] { 0x00, 0x04, 0x00, 0x00, 0x00, 0x00 };
            networkInterfaceSetting.IsDhcpEnabled = true;
            networkInterfaceSetting.IsDynamicDnsEnabled = true;

            var networkController = NetworkController.FromName("GHIElectronics.TinyCLR.NativeApis.ATWINC15xx.NetworkController");

            networkController.SetInterfaceSettings(networkInterfaceSetting);
            networkController.SetCommunicationInterfaceSettings(networkCommunicationInterfaceSettings);
            networkController.SetAsDefaultController();

            var firmware = Winc15x0Interface.GetFirmwareVersion();


            if (firmware.IndexOf("255.255.255.65535") == 0) {
                result = false;
            }

            resetPin.Dispose();
            csPin.Dispose();
            intPin.Dispose();
            enPin.Dispose();

            return result;
        }
        private bool DoTestButtons() {
            var gpioController = GpioController.GetDefault();

            var ldrButton = gpioController.OpenPin(SC20260.GpioPin.PE3);
            var appButton = gpioController.OpenPin(SC20260.GpioPin.PB7);

            ldrButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            appButton.SetDriveMode(GpioPinDriveMode.InputPullUp);

            this.UpdateStatusText("Testing buttons.", true);


            this.UpdateStatusText("Wait for press LDR button ", false);
            while (ldrButton.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (ldrButton.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            this.UpdateStatusText("Wait for press APP button ", false);
            while (appButton.Read() == GpioPinValue.High && this.isRunning) Thread.Sleep(100);
            while (appButton.Read() == GpioPinValue.Low && this.isRunning) Thread.Sleep(100);

            ldrButton.Dispose();
            appButton.Dispose();

            return true;
        }

        private bool DoTestLeds() {
            var gpioController = GpioController.GetDefault();

            var redLed = gpioController.OpenPin(SC20260.GpioPin.PB0);

            redLed.SetDriveMode(GpioPinDriveMode.Output);

            this.UpdateStatusText("Testing the red led.", true);
            this.UpdateStatusText("- The test is passed if red is blinking.", false);
            this.UpdateStatusText(" ", false);
            this.UpdateStatusText("- Only press Next button if the led are blinking.", false, System.Drawing.Color.Yellow);

            this.AddNextButton();

            while (this.doNext == false && this.isRunning) {
                redLed.Write(redLed.Read() == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);

                Thread.Sleep(100);
            }

            redLed.Dispose();

            this.RemoveNextButton();

            return true;
        }

        private bool DoTestUsbHost() {

            var result = true;
            this.UpdateStatusText("Waiting for usb host initialize...", true);

            UsbWindow.InitializeUsbHostController();

            while (!UsbWindow.IsUsbHostConnected) Thread.Sleep(100);

            var storageController = StorageController.FromName(SC20260.StorageController.UsbHostMassStorage);

            IDriveProvider drive;

            try {
                drive = FileSystem.Mount(storageController.Hdc);

                var driveInfo = new DriveInfo(drive.Name);


                this.UpdateStatusText(MountSuccess, false);

            }
            catch {

                this.UpdateStatusText("Usb Host: " + BadConnect1, true);

                result = false;

                goto _return;
            }

_return:

            GHIElectronics.TinyCLR.IO.FileSystem.Flush(storageController.Hdc);
            GHIElectronics.TinyCLR.IO.FileSystem.Unmount(storageController.Hdc);

            return result;
        }

        private bool DoTestSdcard() {

            var result = true;

            this.UpdateStatusText("Waiting for Sd initialize...", true);

            var storageController = StorageController.FromName(SC20260.StorageController.SdCard);

            IDriveProvider drive;
try_again:

            try {
                drive = FileSystem.Mount(storageController.Hdc);

                var driveInfo = new DriveInfo(drive.Name);


                this.UpdateStatusText(MountSuccess, false);

            }
            catch {

                this.UpdateStatusText("Sd: " + BadConnect1, true);

                while (this.doNext == false) {

                    Thread.Sleep(1000);

                    goto try_again;
                }

                result = false;

                goto _return;
            }

_return:

            GHIElectronics.TinyCLR.IO.FileSystem.Flush(storageController.Hdc);
            GHIElectronics.TinyCLR.IO.FileSystem.Unmount(storageController.Hdc);

            return result;
        }

        private bool DoTestBuzzer() {

            this.UpdateStatusText("Testing buzzer...", true);

            using (var pwmController3 = GHIElectronics.TinyCLR.Devices.Pwm.PwmController.FromName(SC20260.PwmChannel.Controller3.Id)) {

                var pwmPinPB1 = pwmController3.OpenChannel(SC20260.PwmChannel.Controller3.PB1);

                pwmController3.SetDesiredFrequency(500);
                pwmPinPB1.SetActiveDutyCyclePercentage(0.5);

                this.UpdateStatusText("Generate Pwm 500Hz...", false);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                this.UpdateStatusText("Generate Pwm 1000Hz...", false);

                pwmController3.SetDesiredFrequency(1000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                this.UpdateStatusText("Generate Pwm 2000Hz...", false);

                pwmController3.SetDesiredFrequency(2000);

                pwmPinPB1.Start();

                Thread.Sleep(1000);

                pwmPinPB1.Stop();

                pwmPinPB1.Dispose();

                this.UpdateStatusText("Testing is success if you heard three different sounds!", false, System.Drawing.Color.Yellow);
            }

            this.AddNextButton();

            while (this.doNext == false) {
                Thread.Sleep(100);
            }

            this.RemoveNextButton();

            return true;
        }

        private bool DoTestRtc() {
            this.UpdateStatusText("Testing real time clock... ", true);
            var rtc = RtcController.GetDefault();

            var m = new DateTime(2020, 7, 7, 00, 00, 00);

try_again:
            if (rtc.IsValid && rtc.Now > m) {

                return true;
            }

            else {
                var newDt = RtcDateTime.FromDateTime(m);

                rtc.SetTime(newDt);

                if (rtc.IsValid && rtc.Now > m) {

                    return true;
                }
            }

            if (this.isRunning)
                goto try_again;

            return false;
        }


        private void UpdateStatusText(string text, bool clearscreen) => this.UpdateStatusText(text, clearscreen, System.Drawing.Color.White);

        private void UpdateStatusText(string text, bool clearscreen, System.Drawing.Color color) {

            var timeout = 100;

            try {

                var count = this.textFlow.TextRuns.Count + 2;

                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(timeout), _ => {

                    if (clearscreen)
                        this.textFlow.TextRuns.Clear();

                    this.textFlow.TextRuns.Add(text, this.font, GHIElectronics.TinyCLR.UI.Media.Color.FromRgb(color.R, color.G, color.B));
                    this.textFlow.TextRuns.Add(TextRun.EndOfLine);

                    return null;

                }, null);

                if (clearscreen) {
                    while (this.textFlow.TextRuns.Count < 2) {
                        Thread.Sleep(10);
                    }
                }
                else {
                    while (this.textFlow.TextRuns.Count < count) {
                        Thread.Sleep(10);
                    }
                }
            }
            catch {

            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

        }
    }
}
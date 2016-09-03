using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HidSharp;
using MFIGamepadFeeder.Properties;
using MFIGamepadShared;
using MFIGamepadShared.Configuration;
using Newtonsoft.Json;

namespace MFIGamepadFeeder.Gamepads
{
    internal class GamepadManager
    {
        private readonly HidDeviceLoader _hidDeviceLoader;

        private readonly Thread _currentDeviceUpdateThread
            ;

        private Thread _currentGamepadThread;

        public GamepadManager()
        {
            _hidDeviceLoader = new HidDeviceLoader();
            FoundDevices = new ObservableCollection<HidDeviceRepresentation>();

            SelectedDevice = JsonConvert.DeserializeObject<HidDeviceRepresentation>(Settings.Default.SelectedHidDevice);
            if (SelectedDevice != null && !FoundDevices.Contains(SelectedDevice))
            {
                FoundDevices.Add(SelectedDevice);
            }

            _currentDeviceUpdateThread = new Thread(() =>
            {
                while (true)
                {
                    var hidDevices = _hidDeviceLoader.GetDevices();
                    var currentDevices =
                        hidDevices.Select(hidDevice => new HidDeviceRepresentation(hidDevice)).ToList();

                    foreach (var newDevice in currentDevices.Where(device => !FoundDevices.Contains(device)))
                    {
                        Application.Current.Dispatcher.Invoke(() => FoundDevices.Add(newDevice));
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            });
            _currentDeviceUpdateThread.Start();
        }

        public ObservableCollection<HidDeviceRepresentation> FoundDevices { get; }
        public HidDeviceRepresentation SelectedDevice { get; set; }

        public event ErorOccuredEventHandler ErrorOccuredEvent;
        public void Refresh()
        {
            // Kill exisiting gamepad Thread
            _currentGamepadThread?.Abort();
            _currentGamepadThread = new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                // Read User input settings
                var selectedConfigFile = Settings.Default.SelectedConfigFile;
                var selectedHidDevice = Settings.Default.SelectedHidDevice;
                var vJoyId = Settings.Default.SelectedJoyId;

                if (selectedConfigFile == string.Empty || selectedHidDevice == string.Empty)
                {
                    Log("Configuration incomplete");
                    return;
                }

                GamepadConfiguration configuration = null;
                HidDeviceRepresentation hidDeviceRepresentation = null;
                // Load ConfigCreator configuration file or return exception
                try
                {
                    configuration = await GetConfigFromFilePath(selectedConfigFile);
                    hidDeviceRepresentation =
                        JsonConvert.DeserializeObject<HidDeviceRepresentation>(selectedHidDevice);
                Log($"Using {hidDeviceRepresentation}, vJoy {vJoyId}, configuration file: {selectedConfigFile}");
                // Setup device communication
                SetupGamepad(hidDeviceRepresentation, vJoyId, configuration);
                }
                catch (Exception ex)
                {
                    Log($"Error while reading configuration: {ex.Message}");
                }
            });
            _currentGamepadThread.Start();
        }

        /** Open HID stream for user input read and output to Gamepad virtual device (vJoy)*/
        public void SetupGamepad(HidDeviceRepresentation hidDeviceRepresentation, uint gamePadId,
            GamepadConfiguration config)
        {
            Gamepad gamepad = null;
            try
            {
                var device =
                    _hidDeviceLoader.GetDevices(
                        hidDeviceRepresentation.VendorId,
                        hidDeviceRepresentation.ProductId,
                        hidDeviceRepresentation.ProductVersion,
                        hidDeviceRepresentation.SerialNumber
                        ).First();


                if (device == null)
                {
                    Log(@"Failed to open device.");
                    return;
                }

                HidStream stream;
                if (!device.TryOpen(out stream))
                {
                    Log("Failed to open device.");
                    return;
                }

                gamepad = new Gamepad(config, gamePadId);
                gamepad.ErrorOccuredEvent += Gamepad_ErrorOccuredEvent;

                Log("Successfully initialized gamepad.");
                if (!gamepad.plug())
                {
                    // try to unplug forced
                    if (gamepad.unPlug(true))
                        Log("Gamepad unplugged.");
                    return;
                }
                Log("Gamepad plugged in...");

                using (stream)
                {
                    while (true)
                    {
                        var bytes = new byte[device.MaxInputReportLength];
                        int count;
                        try
                        {
                            count = stream.Read(bytes, 0, bytes.Length);

                            if (count > 0)
                            {
                                gamepad.UpdateState(bytes);
                            }
                        }
                        catch (TimeoutException)
                        {
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Log(ex.Message);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            finally
            {
                if (gamepad.unPlug(false))
                    Log("Gamepad unplugged.");
            }
        }

        private async Task<GamepadConfiguration> GetConfigFromFilePath(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                var fileText = await reader.ReadToEndAsync();
                return new GamepadConfiguration(fileText);
            }
        }

        private void Gamepad_ErrorOccuredEvent(object sender, string errorMessage)
        {
            Log(sender?.ToString() + " : " + errorMessage);
        }

        protected virtual void Log(string errormessage)
        {
            ErrorOccuredEvent?.Invoke(this, errormessage);
        }

        public void DisposeAllThreads()
        {
            _currentDeviceUpdateThread.Abort();
            _currentGamepadThread.Abort();

        }
    }
}
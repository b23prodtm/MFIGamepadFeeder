using System;
using System.Collections.ObjectModel;
using System.Linq;
using MFIGamepadFeeder.Gamepads.Configuration;
using MFIGamepadShared.Configuration;
using vXbox;
using static vXbox.IWrapper;

public delegate void ErorOccuredEventHandler(object sender, string errorMessage);

namespace MFIGamepadFeeder
{
    public class Gamepad
    {
        private readonly GamepadConfiguration _config;
        private readonly uint _gamepadId;
        private readonly IWrapper _vBox;

        /**
         * Gamepad IWrapper is created if the VBus exists on the host.
         * **/
        public Gamepad(GamepadConfiguration config, uint gamepadId)
        {
            _config = config;
            _vBox = new IWrapper();
            _gamepadId = gamepadId;

            // Test if bus exists
            bool bus = _vBox.isVBusExists();
            if (bus)
                Log(@"Virtual Xbox bus exists\n\n");
            else
            {
                Log(@"Virtual Xbox bus does NOT exist - Aborting\n\n");
                return;
            }


            //uint dllVer = 0, drvVer = 0;
            //var match = _vBox.DriverMatch(ref dllVer, ref drvVer);
            //if (!match)
            //{
            //    Log($@"Version of Driver ({drvVer:X}) does NOT match DLL Version ({dllVer:X})\n");
            //    return;
            //}

            var status = _vBox.GetVJDStatus(_gamepadId);
            if ((status == VJD_STAT_OWN) || ((status == VJD_STAT_FREE) && !_vBox.AcquireVJD(_gamepadId)))
            {
                Log($@"Failed to acquire vJoy device number {_gamepadId}.\n");
                return;
            }
        }

        /** plug Xbox feeder device*/
        public Boolean plug()
        {
            // XboxInterface plugin
            if (!_vBox.PlugIn(_gamepadId))
            {
                Log($@"Failed to plugIn vJoy device number {_gamepadId}.\n");
                return false;
            }
            // reset State
            ResetGamepad(_gamepadId);
            return true;
        }

        /**unplug Xbox feeder device
         Called by the application when exiting */
        public Boolean unPlug(Boolean force)
        {
            if (force)
                return _vBox.UnPlugForce(_gamepadId);
            else
                return _vBox.UnPlug(_gamepadId);
        }
        public event ErorOccuredEventHandler ErrorOccuredEvent;

        private void Log(string message)
        {
            ErrorOccuredEvent?.Invoke(this, message);
        }

        private void ResetGamepad(uint id)
        {
            _vBox.ResetVJD(id);
            var zeroState = new byte[_config.ConfigItems.Count];

            for (var i = 0; i < _config.ConfigItems.Count; i++)
            {
                zeroState[i] = 0;
            }
            UpdateState(zeroState);
        }

        public unsafe void UpdateState(byte[] state)
        {
            //            Log(string.Join(" ", state));
            uint dPad = 0;
            byte AxisX = 0, AxisY = 0, AxisRX = 0, AxisRY = 0, AxisSL0 = 0, AxisSL1 = 0;
            for (var i = 0; i < _config.ConfigItems.Count; i++)
            {
                GetAxis(state, i, &AxisX, &AxisY, &AxisRX, &AxisRY, &AxisSL0, &AxisSL1);
                SetGamepadItem(state, i);
                GetDpad(state, i, &dPad);
            }
            _vBox.SetAxisXY(_gamepadId, &AxisX, &AxisY, hid_X, hid_Y, xinput_LAXIS_DEADZONE);
            _vBox.SetAxisRxy(_gamepadId, &AxisRX, &AxisRY, xinput_RAXIS_DEADZONE);
            _vBox.SetTriggerL(_gamepadId, AxisSL0, xinput_TRIGGER_THRESHOLD);
            _vBox.SetTriggerR(_gamepadId, AxisSL1, xinput_TRIGGER_THRESHOLD);
            _vBox.SetDpad(_gamepadId, dPad);
        }

        /**
         A normalized axis value is sent to vJoy Driver.
            */
        private unsafe void GetAxis(byte[] values, int index, byte* AxisX, byte* AxisY,
            byte* AxisRX, byte* AxisRY, byte* AxisSL0, byte* AxisSL1)
        {
            double value = values[index];
            GamepadConfigurationItem config = _config.ConfigItems[index];
            if (config.Type == GamepadItemType.Axis)
            {
                int maxAxisValue = 0;
                uint targetAxis = config.TargetUsage ?? 0;
                if (targetAxis == 0) return;
                /** get max value for the axis (trigger/Slider is 255 and left/right 
             * analog sticks +-32767 **/
                _vBox.GetVJDAxisMax(_gamepadId, targetAxis, &maxAxisValue);
                /** MFI controller returns [0;Byte.MaxValue] axes, normalize [0;1]*/
                value = NormalizeAxis((byte)value, config.ConvertAxis ?? false);

                if (config.InvertAxis ?? false)
                {
                    value = InvertNormalizedAxis(value);
                }
                /** define byte value with maximum Axis value from xInput*/
                byte bvalue = (byte)(uint)(value * maxAxisValue);
                if (targetAxis == hid_X)
                {
                    *AxisX = bvalue;
                }
                else if (targetAxis == hid_Y)
                {
                    *AxisY = bvalue;
                }
                else if (targetAxis == hid_RX)
                {
                    *AxisRX = bvalue;
                }
                else if (targetAxis == hid_RY)
                {
                    *AxisRY = bvalue;
                }
                else if (targetAxis == hid_SL0)
                {
                    *AxisSL0 = bvalue;
                    /** The MFi controller doesn't provide a left Thumb button 
                     * and the slider/trigger is not recognized as one,
                     * so we can simulate and add a btn press (as the Left Thumb)*/
                    _vBox.SetBtnLT(_gamepadId, ConvertToButtonState(bvalue));
                }
                else if (targetAxis == hid_SL1)
                {
                    *AxisSL1 = bvalue;
                    /** The MFi controller doesn't provide a rightThumb button 
                     * and the slider/trigger is not recognized as one,
                     * so we can simulate and add a btn press (as the Right Thumb)*/
                    _vBox.SetBtnRT(_gamepadId, ConvertToButtonState(bvalue));
                }
            }
        }
        private unsafe void SetGamepadItem(byte[] values, int index)
        {
            double value = values[index];
            GamepadConfigurationItem config = _config.ConfigItems[index];
            if (config.Type == GamepadItemType.Button)
            {
                _vBox.SetBtnAny(_gamepadId, ConvertToButtonState((byte)value), config.TargetButtonId ?? 0);
            }
        }
        /** dPad flags*/
        /** concatenate dPad values */
        private unsafe void GetDpad(byte[] state, int index, uint* dPad)
        {
            bool buttonState = ConvertToButtonState((byte)state[index]);
            if (buttonState)
            {
                GamepadConfigurationItem config = _config.ConfigItems[index];
                if (config.Type == GamepadItemType.DPadUp)
                {
                    *dPad |= xinput_DPAD_UP;
                }

                if (config.Type == GamepadItemType.DPadRight)
                {
                    *dPad |= xinput_DPAD_RIGHT;
                }

                if (config.Type == GamepadItemType.DPadDown)
                {
                    *dPad |= xinput_DPAD_DOWN;
                }

                if (config.Type == GamepadItemType.DPadLeft)
                {
                    *dPad |= xinput_DPAD_LEFT;
                }
            }
        }

        /**
         * param name="valueToNormalize" unsigned 8-bit integer sent by the MFI controller
         * param param name="shouldConvert" rotates axis by 180°. byte.MaxValue/2.0 is the center
         * returns is a double between [0 ; 1.0]
         **/
        private double NormalizeAxis(byte valueToNormalize, bool shouldConvert)
        {
            if (shouldConvert)
            {
                if (valueToNormalize < byte.MaxValue / 2.0)
                {
                    return (valueToNormalize + byte.MaxValue / 2.0) / byte.MaxValue;
                }
                return (valueToNormalize - byte.MaxValue / 2.0) / byte.MaxValue;
            }

            return (double)valueToNormalize / byte.MaxValue;
        }

        /** invert +/- axis value **/
        private double InvertNormalizedAxis(double axisToInvert)
        {
            return 1.0 - axisToInvert;
        }

        /** 
         * returns true if > 0
         * **/
        private bool ConvertToButtonState(byte value)
        {
            return value > 0;
        }
    }
}
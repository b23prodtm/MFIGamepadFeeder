using System.Collections.ObjectModel;
using MFIGamepadFeeder.Gamepads.Configuration;
using MFIGamepadShared.Configuration;
using vXbox;

namespace MFIGamepadConfigCreator
{
    internal class ConfigCreator
    {
        public GamepadConfiguration GetNimbusConfiguration()
        {
            var _vBox = new IWrapper();
            var configItems = new Collection<GamepadConfigurationItem>
            {
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = null,
                    Type = GamepadItemType.Empty
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = null,
                    Type = GamepadItemType.DPadUp
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = null,
                    Type = GamepadItemType.DPadRight
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = null,
                    Type = GamepadItemType.DPadDown
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = null,
                    Type = GamepadItemType.DPadLeft
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = vXbox.IWrapper.xinput_GAMEPAD_A,
                    Type = GamepadItemType.Button
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = vXbox.IWrapper.xinput_GAMEPAD_B,
                    Type = GamepadItemType.Button
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = vXbox.IWrapper.xinput_GAMEPAD_X,
                    Type = GamepadItemType.Button
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = vXbox.IWrapper.xinput_GAMEPAD_Y,
                    Type = GamepadItemType.Button
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = vXbox.IWrapper.xinput_GAMEPAD_LEFT_SHOULDER,
                    Type = GamepadItemType.Button
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = vXbox.IWrapper.xinput_GAMEPAD_RIGHT_SHOULDER,
                    Type = GamepadItemType.Button
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = vXbox.IWrapper.hid_SL0,
                    InvertAxis = false,
                    ConvertAxis = false,
                    TargetButtonId = null,
                    Type = GamepadItemType.Axis
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = vXbox.IWrapper.hid_SL1,
                    InvertAxis = false,
                    ConvertAxis = false,
                    TargetButtonId = null,
                    Type = GamepadItemType.Axis
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = null,
                    InvertAxis = null,
                    ConvertAxis = null,
                    TargetButtonId = vXbox.IWrapper.xinput_GAMEPAD_BACK,
                    Type = GamepadItemType.Button
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = vXbox.IWrapper.hid_X,
                    InvertAxis = false,
                    ConvertAxis = true,
                    TargetButtonId = null,
                    Type = GamepadItemType.Axis
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = vXbox.IWrapper.hid_Y,
                    InvertAxis = false,
                    ConvertAxis = true,
                    TargetButtonId = null,
                    Type = GamepadItemType.Axis
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = vXbox.IWrapper.hid_RX,
                    InvertAxis = false,
                    ConvertAxis = true,
                    TargetButtonId = null,
                    Type = GamepadItemType.Axis
                },
                new GamepadConfigurationItem
                {
                    TargetUsage = vXbox.IWrapper.hid_RY,
                    InvertAxis = false,
                    ConvertAxis = true,
                    TargetButtonId = null,
                    Type = GamepadItemType.Axis
                }
            };

            return new GamepadConfiguration {ConfigItems = configItems};
        }
    }
}
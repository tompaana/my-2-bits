using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace BLEBeaconSample
{
    /// <summary>
    /// Creates beacon instances from received advertisement data.
    /// Can also turn beacon instances back to advertisement data.
    /// </summary>
    public class BeaconFactory
    {
        private const char HexStringSeparator = '-';
        private const byte FirstBeaconDataSectionDataType = 0x01;
        private const byte SecondBeaconDataSectionDataType = 0xFF;
        private const int SecondBeaconDataSectionLengthInBytes = 25;
        private const UInt16 IBeaconManufacturerCompanyId = 0x004C;
        private const UInt16 BeaconTypeAndDataLength = 0x0215;
        private const string IBeaconManufacturerCompanyIdAsString = "4C00";
        private const string DefaultBeaconTypeAndDataLengthAsString = "0215";

        /// <summary>
        /// Constructs a Beacon instance and sets the properties based on the given data.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>A newly created Beacon instance or null in case of a failure.</returns>
        public static Beacon BeaconFromBluetoothLEAdvertisementReceivedEventArgs(BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Beacon beacon = null;

            if (args != null && args.Advertisement != null)
            {
                beacon = BeaconFromDataSectionList(args.Advertisement.DataSections);

                if (beacon != null)
                {
                    beacon.Timestamp = args.Timestamp;
                    beacon.RawSignalStrengthInDBm = args.RawSignalStrengthInDBm;
                }
            }

            return beacon;
        }

        /// <summary>
        /// Constructs a Beacon instance and sets the properties based on the given data.
        /// </summary>
        /// <param name="dataSection">A data section containing beacon data.</param>
        /// <returns>A newly created Beacon instance or null in case of a failure.</returns>
        public static Beacon BeaconFromDataSectionList(IList<BluetoothLEAdvertisementDataSection> dataSections)
        {
            Beacon beacon = null;

            if (dataSections != null && dataSections.Count > 0)
            {
                foreach (BluetoothLEAdvertisementDataSection dataSection in dataSections)
                {
                    if (dataSection != null)
                    {
                        if (dataSection.DataType == SecondBeaconDataSectionDataType)
                        {
                            beacon = BeaconFromDataSection(dataSection);
                        }
                    }
                }
            }

            return beacon;
        }

        /// <summary>
        /// Constructs a Beacon instance and sets the properties based on the given data.
        /// </summary>
        /// <param name="dataSection">A data section containing beacon data.</param>
        /// <returns>A newly created Beacon instance or null in case of a failure.</returns>
        public static Beacon BeaconFromDataSection(BluetoothLEAdvertisementDataSection dataSection)
        {
            Beacon beacon = null;

            if (dataSection != null && dataSection.Data != null)
            {
                beacon = BeaconFromByteArray(dataSection.Data.ToArray());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("BeaconFactory.BeaconFromDataSection(): The given data (section) is null");
            }

            return beacon;
        }

        /// <summary>
        /// Constructs a Beacon instance and sets the properties based on the given data.
        /// 
        /// The expected specification of the data is as follows:
        /// 
        /// Byte(s)     Name
        /// --------------------------
        /// 0-1         Company ID
        /// 2           Beacon Type
        /// 3           Data length after (0x15 -> 21 bytes)
        /// 4-19        Proximity UUID
        /// 20-21       Major
        /// 22-23       Minor
        /// 24          Measured Power
        /// 
        /// The minimum length of the given byte array is 25, and if it is longer, everything
        /// after the 25th bit is ignored.
        /// </summary>
        /// <param name="data">The data to populate the Beacon instance properties with.</param>
        /// <returns>A newly created Beacon instance or null in case of a failure.</returns>
        public static Beacon BeaconFromByteArray([ReadOnlyArray] byte[] data)
        {
            if (data == null || data.Length < SecondBeaconDataSectionLengthInBytes)
            {
                // The given data is null or too short
                return null;
            }

            Beacon beacon = new Beacon();

            try
            {
                beacon.BeaconType = Convert.ToSByte(data[2]); // Byte 2
            }
            catch (OverflowException)
            {
            }

            beacon.ProximityUuid = FormatUuid(BitConverter.ToString(data, 4, 16)); // Bytes 4-19
            beacon.MeasuredPower = Convert.ToSByte(BitConverter.ToString(data, 24, 1), 16); // Byte 24

            // Data is expected to be big endian. Thus, if we are running on a little endian,
            // we need to switch the bytes
            if (BitConverter.IsLittleEndian)
            {
                data = ChangeInt16ArrayEndianess(data);
            }

            beacon.CompanyId = BitConverter.ToInt16(data, 0); // Bytes 0-1        
            beacon.Major = BitConverter.ToUInt16(data, 20); // Bytes 20-21
            beacon.Minor = BitConverter.ToUInt16(data, 22); // Bytes 22-23

            return beacon;
        }

        /// <summary>
        /// Creates a BluetoothLEManufacturerData instance based on iBeacon specifications.
        /// The returned instance can be used as a filter for a BLE advertisement watcher.
        /// 
        /// The returned instance is always the same and does not depend on the values of this
        /// Beacon instance.
        /// </summary>
        /// <returns>BluetoothLEManufacturerData instance based on beacon specifications.</returns>
        public static BluetoothLEManufacturerData DefaultBeaconManufacturerData()
        {
            BluetoothLEManufacturerData manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = IBeaconManufacturerCompanyId;

            DataWriter writer = new DataWriter();
            writer.WriteUInt16(BeaconTypeAndDataLength);

            manufacturerData.Data = writer.DetachBuffer();

            return manufacturerData;
        }

        /// <summary>
        /// Creates the second part of the beacon advertizing packet.
        /// Uses the proximity UUID, major, minor and measured power to create the data section.
        /// </summary>
        /// <param name="beacon">A beacon instance.</param>
        /// <returns>A newly created data section.</returns>
        public static BluetoothLEAdvertisementDataSection BeaconToSecondDataSection(Beacon beacon)
        {
            string[] temp = beacon.ProximityUuid.Split(HexStringSeparator);
            string proximityUuid = string.Join("", temp);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(IBeaconManufacturerCompanyIdAsString);
            stringBuilder.Append(DefaultBeaconTypeAndDataLengthAsString);
            stringBuilder.Append(proximityUuid.ToUpper());

            byte[] beginning = HexStringToByteArray(stringBuilder.ToString());

            byte[] data = new byte[SecondBeaconDataSectionLengthInBytes];
            beginning.CopyTo(data, 0);
            ChangeInt16ArrayEndianess(BitConverter.GetBytes(beacon.Major)).CopyTo(data, 20);
            ChangeInt16ArrayEndianess(BitConverter.GetBytes(beacon.Minor)).CopyTo(data, 22);
            data[24] = (byte)Convert.ToSByte(beacon.MeasuredPower);

            BluetoothLEAdvertisementDataSection dataSection = new BluetoothLEAdvertisementDataSection();
            dataSection.DataType = SecondBeaconDataSectionDataType;
            dataSection.Data = data.AsBuffer();

            return dataSection;
        }

        /// <summary>
        /// Calculates the beacon distance based on the given values.
        /// </summary>
        /// <param name="rawSignalStrengthInDBm">The detected signal strength.</param>
        /// <param name="measuredPower">The device specific measured power as reported by the beacon.</param>
        /// <returns>The distance to the beacon in meters.</returns>
        public static double CalculateDistanceFromRssi(double rawSignalStrengthInDBm, int measuredPower)
        {
            double distance = 0d;
            double near = rawSignalStrengthInDBm / measuredPower;

            if (near < 1.0f)
            {
                distance = Math.Pow(near, 10);
            }
            else
            {
                distance = ((0.89976f) * Math.Pow(near, 7.7095f) + 0.111f);
            }

            return distance;
        }

        /// <summary>
        /// Formats the given UUID. The method also accepts strings, which do
        /// not have the full UUID (are shorter than expected). Too long
        /// strings are truncated.
        /// 
        /// An example of a formatted UUID: de305d54-75b4-431b-adb2-eb6b9e546014
        /// </summary>
        /// <param name="uuid">A UUID to format.</param>
        /// <returns>The formatted UUID.</returns>
        public static string FormatUuid(string uuid)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (uuid.Length > 0 && uuid.Trim().Length > 0)
            {
                uuid = uuid.Trim();
                uuid = uuid.Replace(HexStringSeparator.ToString(), string.Empty);

                if (uuid.Length > 8)
                {
                    stringBuilder.Append(uuid.Substring(0, 8));
                    stringBuilder.Append(HexStringSeparator);

                    if (uuid.Length > 12)
                    {
                        stringBuilder.Append(uuid.Substring(8, 4));
                        stringBuilder.Append(HexStringSeparator);

                        if (uuid.Length > 16)
                        {
                            stringBuilder.Append(uuid.Substring(12, 4));
                            stringBuilder.Append(HexStringSeparator);

                            if (uuid.Length > 20)
                            {
                                stringBuilder.Append(uuid.Substring(16, 4));
                                stringBuilder.Append(HexStringSeparator);

                                if (uuid.Length > 32)
                                {
                                    stringBuilder.Append(uuid.Substring(20, 12));
                                }
                                else
                                {
                                    stringBuilder.Append(uuid.Substring(20));
                                }
                            }
                            else
                            {
                                stringBuilder.Append(uuid.Substring(16));
                            }
                        }
                        else
                        {
                            stringBuilder.Append(uuid.Substring(12));
                        }
                    }
                    else
                    {
                        stringBuilder.Append(uuid.Substring(8));
                    }
                }
                else
                {
                    stringBuilder.Append(uuid);
                }
            }

            return stringBuilder.ToString().ToLower();
        }

        /// <summary>
        /// Converts the given hex string to byte array.
        /// </summary>
        /// <param name="hexString">The hex string to convert.</param>
        /// <returns>The given hex string as a byte array.</returns>
        private static byte[] HexStringToByteArray(string hexString)
        {
            return Enumerable.Range(0, hexString.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// Switches the endianess of the given byte array so that every two bytes
        /// are switched.
        /// </summary>
        /// <param name="byteArray">A byte array, whose endianess needs to be changed.</param>
        /// <returns>The modified byte array.</returns>
        private static byte[] ChangeInt16ArrayEndianess(byte[] byteArray)
        {
            byte[] convertedArray = new byte[byteArray.Length];

            for (int i = 0; i < byteArray.Length; i += 2)
            {
                if (i + 1 < byteArray.Length)
                {
                    convertedArray[i] = byteArray[i + 1];
                    convertedArray[i + 1] = byteArray[i];
                }
                else
                {
                    convertedArray[i] = byteArray[i];
                }
            }

            return convertedArray;
        }
    }
}

using System;
using System.ComponentModel;
using System.Threading;

namespace BLEBeaconSample
{
	public class Beacon : INotifyPropertyChanged
	{
        private const int BeaconNotInRangeTimeoutInSeconds = 2;
        private const int LastSeenTimerTimeoutInMilliseconds = 1000;
        public event PropertyChangedEventHandler PropertyChanged;
        private Timer _lastSeenTimer;
        private object _lockerObject;

        public int CompanyId
        {
            get;
            set;
        }

        public int BeaconType
        {
            get;
            set;
        }

        private string _proximityUuid;
		public string ProximityUuid
		{
			get
			{
                return _proximityUuid;
			}
            set
            {
                if (string.IsNullOrEmpty(_proximityUuid) || !_proximityUuid.Equals(value))
                {
                    _proximityUuid = value;
                    NotifyPropertyChanged("ProximityUuid");
                }
            }
		}

        private UInt16 _major;
		public UInt16 Major
		{
			get
			{
                return _major;
			}
            set
            {
                if (_major != value)
                {
                    _major = value;
                    NotifyPropertyChanged("Major");
                }
            }
		}

        private UInt16 _minor;
		public UInt16 Minor
		{
			get
			{
				return _minor;
			}
            set
            {
                if (_minor != value)
                {
                    _minor = value;
                    NotifyPropertyChanged("Minor");
                }
            }
		}

        private int _rawSignalStrengthInDBm;
		public int RawSignalStrengthInDBm
		{
			get
			{
                return _rawSignalStrengthInDBm;
			}
            set
            {
                if (_rawSignalStrengthInDBm != value)
                {
                    _rawSignalStrengthInDBm = value;
                    NotifyPropertyChanged("RawSignalStrengthInDBm");

                    CalculateDistance(_rawSignalStrengthInDBm, MeasuredPower);
                }
            }
		}

        private int _measuredPower;
		public int MeasuredPower
		{
			get
			{
                return _measuredPower;
			}
            set
            {
                if (_measuredPower != value)
                {
                    _measuredPower = value;
                    NotifyPropertyChanged("MeasuredPower");

                    CalculateDistance(RawSignalStrengthInDBm, _measuredPower);
                }
            }
		}

        private DateTimeOffset _timestamp;
		public DateTimeOffset Timestamp
		{
            get
            {
                return _timestamp;
            }
            set
            {
                _timestamp = value;
                NotifyPropertyChanged("Timestamp");

                TimeSpan timeElapsedSinceLastSeen = DateTime.Now - _timestamp;
                SecondsElapsedSinceLastSeen = (int)timeElapsedSinceLastSeen.TotalSeconds;
            }
		}

        private double _distance;
        public double Distance
        {
            get
            {
                return _distance;
            }
            private set
            {
                if (_distance != value)
                {
                    _distance = value;
                    NotifyPropertyChanged("Distance");

                    UpdateRange();
                }
            }
        }

        private int _range;
        public int Range
        {
            get
            {
                return _range;
            }
            set
            {
                if (_range != value)
                {
                    _range = value;
                    NotifyPropertyChanged("Range");
                }
            }
        }

        private int _secondsElapsedSinceLastSeen;
		public int SecondsElapsedSinceLastSeen
		{
			get
			{
                return _secondsElapsedSinceLastSeen;
			}
            private set
            {
                if (_secondsElapsedSinceLastSeen != value)
                {
                    _secondsElapsedSinceLastSeen = value;
                    NotifyPropertyChanged("SecondsElapsedSinceLastSeen");
                }
            }
		}

        public Beacon()
        {
            _lockerObject = new Object();
            _lastSeenTimer = new Timer(LastSeenTimerCallbackAsync, null,
                LastSeenTimerTimeoutInMilliseconds, LastSeenTimerTimeoutInMilliseconds);
        }

        /// <summary>
        /// Updates the beacon data, if the given beacon matches this one.
        /// </summary>
        /// <param name="beacon">The beacon with new data.</param>
        /// <returns>True, if the given beacon matches this one (and the data was updated). False otherwise.</returns>
        public bool Update(Beacon beacon)
        {
            bool matches = Matches(beacon);

            if (matches)
            {
                Timestamp = beacon.Timestamp;
                RawSignalStrengthInDBm = beacon.RawSignalStrengthInDBm;

                if (_lastSeenTimer != null)
                {
                    _lastSeenTimer.Dispose();
                    _lastSeenTimer = new Timer(LastSeenTimerCallbackAsync, null,
                        LastSeenTimerTimeoutInMilliseconds, LastSeenTimerTimeoutInMilliseconds);
                }
            }

            return matches;
        }

        /// <summary>
        /// Compares the given beacon to this.
        /// </summary>
        /// <param name="beacon">The beacon to compare to.</param>
        /// <returns>True, if the beacons match.</returns>
        public bool Matches(Beacon beacon)
        {
            return beacon.ProximityUuid.Equals(ProximityUuid)
                && beacon.Major == Major
                && beacon.Minor == Minor;
        }

        public override string ToString()
        {
            return BeaconFactory.FormatUuid(ProximityUuid) + ":" + Major + ":" + Minor;
        }

        private void CalculateDistance(int rawSignalStrengthInDBm, int measuredPower)
        {
            if (rawSignalStrengthInDBm != 0 && measuredPower != 0)
            {
                Distance = Math.Round(BeaconFactory.CalculateDistanceFromRssi(rawSignalStrengthInDBm, measuredPower), 1);
            }
        }

        private void UpdateRange()
        {
            if (SecondsElapsedSinceLastSeen >= BeaconNotInRangeTimeoutInSeconds)
            {
                Range = 0;
            }
            else
            {
                if (Distance <= 2.0d)
                {
                    Range = 4;
                }
                else if (Distance <= 5.0d)
                {
                    Range = 3;
                }
                else if (Distance <= 10.0d)
                {
                    Range = 2;
                }
                else
                {
                    Range = 1;
                }
            }
        }

        private async void LastSeenTimerCallbackAsync(object state)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    lock(_lockerObject)
                    {
                        TimeSpan timeElapsedSinceLastSeen = DateTime.Now - Timestamp;
                        SecondsElapsedSinceLastSeen = (int)timeElapsedSinceLastSeen.TotalSeconds;
                    }
                });
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

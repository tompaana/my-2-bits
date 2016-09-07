using System;
using System.ComponentModel;

namespace ProcessMonitor.Logging
{
    public class LogItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                NotifyPropertyChanged("Message");
                
                if (!string.IsNullOrEmpty(_message))
                {
                    MessageWithTimestamp = CreateTimestamp() + " " + _message;
                }
                else
                {
                    MessageWithTimestamp = _message;
                }
            }
        }

        private string _messageWithTimestamp;
        public string MessageWithTimestamp
        {
            get
            {
                return _messageWithTimestamp;
            }
            private set
            {
                _messageWithTimestamp = value;
                NotifyPropertyChanged("MessageWithTimestamp");
            }
        }

        public LogItem()
        {
        }

        public LogItem(string message)
        {
            Message = message;
        }

        private string CreateTimestamp()
        {
            return string.Format("[{0:hh:mm:ss}]", DateTime.Now);
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

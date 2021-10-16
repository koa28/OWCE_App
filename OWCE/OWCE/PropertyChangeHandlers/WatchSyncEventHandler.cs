using System;
using System.Collections.Generic;
using System.ComponentModel;
using OWCE.Converters;
using OWCE.DependencyInterfaces;
using Xamarin.Forms;

namespace OWCE.PropertyChangeHandlers
{
    public class WatchSyncEventHandler
    {
        private static readonly HashSet<String> PropertiesToWatch = new HashSet<string> { "BatteryPercent", "BatteryVoltage", "RPM", "TripOdometer" };

        public static readonly WatchSyncEventHandler Instance = new WatchSyncEventHandler();

        public static Action ForceReconnect { get; set; }

        private Dictionary<string, object> watchUpdates = new Dictionary<string, object>();

        public bool WatchReachable { get; set; }

        // Updates the watch with the given property
        // - propertyName: null if updating all properties
        private void UpdateProperty(string propertyName, OWBoard board)
        {
            board = App.Current.CurrentBoard;
            if (board != null)
            {
                if (propertyName == null || propertyName.Equals("BatteryVoltage"))
                {
                    float voltage = board.BatteryVoltage;
                    watchUpdates["Voltage"] = voltage;

                    // For Quart, should add battery percent here
                    double pct = QuartVoltageConverter.GetPercentFromVoltage(voltage);
                    watchUpdates["BatteryPercent"] =  (int)pct;
                }
                if (propertyName == null || propertyName.Equals("RPM"))
                {
                    int rpm = board.RPM;
                    int speed = (int)RpmToSpeedConverter.ConvertFromRpm(rpm);
                    watchUpdates["Speed"] = speed;
                }
                //if (propertyName == null || propertyName.Equals("BatteryPercent"))
                //{
                //    int batteryPercent = board.BatteryPercent;
                //    watchUpdates["BatteryPercent"] = batteryPercent;
                //}
                if (propertyName == null || propertyName.Equals("TripOdometer"))
                {
                    ushort tripOdometer = board.TripOdometer;
                    string tripDescription = RotationsToDistanceConverter.ConvertRotationsToDistance(tripOdometer);
                    watchUpdates["Distance"] = tripDescription;
                }
            }

            if (propertyName == null)
            {
                watchUpdates["SpeedUnitsLabel"] = App.Current.MetricDisplay ? "km/h" : "mph";
                watchUpdates["AppState"] = App.Current.AppState;
                watchUpdates["ReconnectingErrors"] = App.Current.ReconnectingErrors;
                watchUpdates["TimeStarted"] = App.Current.TimeStarted.ToString("yyyy-MM-dd HH:mm");
                watchUpdates["BoardName"] = App.Current.CurrentBoard == null ? "Board: null" : App.Current.CurrentBoard.Name;
                watchUpdates["BoardConnectionState"] = string.Format("State: {0}", App.Current.ConnectionState);
            }
            // TODO: Here we can implement delayed send
            FlushMessagesIfNecessary(propertyName != null);
        }

        private void FlushMessagesIfNecessary(bool checkForWatchReachability)
        {
            if (!checkForWatchReachability || WatchReachable) { FlushMessages(); }
        }

        private void FlushMessages()
        {
            var updates = watchUpdates;
            watchUpdates = new Dictionary<string, object>();

            IWatch watchService = DependencyService.Get<IWatch>();
            watchService.SendWatchMessages(updates);
        }

        // Invoked when the OWBoard has properties changed (eg Speed, Voltage) that we need
        // to update the watch on
        public static void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (!PropertiesToWatch.Contains(e.PropertyName)) { return; }

                Instance.UpdateProperty(e.PropertyName, (sender as OWBoard));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Handling Watch Property Change: {ex.Message}");
                //(sender as OWBoard).ErrorMessage = $"Exception Handling Watch Property Change: {ex.Message}";
            }

        }

        // Invoked when the watch sends messages to the phone (eg when the watch wakes up)
        public static void HandleWatchMessage(Dictionary<string, object> message, OWBoard board)
        {
            try
            {
                if (message.ContainsKey("WatchAppAwake"))
                {
                    //if (board == null)
                    //{
                    //    Console.WriteLine("Board not initialized yet. Returning");
                    //    return;
                    //}
                    // Watch just woke up -- send all current data to bring
                    // the watch up to speed
                    Instance.UpdateProperty(null, board);
                }
                else if (message.ContainsKey("ForceReconnect"))
                {
                    ForceReconnect?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Handling Watch Message: {ex.Message}");
            }
        }
    }
}

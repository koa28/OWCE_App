﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using OWCE.Converters;
using OWCE.DependencyInterfaces;
using Xamarin.Forms;

namespace OWCE.PropertyChangeHandlers
{
    public class WatchSyncEventHandler
    {
        private static readonly HashSet<String> PropertiesToWatch =
            new HashSet<string> { "BatteryPercent", "BatteryVoltage", "RPM", "TripOdometer" };

        public static readonly WatchSyncEventHandler Instance = new WatchSyncEventHandler();

        public static Action ForceReconnect { get; set; }
        public static Action ForceDisconnect { get; set; }

        private Dictionary<WatchMessage, object> watchUpdates = new Dictionary<WatchMessage, object>();

        public bool WatchReachable { get; set; }

        // Updates the watch with the given property
        // - propertyName: null if updating all properties
        private void UpdateProperty(string propertyName, OWBoard board)
        {
            board = App.Current.CurrentBoard;
            if (board != null)
            {
                if (propertyName == null || propertyName.Equals("BatteryVoltage"))
                    float voltage = board.BatteryVoltage;
                    watchUpdates[WatchMessage.Voltage] = voltage;

                    // For Quart, should add battery percent here
                    double pct = QuartVoltageConverter.GetPercentFromVoltage(voltage);
                    watchUpdates[WatchMessage.BatteryPercent] =  (int)pct;
                }

                if (propertyName == null || propertyName.Equals("RPM"))
                {
                    int rpm = board.RPM;
                    int speed = (int)RpmToSpeedConverter.ConvertFromRpm(rpm);
                    watchUpdates[WatchMessage.Speed] = speed;
                }

                // if (propertyName == null || propertyName.Equals("BatteryPercent"))
                // {
                //     int batteryPercent = board.BatteryPercent;
                //     watchUpdates[WatchMessage.BatteryPercent] = batteryPercent;
                // }

                if (propertyName == null || propertyName.Equals("TripOdometer"))
                {
                    ushort tripOdometer = board.TripOdometer;
                    string tripDescription = RotationsToDistanceConverter.ConvertRotationsToDistance(tripOdometer);
                    watchUpdates[WatchMessage.Distance] = tripDescription;
                }
            }

            if (propertyName == null)
            {
                watchUpdates[WatchMessage.SpeedUnitsLabel] = App.Current.MetricDisplay ? "km/h" : "mph";
                watchUpdates[WatchMessage.AppState] = App.Current.AppState;
                watchUpdates[WatchMessage.ReconnectingErrors] = App.Current.ReconnectingErrors;
                watchUpdates[WatchMessage.TimeStarted] = App.Current.TimeStarted.ToString("yyyy-MM-dd HH:mm");
                watchUpdates[WatchMessage.BoardName] = App.Current.CurrentBoard == null ? "Board: null" : App.Current.CurrentBoard.Name;
                watchUpdates[WatchMessage.BoardConnectionState] = string.Format("State: {0}", App.Current.ConnectionState);
            }
            // TODO: Here we can implement delayed send
            FlushMessagesIfNecessary(propertyName != null);
        }

        private void FlushMessagesIfNecessary(bool checkForWatchReachability)
        {
            if (!checkForWatchReachability || WatchReachable) { FlushMessages(); }
        }

        // Sends all outstanding updates to the watch, and reset the update Dictionary
        private void FlushMessages()
        {
            var updates = watchUpdates;
            watchUpdates = new Dictionary<WatchMessage, object>();

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
                System.Diagnostics.Debug.WriteLine($"Exception Handling Watch Property Change: {ex.Message}");
                //(sender as OWBoard).ErrorMessage = $"Exception Handling Watch Property Change: {ex.Message}";
            }

        }

        // Invoked when the watch sends messages to the phone (eg when the watch wakes up)
        public static void HandleWatchMessage(Dictionary<WatchMessage, object> message, OWBoard board)
        {
            try
            {
                if (message.ContainsKey(WatchMessage.Awake))
                {
                    //if (board == null)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Board not initialized yet. Returning");
                    //    return;
                    //}
                    // Watch just woke up -- send all current data to bring
                    // the watch up to speed
                    Instance.UpdateProperty(null, board);
                }
                else if (message.ContainsKey(WatchMessage.ForceReconnect))
                {
                    ForceReconnect?.Invoke();
                }
                else if (message.ContainsKey(WatchMessage.ForceDisconnect))
                {
                    ForceDisconnect?.Invoke();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception Handling Watch Message: {ex.Message}");
            }
        }
    }
}

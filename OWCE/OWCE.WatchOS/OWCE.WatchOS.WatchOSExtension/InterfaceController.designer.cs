// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace OWCE.WatchOS.WatchOSExtension
{
	[Register ("InterfaceController")]
	partial class InterfaceController
	{
		[Outlet]
		WatchKit.WKInterfaceLabel appStateLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup batteryLabelGroup { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel batteryPercentageLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel boardNameLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup connectToBoardGroup { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel errorMessages { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel myLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel reconnectingErrors { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel reconnectingProgressLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup rideDetailsGroup { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel speedLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup speedLabelGroup { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel speedUnitsLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel timeStartedLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel tripDistanceLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel voltageLabel { get; set; }

		[Action ("darkModeTogglePressed")]
		partial void darkModeTogglePressed ();

		[Action ("forceReconnectButtonPressed")]
		partial void forceReconnectButtonPressed ();
		
		void ReleaseDesignerOutlets ()
		{
			if (appStateLabel != null) {
				appStateLabel.Dispose ();
				appStateLabel = null;
			}

			if (batteryLabelGroup != null) {
				batteryLabelGroup.Dispose ();
				batteryLabelGroup = null;
			}

			if (batteryPercentageLabel != null) {
				batteryPercentageLabel.Dispose ();
				batteryPercentageLabel = null;
			}

			if (connectToBoardGroup != null) {
				connectToBoardGroup.Dispose ();
				connectToBoardGroup = null;
			}

			if (errorMessages != null) {
				errorMessages.Dispose ();
				errorMessages = null;
			}

			if (myLabel != null) {
				myLabel.Dispose ();
				myLabel = null;
			}

			if (reconnectingErrors != null) {
				reconnectingErrors.Dispose ();
				reconnectingErrors = null;
			}

			if (reconnectingProgressLabel != null) {
				reconnectingProgressLabel.Dispose ();
				reconnectingProgressLabel = null;
			}

			if (rideDetailsGroup != null) {
				rideDetailsGroup.Dispose ();
				rideDetailsGroup = null;
			}

			if (speedLabel != null) {
				speedLabel.Dispose ();
				speedLabel = null;
			}

			if (speedLabelGroup != null) {
				speedLabelGroup.Dispose ();
				speedLabelGroup = null;
			}

			if (speedUnitsLabel != null) {
				speedUnitsLabel.Dispose ();
				speedUnitsLabel = null;
			}

			if (timeStartedLabel != null) {
				timeStartedLabel.Dispose ();
				timeStartedLabel = null;
			}

			if (boardNameLabel != null) {
				boardNameLabel.Dispose ();
				boardNameLabel = null;
			}

			if (tripDistanceLabel != null) {
				tripDistanceLabel.Dispose ();
				tripDistanceLabel = null;
			}

			if (voltageLabel != null) {
				voltageLabel.Dispose ();
				voltageLabel = null;
			}
		}
	}
}

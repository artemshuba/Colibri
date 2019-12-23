using System;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Microsoft.Xaml.Interactivity;

namespace Colibri.Behaviors
{
    public class ContextMenuBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;

            var control = associatedObject as FrameworkElement;
            if (control != null)
            {
                control.Holding += ControlOnHolding;
                control.RightTapped += ControlOnRightTapped;
            }
        }


        public void Detach()
        {
            var control = AssociatedObject as FrameworkElement;
            if (control != null)
            {
                control.Holding -= ControlOnHolding;
                control.RightTapped -= ControlOnRightTapped;
            }

            AssociatedObject = null;
        }


        private void ControlOnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Touch)
            {
                Debug.WriteLine("Context menu: right tapped");
                ShowFlyout(e.GetPosition(null));
            }
        }

        private void ControlOnHolding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Started)
            {
                Debug.WriteLine("Context menu: holding");
                ShowFlyout();
            }
        }

        private void ShowFlyout(Point? position = null)
        {
            var control = (FrameworkElement)AssociatedObject;
            MenuFlyout flyout = null;

            flyout = FlyoutBase.GetAttachedFlyout(control) as MenuFlyout;
            if (flyout != null)
            {
                foreach (var menuFlyoutItemBase in flyout.Items)
                {
                    menuFlyoutItemBase.DataContext = control.DataContext;
                }
                //FlyoutBase.ShowAttachedFlyout(control);
                try
                {
                    if (position != null)
                        flyout.ShowAt(null, position.Value);
                    else
                        flyout.ShowAt(control);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to open context menu. " + ex);
                }
            }
        }
    }
}

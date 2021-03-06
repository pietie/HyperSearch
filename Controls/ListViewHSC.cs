﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HscLib;
using System.Windows.Media;
using HyperSearch;
using System.Windows.Input;
using HyperSearch.Classes;

namespace HscLib.Controls
{
    public class ListViewHSC : ListView
    {
        public static readonly DependencyProperty CanScrollUpProperty = DependencyProperty.Register("CanScrollUp", typeof(bool), typeof(ListViewHSC), new UIPropertyMetadata(false, null));
        public static readonly DependencyProperty CanScrollDownProperty = DependencyProperty.Register("CanScrollDown", typeof(bool), typeof(ListViewHSC), new UIPropertyMetadata(false, null));

        public bool CanScrollUp { get { return (bool)this.GetValue(CanScrollUpProperty); } set { this.SetValue(CanScrollUpProperty, value); } }
        public bool CanScrollDown { get { return (bool)this.GetValue(CanScrollDownProperty); } set { this.SetValue(CanScrollDownProperty, value); } }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private Orientation? _onLoadOrientationChange = null;
        public StackPanel StackPanelLayout { get; private set; }
        public ScrollViewer ScrollViewer { get; private set; }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ListViewHSC), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        //public ItemsPanelTemplate ItemsPanel { get; set; }
        public static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var lv = (ListViewHSC)sender;

            if (lv.StackPanelLayout == null)
            {
                lv._onLoadOrientationChange = (Orientation)e.NewValue;
                return;
            }

            lv.StackPanelLayout.Orientation = (Orientation)e.NewValue;
        }

        private void OnStackPanelItemsPanelLoaded(object sender, RoutedEventArgs e)
        {
            if (StackPanelLayout == null)
            {
                StackPanelLayout = (StackPanel)sender;
            }

            if (_onLoadOrientationChange.HasValue)
            {
                StackPanelLayout.Orientation = _onLoadOrientationChange.Value;
                _onLoadOrientationChange = null;
            }
        }


        public ListViewHSC()
        {
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
            VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
            this.Background = null;  // for some reason setting the background property in the Style (ListViewWithScrollIndicatorsStyle) doesn't work
            this.SelectionChanged += OnSelectionChanged;
            this.Loaded += OnLoaded;
            this.PreviewKeyDown += ListViewHSC_PreviewKeyDown;
            this.MouseDoubleClick += ListViewHSC_MouseDoubleClick;
            
        }

        private void ListViewHSC_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("DirectlyOver = {0}", Mouse.DirectlyOver == Keyboard.FocusedElement);
            var target = Keyboard.FocusedElement as FrameworkElement;

            if (target == null) return;

            var htr = VisualTreeHelper.HitTest(target, e.GetPosition(target));

            if (htr == null) return;

            var key = HyperSearchSettings.Instance().Input.Action.FirstKey;
            
            var routedEvent = Keyboard.PreviewKeyDownEvent;

            target.RaiseEvent(
              new KeyEventArgs(
                Keyboard.PrimaryDevice,
                Keyboard.PrimaryDevice.ActiveSource,
                0,
                key)
              { RoutedEvent = routedEvent }
            );
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (this.View == null)
            {
                this.ItemsPanel = new ItemsPanelTemplate();
                var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));

                stackPanelFactory.SetValue(StackPanel.OrientationProperty, this.Orientation);

                stackPanelFactory.AddHandler(StackPanel.LoadedEvent, new RoutedEventHandler(OnStackPanelItemsPanelLoaded));

                this.ItemsPanel.VisualTree = stackPanelFactory;
            }
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var sv = this.FindVisualChildren<ScrollViewer>().FirstOrDefault();

            if (sv != null)
            {
                this.ScrollViewer = sv;
                sv.ScrollChanged += OnScrollChanged;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshScrollIndicators();
        }

        protected void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            RefreshScrollIndicators();
        }

        private void RefreshScrollIndicators()
        {
            if (this.ScrollViewer == null)
            {
                // try to find the scrollviewer again
                this.ScrollViewer = this.FindVisualChildren<ScrollViewer>().FirstOrDefault();

                if (this.ScrollViewer != null)
                {
                    this.ScrollViewer.ScrollChanged += OnScrollChanged;
                }
            }

            if (this.ScrollViewer != null)
            {
                if (this.Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    CanScrollUp = this.ScrollViewer.ContentVerticalOffset > 0;
                    CanScrollDown = this.ScrollViewer.ContentVerticalOffset < this.ScrollViewer.ScrollableHeight;
                }
                else
                {
                    CanScrollUp = this.ScrollViewer.ContentHorizontalOffset > 0;
                    CanScrollDown = this.ScrollViewer.ContentHorizontalOffset < this.ScrollViewer.ScrollableWidth;
                }
            }
        }

        private void ListViewHSC_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //try
            //{
            //    e.Handled = true;
            //    var elementWithFocus = Keyboard.FocusedElement as UIElement;

            //    if (HyperSearchSettings.Instance().Input.Down.Is(e.Key))
            //    {
            //            elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            //        return;
            //    }
            //    else if (HyperSearchSettings.Instance().Input.Up.Is(e.Key))
            //    {
            //        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
            //        return;
            //    }
            //    else if (HyperSearchSettings.Instance().Input.Left.Is(e.Key))
            //    {
            //        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
            //        return;
            //    }
            //    else if (HyperSearchSettings.Instance().Input.Right.Is(e.Key))
            //    {
            //        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
            //        return;
            //    }
            //    else if (HyperSearchSettings.Instance().Input.Back.Is(e.Key))
            //    {
                    
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ErrorHandler.HandleException(ex);
            //}
        }

    }
}

using HyperSearch.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace HyperSearch
{
    public enum OskSpecialKey
    {
        None,
        Backspace,
        Space,
        Clear,
        Done,
        Exit
    }

    public delegate void OnOskKeyPressedHandler(string charRepresentation, OskSpecialKey specialKey);

    public class OskBaseControl : UserControl
    {
        public event OnOskKeyPressedHandler OnOskKeyPressed;
        public TextBox AttachedTextBox { get; set; }

        private ListView listView;
        private OnScreenKeyboardButton space, backspace, clear, done;
        public virtual void FocusInput()
        {
            Window.GetWindow(this).Activate();
            Window.GetWindow(this).Focus();

            if (listView != null)
            {
                listView.Focus();
                Keyboard.Focus(listView);
            }
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            listView = FindName("listView") as ListView;
            space = FindName("space") as OnScreenKeyboardButton;
            backspace = FindName("backspace") as OnScreenKeyboardButton;
            clear = FindName("clear") as OnScreenKeyboardButton;
            done = FindName("done") as OnScreenKeyboardButton;

            if (listView != null)
            {
                listView.Loaded += _listView_Loaded;
                listView.PreviewKeyDown += listView_PreviewKeyDown;
            }

            this.MouseDoubleClick += OskBaseControl_MouseDoubleClick;
        }

        private void OskBaseControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var button = listView.SelectedItem as OnScreenKeyboardButton;

                if (button == null) return;

                HandleOskButtonPressed(button);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void listView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (listView.SelectedItem == null) return;

                e.Handled = true;
                var elementWithFocus = Keyboard.FocusedElement as UIElement;
                var settings = HyperSearchSettings.Instance().Input;

                if (settings.Action.Is(e.Key))
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                        HandleOskButtonPressed(done);
                    }
                    else
                    {
                        var button = listView.SelectedItem as OnScreenKeyboardButton;
                        HandleOskButtonPressed(button);
                    }
                    return;
                }
                else if (settings.Up.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    return;
                }
                else if (settings.Right.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    return;
                }
                else if (settings.Down.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    return;
                }
                else if (settings.Left.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    return;
                }
                else if (settings.Back.Is(e.Key))
                {
                    if (string.IsNullOrEmpty(AttachedTextBox.Text))
                    {
                        this.RaiseOskKeyPressedEvent(null, OskSpecialKey.Exit);
                        return;
                    }

                    listView.SelectedItem = backspace;
                    HandleOskButtonPressed(backspace);
                }
                else if (settings.Clear.Is(e.Key))
                {
                    listView.SelectedItem = clear;
                    HandleOskButtonPressed(clear);
                    return;
                }


                // if not in Cab Mode we'll allow the user to type on his keyboard
                if (!(HyperSearchSettings.Instance().General.CabMode?? false))
                {
                    if ((int)e.Key >= (int)Key.D0 && (int)e.Key <= (int)Key.D9) // handle Digits 0-9
                    {
                        var txt = e.Key.ToString().TrimStart('D');

                        var oskButtons = listView.FindVisualChildren<OnScreenKeyboardButton>().ToList();

                        var button = oskButtons.Where(b => b.Text == txt).FirstOrDefault();

                        if (button != null)
                        {
                            listView.SelectedItem = button;
                            HandleOskButtonPressed(button);
                            return;
                        }
                    }
                    else if (e.Key == Key.Space)
                    {
                        listView.SelectedItem = space;
                        HandleOskButtonPressed(space);
                        return;
                    }
                    //if (((int)e.Key >= (int)Key.A && (int)e.Key <= (int)Key.Z)
                    // || e.Key == Key.OemMinus
                    // || e.Key == Key.OemComma
                    // )
                    else {
                        var txt = e.Key.ToString();

                        if (e.Key == Key.OemMinus) txt = "-";
                        if (e.Key == Key.OemComma) txt = ",";
                        if (e.Key == Key.OemPeriod) txt = ".";

                        var oskButtons = listView.FindVisualChildren<OnScreenKeyboardButton>().ToList();

                        var button = oskButtons.Where(b => b.Text == txt).FirstOrDefault();

                        if (button != null)
                        {
                            listView.SelectedItem = button;
                            HandleOskButtonPressed(button);
                            return;
                        }
                    }
                } // if (!MainWindow.IsCabModeEnabled)

            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void _listView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (listView != null)
            {
                if (listView.Items.Count > 0) listView.SelectedIndex = 0;
                (listView.SelectedItem as OnScreenKeyboardButton).DelayedFocus();
            }
        }

        

        private void HandleOskButtonPressed(OnScreenKeyboardButton button)
        {  
            if (button == space)
            {
                this.RaiseOskKeyPressedEvent(" ", OskSpecialKey.Space);
            }
            else if (button == backspace)
            {
                this.RaiseOskKeyPressedEvent(null, OskSpecialKey.Backspace);
            }
            else if (button == clear)
            {
                this.RaiseOskKeyPressedEvent(null, OskSpecialKey.Clear);
            }
            else if (button == done)
            {
                this.RaiseOskKeyPressedEvent(null, OskSpecialKey.Done);
            }
            else
            {
                this.RaiseOskKeyPressedEvent(button.Text, OskSpecialKey.None);
            }
        }

        protected void RaiseOskKeyPressedEvent(string charRepresentation, OskSpecialKey specialKey)
        {
            if (SystemSoundPlayer.IsInitialised) SystemSoundPlayer.Instance().PlaySound(Classes.SystemSound.LetterClick);

            if (this.AttachedTextBox != null)
            {
                var txt = this.AttachedTextBox;

                if (specialKey == OskSpecialKey.Clear)
                {
                    txt.Text = "";
                }
                else if (specialKey == OskSpecialKey.Backspace)
                {
                    if (txt.Text != null && txt.Text.Length > 0)
                    {
                        txt.Text = txt.Text.Remove(txt.Text.Length - 1);
                    }
                }
                else
                {
                    if (txt.Text == null) txt.Text = "";
                    txt.Text += charRepresentation;
                }

            }

            if (this.OnOskKeyPressed != null)
                this.OnOskKeyPressed(charRepresentation, specialKey);
        }
    }
}

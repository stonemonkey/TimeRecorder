using System.Windows;
using System.Windows.Input;

namespace TimeRecorder
{
    /// <summary>
    /// Represents a control that can be used to display or edit unformatted text and supports the
    /// ability to automatically select all of the text within the control on focus and when releasing
    /// Enter key.
    /// </summary>
    public class TextBoxEx : System.Windows.Controls.TextBox
    {

        #region " Declarations "

        public static DependencyProperty SelectAllOnFocusProperty = DependencyProperty.Register(
            "SelectAllOnFocus", typeof(bool), typeof(TextBoxEx), new System.Windows.PropertyMetadata(true));

        /// <summary>
        /// Used in event handling to determine if mouse capture should be ignored.
        /// </summary>
        private bool CancelGotMouseCapture;

        /// <summary>
        /// Delegate used to invoke the OnFocusComplete method.
        /// </summary>
        private delegate void OnFocusCompleteDelegate(TextBoxEx txt);

        /// <summary>
        /// Used to invoke the OnFocusCompleteDelegate delegate.
        /// </summary>
        private OnFocusCompleteDelegate OnFocusCompleteInvoker = new OnFocusCompleteDelegate(OnFocusComplete);

        #endregion

        #region " Properties "
        /// <summary>
        /// Gets or sets whether or not the text contained in this control is automatically selected when focus is received.
        /// </summary>
        /// <value>Whether or not the text contained in this control is automatically selected when focus is received.</value>
        /// <returns>Whether or not the text contained in this control is automatically selected when focus is received.</returns>
        public bool SelectAllOnFocus
        {
            get { return (bool)base.GetValue(SelectAllOnFocusProperty); }
            set { base.SetValue(SelectAllOnFocusProperty, value); }
        }
        #endregion

        #region " Method Overrides "

        /// <summary>
        /// Handles TextBox.GotKeyboardFocus event.
        /// </summary>
        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (SelectAllOnFocus)
            {
                // Select all of the text in the control.
                this.SelectAll();

                // Allow the TextBox.GotMouseCapture custom code to execute.
                CancelGotMouseCapture = true;

                // Invoke the OnFocusComplete method.  The method should run as the last step
                // of the textbox focusing logic.
                this.Dispatcher.BeginInvoke(
                    OnFocusCompleteInvoker, System.Windows.Threading.DispatcherPriority.Input, this);
            }
            else
            {
                // Select feature is disabled.  Prevent the TextBox.GotMouseCapture custom code from executing.
                if (CancelGotMouseCapture)
                    CancelGotMouseCapture = false;
            }

            base.OnGotKeyboardFocus(e);
        }

        /// <summary>
        /// Handles TextBox.GotMouseCapture event.
        /// </summary>
        protected override void OnGotMouseCapture(System.Windows.Input.MouseEventArgs e)
        {
            // Check that this code should execute.

            if (CancelGotMouseCapture)
            {
                if (SelectAllOnFocus)
                {
                    // Select all of the text in the control.
                    // It would seem this call is redundant however it is necessary to handle the case where the
                    // user has clicked the mouse outside of the text region and within the textbox (in other words,
                    // the user clicked within the whitespace).
                    this.SelectAll();

                    // Release mouse capture will prevent the textbox from unselecting the text during the TextBox.MouseUp event.
                    this.ReleaseMouseCapture();
                }

                // Prevent this function from executing again (until the next focus event).
                // If this code were allowed to continuously execute, the user would not be able to change the selection.
                CancelGotMouseCapture = false;
            }

            base.OnGotMouseCapture(e);
        }

        /// <summary>
        /// Handles Enter on the TextBox by selecting again the content
        /// </summary>
        protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (SelectAllOnFocus && (e.Key == Key.Enter))
            {
                this.SelectAll();
            }

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Prevents alfanumeric input.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            // TODO: should be extended with a dependency property for applying this text validation
            e.Handled = IsTextNumeric(e.Text);
        }
        
        /// <summary>
        /// Prevent the TextBox.GotMouseCapture custom code from firing until the next focus event.
        /// This method should be invoked after the TextBox has finalized its focus logic.
        /// </summary>
        protected static void OnFocusComplete(TextBoxEx txt)
        {
            if (txt == null)
                return;

            if (txt.CancelGotMouseCapture)
                txt.CancelGotMouseCapture = false;
        }

        private static bool IsTextNumeric(string str)
        {
            var reg = new System.Text.RegularExpressions.Regex("[^0-9]");

            var input = str.Trim(' ');
            return reg.IsMatch(input);
        }

        #endregion
    }
}
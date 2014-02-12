using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DynamicVizSegmenter.CustomControls
{
    /// <summary>
    /// Interaction logic for NumericUpDownTextBox.xaml
    /// </summary>    
    public partial class NumericUpDownTextBox : TextBox
    {
        #region Private Members
        private VisualCollection controls;
        private Button upButton;
        private Button downButton;
        private ButtonsProperties ButtonsViewModel;

        /// <summary>
        /// Set to indicate if the Buttons are displayed.
        /// The buttons are not displayed if the width to height 
        /// ratio of the control is less than a certain value
        /// </summary>
        private bool showButtons = true;

        /// <summary>
        /// Used for repeat button feature
        /// </summary>
        private Timer buttonTimer;
        private Button timerButton;
        #endregion

        public event RoutedEventHandler ValueChanged;

        private void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, new RoutedEventArgs());
        }

        #region Constructor
        /// <summary>
        /// Constructor: initializes the TextBox, creates the buttons,
        /// and attaches event handlers for the buttons and TextBox
        /// </summary>
        public NumericUpDownTextBox()
        {
            InitializeComponent();
            var buttons = new ButtonsProperties(this);
            ButtonsViewModel = buttons;

            // Create buttons
            upButton = new Button()
            {
                Cursor = Cursors.Arrow,
                DataContext = buttons,
                Tag = true
            };
            upButton.Click += Button_Click;
            upButton.PreviewMouseDown += Button_PreviewMouseDown;
            upButton.PreviewMouseUp += Button_PreviewMouseUp;

            downButton = new Button()
            {
                Cursor = Cursors.Arrow,
                DataContext = buttons,
                Tag = false
            };
            downButton.Click += Button_Click;
            downButton.PreviewMouseDown += Button_PreviewMouseDown;
            downButton.PreviewMouseUp += Button_PreviewMouseUp;

            // Create control collections
            controls = new VisualCollection(this);
            controls.Add(upButton);
            controls.Add(downButton);

            //Hook up text event handlers
            this.PreviewTextInput += control_PreviewTextInput;
            this.PreviewKeyDown += control_PreviewKeyDown;
            this.LostFocus += control_LostFocus;
        }
        #endregion

        #region Paint methods
        /// <summary>
        /// Called to arrange and size the content of a 
        ///         System.Windows.Controls.Control object.
        /// </summary>
        /// <param name="arrangeSize">The computed size that is used to 
        ///                 arrange the content</param>
        /// <returns>The size of the control</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double height = arrangeSize.Height;
            double width = arrangeSize.Width;
            showButtons = width > 1.5 * height;

            if (showButtons)
            {
                double buttonWidth = 3 * height / 4;
                Size buttonSize = new Size(buttonWidth, height / 2);
                Size textBoxSize = new Size(width - buttonWidth, height);
                buttonsLeft = width - buttonWidth;
                Rect upButtonRect = new Rect(new
                    Point(buttonsLeft, 0), buttonSize);
                Rect downButtonRect = new Rect(new
                    Point(buttonsLeft, height / 2), buttonSize);
                base.ArrangeOverride(textBoxSize);

                upButton.Arrange(upButtonRect);
                downButton.Arrange(downButtonRect);
                return arrangeSize;
            }
            return base.ArrangeOverride(arrangeSize);
        }

        private double buttonsLeft;

        /// <summary>
        /// Overrides System.Windows.Media.Visual.GetVisualChild(System.Int32), and returns
        //     a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>The requested child element.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index < base.VisualChildrenCount)
                return base.GetVisualChild(index);
            return controls[index - base.VisualChildrenCount];
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                if (showButtons)
                    return controls.Count + base.VisualChildrenCount;
                return base.VisualChildrenCount;
            }
        }
        #endregion

        #region Keypress and button click Event Handlers
        /// <summary>
        /// Handles up button click (ie increment TextBox value)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Add(sender == upButton ? 1 : -1);
        }

        private void ButtonTimerCallback(Object sender)
        {
            Add(sender == upButton ? 1 : -1);
        }

        /// <summary>
        /// Ensures that keypress is a valid character (numeric or negative sign)
        /// - negative sign ('-') only allowed if Minimum value less than 0, the entry 
        ///     is at the beginning of the text and there is not already a negative sign
        /// - number only allowed if not beginning of text and there is a negative sign
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Contains arguments associated with changes to
        ///                          a System.Windows.Input.TextComposition</param>
        private void control_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Catch any non-numeric keys
            if ("0123456789".IndexOf(e.Text) < 0)
            {
                // else check for negative sign
                if (e.Text == "-" && MinValue < 0)
                {
                    if (this.Text.Length == 0 || (this.CaretIndex == 0 &&
                                    this.Text[0] != '-'))
                    {
                        e.Handled = false;
                        return;
                    }
                }
                e.Handled = true;
            }
            else // A digit has been pressed
            {
                // We now know that have good value: check for attempting to put number before '-'
                if (this.Text.Length > 0 && this.CaretIndex == 0 &&
                    this.Text[0] == '-' && this.SelectionLength == 0)
                {
                    // Can't put number before '-'
                    e.Handled = true;
                }
                else
                {
                    // check for what new value will be:
                    StringBuilder sb = new StringBuilder(this.Text);
                    sb.Remove(this.CaretIndex, this.SelectionLength);
                    sb.Insert(this.CaretIndex, e.Text);
                    int newValue = int.Parse(sb.ToString());
                    // check if beyond allowed values
                    e.Handled = !FixValueKeyPress(newValue);
                }
            }
        }

        /// <summary>
        /// Checks if the keypress is the up or down key, and then handles keyboard input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void control_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                HandleModifiers(-1);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                HandleModifiers(1);
                e.Handled = true;
            }
            // Space key is not caught by PreviewTextInput
            else if (e.Key == Key.Space)
                e.Handled = true;
            else
                e.Handled = false;
        }

        private void control_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Text)) this.Text = "0";
            FixValue();
        }
        #endregion

        #region private methods
        /// <summary>
        /// Checks if any of the Keyboard modifier keys are pressed that might 
        /// affect the change in the value of the TextBox.
        /// In this case only the shift key affects the value
        /// </summary>
        /// <param name="value">Integer value to modify</param>
        private void HandleModifiers(int value)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift) value *= 10;
            Add(value);
        }

        /// <summary>
        /// Works directly with the Value property to increment or decrement the 
        /// integer value in the textbox
        /// </summary>
        /// <param name="value"></param>
        private void Add(int value)
        {
            int? v = this.Value;
            if (v == null)
            {
                if (this.MaxValue < 0) v = this.MaxValue;
                else if (this.MinValue > 0) v = this.MinValue;
                else v = 0;
            }
            v += value;
            if (v > MaxValue) v = this.MaxValue;
            else if (v < MinValue) v = this.MinValue;
            this.Value = v;
        }

        /// <summary>
        /// Only does something if the Textbox contains an out of range number
        /// </summary>
        private int FixValue()
        {
            int value;
            if (int.TryParse(this.Text, out value))
            {
                if (value > MaxValue)
                {
                    UpdateText(MaxValue);
                    value = MaxValue;
                }
                else if (value < MinValue)
                {
                    UpdateText(MinValue);
                    value = MinValue;
                }
                else
                    this.Value = value;
            }
            return value;
        }

        /// <summary>
        /// Will fix value only if the value is beyond what could be
        /// a valid entry given more characters could be added to TextBox
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool FixValueKeyPress(int value)
        {
            //Want to allow user to build a number with keystrokes
            if (value > MaxValue && MaxValue > 0)
            {
                UpdateText(MaxValue);
                return false;
            }
            if (value < MinValue && MinValue < 0)
            {
                UpdateText(MinValue);
                return false;
            }
            // else: nothing to fix
            return true;
        }

        private void UpdateText(int value)
        {
            this.Text = value.ToString();
            this.CaretIndex = this.Text.Length;
            Value = value;
        }
        #endregion

        #region Value Dependency Properties
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(int.MaxValue, new PropertyChangedCallback(MinMaxValueChangedCallback)));

        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(0, new PropertyChangedCallback(MinMaxValueChangedCallback)));

        /// <summary>
        /// TextBox Text value converted to an integer
        /// </summary>
        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                OnValueChanged();
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, new PropertyChangedCallback(ValueChangedCallback)));

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericUpDownTextBox;
            control.Text = e.NewValue.ToString();
        }

        private static void MinMaxValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericUpDownTextBox;
            control.FixValue();
        }
        #endregion

        #region Button dependency properties
        /// <summary>
        /// Button Background Brush
        /// </summary>
        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, ButtonPropertyChangedCallback));

        public Brush ButtonForeground
        {
            get { return (Brush)GetValue(ButtonForegroundProperty); }
            set { SetValue(ButtonForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonForegroundProperty =
            DependencyProperty.Register("ButtonForeground", typeof(Brush), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, ButtonPropertyChangedCallback));

        /// <summary>
        /// Button Background Brush when button IsPressed
        /// </summary>
        public Brush ButtonPressedBackground
        {
            get { return (Brush)GetValue(ButtonPressedBackgroundProperty); }
            set { SetValue(ButtonPressedBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonPressedBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonPressedBackgroundProperty =
            DependencyProperty.Register("ButtonPressedBackground", typeof(Brush), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, ButtonPropertyChangedCallback));

        /// <summary>
        /// Button Background when mouse is over button
        /// </summary>
        public Brush ButtonMouseOverBackground
        {
            get { return (Brush)GetValue(ButtonMouseOverBackgroundProperty); }
            set { SetValue(ButtonMouseOverBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonMouseOverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonMouseOverBackgroundProperty =
            DependencyProperty.Register("ButtonMouseOverBackground", typeof(Brush), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, ButtonPropertyChangedCallback));

        /// <summary>
        /// Button Border Brush
        /// </summary>
        public Brush ButtonBorderBrush
        {
            get { return (Brush)GetValue(ButtonBorderBrushProperty); }
            set { SetValue(ButtonBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonBorderBrushProperty =
            DependencyProperty.Register("ButtonBorderBrush", typeof(Brush), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, ButtonPropertyChangedCallback));

        /// <summary>
        /// Button Border Thickness
        /// </summary>
        public Thickness? ButtonBorderThickness
        {
            get { return (Thickness?)GetValue(ButtonBorderThicknessProperty); }
            set { SetValue(ButtonBorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonBorderThicknessProperty =
            DependencyProperty.Register("ButtonBorderThickness", typeof(Thickness?), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, ButtonPropertyChangedCallback));

        /// <summary>
        /// Button Border Thickness
        /// </summary>
        public CornerRadius? ButtonCornerRadius
        {
            get { return (CornerRadius?)GetValue(ButtonCornerRadiusProperty); }
            set { SetValue(ButtonCornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonCornerRadiusProperty =
            DependencyProperty.Register("ButtonCornerRadiusThickness", typeof(CornerRadius?), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(null, ButtonPropertyChangedCallback));

        private static void ButtonPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDownTextBox target = (NumericUpDownTextBox)d;
            target.ButtonsViewModel.NotifyPropertyChanged(e.Property.ToString());
        }
        #endregion

        #region RepeatButton functionality
        #region repeat button press events
        /// <summary>
        /// When mouse button is depressed over the up or down button, initiates the 
        /// RepeatButton behaviour by setting up a timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            timerButton = sender as Button;
            buttonTimer = new Timer(new TimerCallback(RepeatButtonCallback), null, Delay, Interval);
        }

        /// <summary>
        /// Resets the repeat button so no longer active when Mouse button released
        ///     Does not matter if mouse is on button or not
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (buttonTimer == null)
                return;
            buttonTimer.Dispose();
            buttonTimer = null;
        }

        /// <summary>
        /// Observes the button timer event for the repeat button functionality, 
        /// and then starts the 'Value' dependency property change process
        /// Note: Cannot interact directly with UI from this event
        /// </summary>
        /// <param name="o"></param>
        private void RepeatButtonCallback(object o)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal,
                         new Action<int>(RepeatButtonDispatched),
                         timerButton == upButton ? 1 : -1);
        }

        /// <summary>
        /// Checks that mouse cursor is in the button, and if so updates the 'Value' dependency property
        /// </summary>
        /// <param name="value">amount to change the value by</param>
        private void RepeatButtonDispatched(int value)
        {
            var pos = Mouse.GetPosition(timerButton);
            if (pos.X >= 0 && pos.Y >= 0 && pos.X <= timerButton.ActualWidth && pos.Y <= timerButton.ActualHeight)
                Add(value);
        }
        #endregion

        #region Timer Dependency properties
        public int Delay
        {
            get { return (int)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Delay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DelayProperty =
            DependencyProperty.Register("Delay", typeof(int), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(750));

        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Interval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(NumericUpDownTextBox),
            new UIPropertyMetadata(100));
        #endregion
        #endregion
    }

    /// <summary>
    /// Returns the point collection for the polygon that is
    /// the arrow pointing either up or down in the center of the 
    /// up and down buttons.
    /// </summary>
    class ArrowCreater : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            double width = (double)values[0];
            double height = (double)values[1];
            if ((height == 0.0) || (width == 0.0)) return null;
            Thickness borderThickness = (Thickness)values[2];
            bool up = (bool)values[3];
            double arrowHeight = height - borderThickness.Top -
                borderThickness.Bottom;
            double arrowWidth = width - borderThickness.Left -
                borderThickness.Right;
            return CreateArrow(arrowWidth, arrowHeight, up);
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private PointCollection CreateArrow(double width, double height, bool isUp)
        {
            double margin = height * .2;
            double pointY;
            double baseY;

            if (isUp)
            {
                pointY = margin;
                baseY = height - margin;
            }
            else
            {
                baseY = margin;
                pointY = height - margin;
            }
            var pts = new PointCollection();
            pts.Add(new Point(margin, baseY));
            pts.Add(new Point(width / 2, pointY));
            pts.Add(new Point(width - margin, baseY));
            return pts;
        }
    }

    /// <summary>
    /// Properties for button attributes
    /// </summary>
    public class ButtonsProperties : INotifyPropertyChanged
    {
        private Brush isPressedDefault = new SolidColorBrush(Colors.LightBlue);
        private Brush isMouseOverDefault = new SolidColorBrush(Colors.LightSteelBlue);
        private CornerRadius cornerRadiusDefault = new CornerRadius(3);

        #region constructor
        public ButtonsProperties(NumericUpDownTextBox textBox)
        {
            parent = textBox;
        }
        #endregion

        private NumericUpDownTextBox parent { get; set; }

        #region Button properties
        public Brush Background
        {
            get { return parent.ButtonBackground ?? parent.Background; }
        }

        public Brush Foreground
        {
            get { return parent.ButtonForeground ?? parent.Foreground; }
        }

        public Brush BorderBrush
        {
            get { return parent.ButtonBorderBrush ?? parent.BorderBrush; }
        }

        public Thickness BorderThickness
        {
            get { return (Thickness)(parent.ButtonBorderThickness ?? parent.BorderThickness); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)(parent.ButtonCornerRadius ?? cornerRadiusDefault); }
        }

        public Brush IsPressedBackground
        {
            get { return parent.ButtonPressedBackground ?? isPressedDefault; }
        }

        public Brush IsMouseOverBackground
        {
            get { return parent.ButtonMouseOverBackground ?? isMouseOverDefault; }
        }
        #endregion

        #region INotifyPropertyChanged
        internal void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
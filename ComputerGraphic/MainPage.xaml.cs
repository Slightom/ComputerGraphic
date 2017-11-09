using ComputerGraphic.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ComputerGraphic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Point startPointOfDragging;
        private bool pointerMovedHandlerIsAdded;
        private bool pointerPressedHandlerIsAdded;
        private bool pointerReleasedHandlerIsAdded;
        private bool pointerShapePressed;
        private string selectedOption;
        private bool pointerIsPressed;
        private Shape draggingElement;
        private Shape selectedElement;
        private Image draggingImage;
        private Image selectedImage;


        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            pointerMovedHandlerIsAdded = false;
            pointerIsPressed = false;
            PersonalizeTitleBar();

            InitColorPicker();
            //BitmapImage b = new BitmapImage(new Uri("C:\\Users\\Slightom\\Downloads\\ppm-obrazy-testowe\\ppm-test-01-p3.ppm"));
            //myBitMap.Source = b;
        }


        #region PAINT

        #region dragging
        private void Shape_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.Caption = "drag and drop"; // Sets custom UI text
            e.DragUIOverride.IsCaptionVisible = false; // Sets if the caption is visible
            e.DragUIOverride.IsContentVisible = true; // Sets if the dragged content is visible
            e.DragUIOverride.IsGlyphVisible = false; // Sets if the glyph is visibile
        }

        private async void Shape_Drop(object sender, DragEventArgs e)
        {
            var point = e.GetPosition(MyCanvas);
            point.X = Math.Round(point.X, 0);
            point.Y = Math.Round(point.Y, 0);

            var droppingShape = MyCanvas.Children.OfType<Shape>().Where(x => x.Name == draggingElement.Name).FirstOrDefault();
            var leftPoistionOnCanvas = point.X - startPointOfDragging.X;
            var topPoistionOnCanvas = point.Y - startPointOfDragging.Y;

            if (droppingShape != null)
            {
                if (droppingShape is Line)
                {
                    var line = droppingShape as Line;
                    topPoistionOnCanvas = (line.Y1 < line.Y2) ? topPoistionOnCanvas : topPoistionOnCanvas + 2 * startPointOfDragging.Y;
                    var oldLeftOnCanvas = line.X1;
                    var oldTopOnCanvas = line.Y1;
                    if (leftPoistionOnCanvas > oldLeftOnCanvas)
                    {
                        line.X1 += Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                        line.X2 += Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                    }
                    else
                    {
                        line.X1 -= Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                        line.X2 -= Modul(oldLeftOnCanvas - leftPoistionOnCanvas);
                    }
                    if (topPoistionOnCanvas > oldTopOnCanvas)
                    {
                        line.Y1 += Modul(oldTopOnCanvas - topPoistionOnCanvas);
                        line.Y2 += Modul(oldTopOnCanvas - topPoistionOnCanvas);
                    }
                    else
                    {
                        line.Y1 -= Modul(oldTopOnCanvas - topPoistionOnCanvas);
                        line.Y2 -= Modul(oldTopOnCanvas - topPoistionOnCanvas);
                    }
                }
                else
                {
                    Canvas.SetLeft(MyCanvas.Children.OfType<Shape>().Where(x => x.Name == draggingElement.Name).FirstOrDefault(), leftPoistionOnCanvas);
                    Canvas.SetTop(MyCanvas.Children.OfType<Shape>().Where(x => x.Name == draggingElement.Name).FirstOrDefault(), topPoistionOnCanvas);
                }

                droppingShape.Opacity = 1.0;
            }
            else
            {
                var droppingImage = MyCanvas.Children.OfType<Image>().Where(x => x.Name == draggingImage.Name).FirstOrDefault();

                Canvas.SetLeft(MyCanvas.Children.OfType<Image>().Where(x => x.Name == draggingImage.Name).FirstOrDefault(), leftPoistionOnCanvas);
                Canvas.SetTop(MyCanvas.Children.OfType<Image>().Where(x => x.Name == draggingImage.Name).FirstOrDefault(), topPoistionOnCanvas);

                droppingImage.Opacity = 1.0;
            }

            draggingElement = null;
            draggingImage = null;

            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Shape_DragStarting(UIElement sender, DragStartingEventArgs e)
        {
            draggingElement = sender as Shape;
            draggingImage = sender as Image;
            startPointOfDragging = e.GetPosition(MyCanvas);
            var difX = (sender is Line) ? Modul(startPointOfDragging.X - (sender as Line).X1) : Modul(startPointOfDragging.X - Canvas.GetLeft(sender));
            var difY = (sender is Line) ? Modul(startPointOfDragging.Y - (sender as Line).Y1) : Modul(startPointOfDragging.Y - Canvas.GetTop(sender));
            startPointOfDragging.X = Math.Round(difX, 0);
            startPointOfDragging.Y = Math.Round(difY, 0);

            e.Data.RequestedOperation = DataPackageOperation.Move;
            sender.Opacity = 0.2;

            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }

        #endregion

        private void ConfirmDimensions_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOption != "CursorListBoxItem")
            {
                switch (selectedOption)
                {
                    case "LineListBoxItem":
                        Line l2 = new Line();
                        l2.Name = "l" + MyCanvas.Children.Count();
                        l2.Stroke = new SolidColorBrush(color);
                        l2.X1 = SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text));
                        l2.X2 = BiggerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text));
                        l2.Y1 = (double.Parse(X1TextBox.Text) < double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text);
                        l2.Y2 = (double.Parse(X1TextBox.Text) > double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text);
                        l2.StrokeThickness = 2;
                        l2.DragStarting += Shape_DragStarting;
                        MyCanvas.Children.Add(l2);
                        Canvas.SetLeft(l2, 0);
                        Canvas.SetTop(l2, 0);
                        break;
                    case "RectangleListBoxItem":
                        Rectangle r = new Rectangle();
                        r.Name = "r" + MyCanvas.Children.Count();
                        r.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                        r.Height = Modul(double.Parse(Y1TextBox.Text) - double.Parse(Y2TextBox.Text));
                        r.Stroke = new SolidColorBrush(color);
                        r.StrokeThickness = 3;
                        r.DragStarting += Shape_DragStarting;
                        MyCanvas.Children.Add(r);
                        Canvas.SetLeft(r, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                        Canvas.SetTop(r, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                        break;
                    case "CircleListBoxItem":
                        Ellipse el = new Ellipse();
                        el.Name = "e" + MyCanvas.Children.Count();
                        el.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                        el.Height = el.Width;
                        el.StrokeThickness = 3;
                        el.Stroke = new SolidColorBrush(color);
                        el.DragStarting += Shape_DragStarting;
                        MyCanvas.Children.Add(el);
                        Canvas.SetLeft(el, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                        Canvas.SetTop(el, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                        break;
                }
            }
            else
            {
                if (MyCanvas.Children.OfType<Line>().Where(x => x.Name == selectedElement.Name).Count() > 0)
                {
                    var qw = MyCanvas.Children.OfType<Line>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                    qw.X1 = Math.Round(double.Parse(X1TextBox.Text), 0);
                    qw.Y1 = Math.Round(double.Parse(Y1TextBox.Text), 0);
                    qw.X2 = Math.Round(double.Parse(X2TextBox.Text), 0);
                    qw.Y2 = Math.Round(double.Parse(Y2TextBox.Text), 0);
                }
                else if (MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == selectedElement.Name).Count() > 0)
                {
                    var qw = MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                    qw.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                    qw.Height = Modul(double.Parse(Y1TextBox.Text) - double.Parse(Y2TextBox.Text));

                    Canvas.SetLeft(qw, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                    Canvas.SetTop(qw, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                }
                else
                {
                    var qw = MyCanvas.Children.OfType<Ellipse>().Where(x => x.Name == selectedElement.Name).FirstOrDefault();
                    qw.Width = Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text));
                    qw.Height = qw.Width;

                    Canvas.SetLeft(qw, SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)));
                    Canvas.SetTop(qw, HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)));
                }
            }
        }


        private void SelectMenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.selectedOption = SelectShapeListBox.Items.Cast<ListBoxItem>().Where(p => p.IsSelected).FirstOrDefault().Name;
            switch (selectedOption)
            {
                case "CursorListBoxItem":
                    if (MyCanvas != null)
                    {
                        SetPointerMovedEvent(false);
                        SetPointerPressed(false);
                        SetPointerReleased(false);
                        SetPointerEnteredEventForAllObjects(true);
                        SetCanDragForAllObject(true);
                        SetShapePointerPressed(true);
                        //SetShapePointerPressedForAllObjects(true);
                    }

                    break;
                case "LineListBoxItem":
                    SetPointerMovedEvent(true);
                    SetPointerPressed(true);
                    SetPointerReleased(true);
                    SetPointerEnteredEventForAllObjects(false);
                    SetCanDragForAllObject(false);
                    SetShapePointerPressed(false);
                    //SetShapePointerPressedForAllObjects(false);
                    break;
                case "RectangleListBoxItem":
                    SetPointerMovedEvent(true);
                    SetPointerPressed(true);
                    SetPointerReleased(true);
                    SetPointerEnteredEventForAllObjects(false);
                    SetCanDragForAllObject(false);
                    SetShapePointerPressed(false);
                    //SetShapePointerPressedForAllObjects(false);
                    break;
                case "CircleListBoxItem":
                    SetPointerMovedEvent(true);
                    SetPointerPressed(true);
                    SetPointerReleased(true);
                    SetPointerEnteredEventForAllObjects(false);
                    SetCanDragForAllObject(false);
                    SetShapePointerPressed(false);
                    //SetShapePointerPressedForAllObjects(false);
                    break;
            }
        }



        #region setting pointer handlers
        private void SetPointerReleased(bool add)
        {
            if (add)
            {
                if (!pointerReleasedHandlerIsAdded)
                {
                    MyCanvas.PointerReleased += MyCanvas_PointerReleased;
                    pointerReleasedHandlerIsAdded = true;
                }
            }
            else if (pointerReleasedHandlerIsAdded)
            {
                MyCanvas.PointerReleased -= MyCanvas_PointerReleased;
                pointerReleasedHandlerIsAdded = false;
            }
        }

        private void SetPointerPressed(bool add)
        {
            if (add)
            {
                if (!pointerPressedHandlerIsAdded)
                {
                    MyCanvas.PointerPressed += MyCanvas_PointerPressed;
                    pointerPressedHandlerIsAdded = true;
                }
            }
            else if (pointerPressedHandlerIsAdded)
            {
                MyCanvas.PointerPressed -= MyCanvas_PointerPressed;
                pointerPressedHandlerIsAdded = false;
            }
        }

        private void SetPointerMovedEvent(bool add)
        {
            if (add)
            {
                if (!pointerMovedHandlerIsAdded)
                {
                    MyCanvas.PointerMoved += MyCanvas_PointerMoved;
                    pointerMovedHandlerIsAdded = true;
                }
            }
            else if (pointerMovedHandlerIsAdded)
            {
                MyCanvas.PointerMoved -= MyCanvas_PointerMoved;
                pointerMovedHandlerIsAdded = false;
            }
        }

        private void SetPointerEnteredEventForAllObjects(bool v)
        {
            if (v)
            {
                foreach (var c in MyCanvas.Children.OfType<Shape>())
                {
                    if (!c.Name.Contains("pointerEntered"))
                    {
                        c.PointerEntered += Shape_PointerEntered;
                        c.PointerExited += Shape_PointerExited;
                        c.Name += "pointerEntered";
                    }
                }
            }
            else
            {
                foreach (var c in MyCanvas.Children.OfType<Shape>())
                {
                    if (c.Name.Contains("pointerEntered"))
                    {
                        c.PointerEntered -= Shape_PointerEntered;
                        c.PointerExited -= Shape_PointerExited;
                        c.Name = c.Name.Replace("pointerEntered", "");
                    }
                }
            }
        }

        private void SetShapePointerPressed(bool add)
        {
            if (add)
            {
                if (!pointerShapePressed)
                {
                    MyCanvas.PointerPressed += Shape_PointerPressed;
                    pointerShapePressed = true;
                }
            }
            else if (pointerShapePressed)
            {
                MyCanvas.PointerPressed -= Shape_PointerPressed;
                pointerShapePressed = false;
            }
        }

        //private void SetShapePointerPressedForAllObjects(bool v)
        //{
        //    if (v)
        //    {
        //        foreach (var c in MyCanvas.Children.OfType<Shape>())
        //        {
        //            if (!c.Name.Contains("shapePointerPressed"))
        //            {
        //                c.PointerPressed += Shape_PointerPressed;
        //                c.Name += "shapePointerPressed";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var c in MyCanvas.Children.OfType<Shape>())
        //        {
        //            if (c.Name.Contains("shapePointerPressed"))
        //            {
        //                c.PointerPressed -= Shape_PointerPressed;
        //                c.Name = c.Name.Replace("shapePointerPressed", "");
        //            }
        //        }
        //    }
        //}
        #endregion

        #region pointer handlers
        private void MyCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!pointerIsPressed)
            {
                X1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
                Y1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();
            }
            else
            {
                X2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
                Y2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();
            }

        }

        private void MyCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pointerIsPressed = true;
            X1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
            Y1TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();
        }

        private void MyCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            pointerIsPressed = false;
            X2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.X, 0).ToString();
            Y2TextBox.Text = Math.Round(e.GetCurrentPoint(MyCanvas).Position.Y, 0).ToString();

            switch (selectedOption)
            {
                case "LineListBoxItem":
                    Line l2 = new Line();
                    l2.Name = "l" + MyCanvas.Children.Count();
                    l2.Stroke = new SolidColorBrush(color);
                    l2.X1 = Math.Round(SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0);
                    l2.Y1 = Math.Round((double.Parse(X1TextBox.Text) < double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text), 0);
                    l2.X2 = Math.Round(BiggerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0);
                    l2.Y2 = Math.Round((double.Parse(X1TextBox.Text) > double.Parse(X2TextBox.Text)) ? double.Parse(Y1TextBox.Text) : double.Parse(Y2TextBox.Text), 0);
                    l2.StrokeThickness = 2;
                    l2.DragStarting += Shape_DragStarting;
                    MyCanvas.Children.Add(l2);

                    Canvas.SetLeft(l2, 0);
                    Canvas.SetTop(l2, 0);
                    break;

                case "RectangleListBoxItem":
                    Rectangle r = new Rectangle();
                    r.Name = "r" + MyCanvas.Children.Count();
                    r.Width = Math.Round(Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text)), 0);
                    r.Height = Math.Round(Modul(double.Parse(Y1TextBox.Text) - double.Parse(Y2TextBox.Text)), 0);
                    r.Stroke = new SolidColorBrush(color);
                    r.StrokeThickness = 3;
                    r.DragStarting += Shape_DragStarting;
                    MyCanvas.Children.Add(r);
                    Canvas.SetLeft(r, Math.Round(SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0));
                    Canvas.SetTop(r, Math.Round(HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)), 0));
                    break;

                case "CircleListBoxItem":
                    Ellipse el = new Ellipse();
                    el.Name = "e" + MyCanvas.Children.Count();
                    el.Width = Math.Round(Modul(double.Parse(X1TextBox.Text) - double.Parse(X2TextBox.Text)), 0);
                    el.Height = el.Width;
                    el.StrokeThickness = 3;
                    el.Stroke = new SolidColorBrush(color);
                    el.DragStarting += Shape_DragStarting;
                    MyCanvas.Children.Add(el);
                    Canvas.SetLeft(el, Math.Round(SmallerFrom(double.Parse(X1TextBox.Text), double.Parse(X2TextBox.Text)), 0));
                    Canvas.SetTop(el, Math.Round(HigherFrom(double.Parse(Y1TextBox.Text), double.Parse(Y2TextBox.Text)), 0));
                    break;
            }

        }

        private void Shape_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 0);
        }

        private void Shape_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Shape_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            selectedElement = null;
            selectedImage = null;
            if (sender is Line) { selectedElement = MyCanvas.Children.OfType<Line>().Where(x => x.Name == (sender as Line).Name).FirstOrDefault(); }
            else if (sender is Rectangle) { selectedElement = MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == (sender as Rectangle).Name).FirstOrDefault(); }
            else if (sender is Ellipse) { selectedElement = MyCanvas.Children.OfType<Ellipse>().Where(x => x.Name == (sender as Ellipse).Name).FirstOrDefault(); }
            else { selectedImage = sender as Image; }

            if (selectedImage != null)
            {
                X1TextBox.Text = Canvas.GetLeft(selectedImage).ToString();
                X2TextBox.Text = (Canvas.GetLeft(selectedImage) + selectedImage.Width).ToString();
                Y1TextBox.Text = Canvas.GetTop(selectedImage).ToString();
                Y2TextBox.Text = (Canvas.GetTop(selectedImage) + selectedImage.Height).ToString();

                setImageResources();
            }
            else
            {
                if (selectedElement is Line)
                {
                    selectedElement = MyCanvas.Children.OfType<Line>().Where(x => x.Name == (selectedElement as Line).Name).FirstOrDefault();
                    var line = (selectedElement as Line);
                    X1TextBox.Text = line.X1.ToString();
                    X2TextBox.Text = line.X2.ToString();
                    Y1TextBox.Text = line.Y1.ToString();
                    Y2TextBox.Text = line.Y2.ToString();

                }
                else if (selectedElement is Rectangle)
                {
                    selectedElement = MyCanvas.Children.OfType<Rectangle>().Where(x => x.Name == (selectedElement as Rectangle).Name).FirstOrDefault();
                    X1TextBox.Text = Canvas.GetLeft(selectedElement).ToString();
                    X2TextBox.Text = (Canvas.GetLeft(selectedElement) + selectedElement.Width).ToString();
                    Y1TextBox.Text = Canvas.GetTop(selectedElement).ToString();
                    Y2TextBox.Text = (Canvas.GetTop(selectedElement) + selectedElement.Height).ToString();
                }
                else
                {
                    selectedElement = MyCanvas.Children.OfType<Ellipse>().Where(x => x.Name == (selectedElement as Ellipse).Name).FirstOrDefault();
                    X1TextBox.Text = Canvas.GetLeft(selectedElement).ToString();
                    X2TextBox.Text = (Canvas.GetLeft(selectedElement) + selectedElement.Width).ToString();
                    Y1TextBox.Text = Canvas.GetTop(selectedElement).ToString();
                    Y2TextBox.Text = (Canvas.GetTop(selectedElement) + selectedElement.Height).ToString();
                }
            }

        }
        #endregion

        #region helpful functions 

        private double HigherFrom(double v1, double v2)
        {
            if (v1 < v2) { return v1; }
            else { return v2; }
        }

        private double SmallerFrom(double v1, double v2)
        {
            if (v1 < v2) { return v1; }
            else { return v2; }
        }

        private double LowerFrom(double v1, double v2)
        {
            if (v1 > v2) { return v1; }
            else { return v2; }
        }

        private double BiggerFrom(double v1, double v2)
        {
            if (v1 > v2) { return v1; }
            else { return v2; }
        }

        private double Modul(double v)
        {
            if (v < 0) { return v * (-1); }
            else { return v; }
        }

        private void SetCanDragForAllObject(bool v)
        {
            foreach (var c in MyCanvas.Children) { c.CanDrag = v; }
        }

        #endregion

        #endregion



        #region PPM Files

        private async void LoadFileClickAsync(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".ppm");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                if (file.FileType == ".jpg" || file.FileType == ".jpeg")
                {
                    await ReadJPG(file);
                }
                else if (file.FileType == ".ppm")
                {
                    //Read ppm
                    await ReadPPM(file);
                }
                else
                {
                    throw new Exception("Invalid tye of file");
                }
            }
        }

        private async Task ReadJPG(StorageFile file)
        {
            var bitmap = new BitmapImage();
            FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
            bitmap.SetSource(stream);
            Image Image = new Image();
            Image.Source = bitmap;
            Image.DragStarting += Shape_DragStarting;
            Image.CanDrag = true;
            Image.Name = "i" + MyCanvas.Children.Count().ToString();
            MyCanvas.Children.Add(Image);
            Image.PointerPressed += Shape_PointerPressed;
            selectedImage = Image;
            setImageResources();
        }

        private async Task ReadPPM(StorageFile file)
        {
            FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
            BinaryReader reader = new BinaryReader(stream.AsStream());

            var header = reader.ReadBytes(2);
            if (header[0] == 'P' && header[1] == '3')
            {
                await ReadP3(stream);
            }
            else if (header[0] == 'P' && header[1] == '6')
            {
                await ReadP6(reader);
            }
            else
            {
                //invalid format
            }
        }


        private async Task ReadP3(FileRandomAccessStream stream)
        {
            var reader = new StreamReader(stream.AsStream());
            int width = -1, height = -1, max = -1;

            ReadParameters(reader, ref width, ref height, ref max);

            var numbers = ReadAllNumbers(reader, width, height);

            WriteableBitmap bitmap = new WriteableBitmap(width, height);
            var array = ReadP3(width, height, max, numbers);
            using (Stream bitmapStream = bitmap.PixelBuffer.AsStream())
            {
                await bitmapStream.WriteAsync(array, 0, array.Length);
                //await bitmapStream.WriteAsync()
            }

            Image Image = new Image();
            //Image.Width = width;
            //Image.Height = height;
            Image.Source = bitmap;
            Image.DragStarting += Shape_DragStarting;
            Image.CanDrag = true;
            Image.Name = "i" + MyCanvas.Children.Count().ToString();
            MyCanvas.Children.Add(Image);
            Image.PointerPressed += Shape_PointerPressed;
            selectedImage = Image;
            setImageResources();
        }

        private byte[] ReadP3(int width, int height, int max, int[] numbers)
        {
            byte[] array = new byte[height * width * 4];
            try
            {
                for (int i = 0, j = 0; i < array.Length; i += 4, j += 3)
                {
                    array[i] = (byte)(numbers[j + 2] * 255 / max);
                    array[i + 1] = (byte)((numbers[j + 1]) * 255 / max);
                    array[i + 2] = (byte)((numbers[j]) * 255 / max);
                    array[i + 3] = (byte)255;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return array;
        }

        private int[] ReadAllNumbers(StreamReader reader, int width, int height)
        {
            string line;
            string[] words;
            int number;
            int counter = 0;
            int[] numbers = new int[width * height * 3];
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                line = line.Replace("\t", " ");
                words = line.Split(' ');
                foreach (var word in words)
                {
                    if (word.Contains("#"))
                    {
                        break;
                    }
                    if (word == " " || word == "" || word == "\0" || word == "\n")
                    {
                        continue;
                    }
                    try
                    {
                        number = Int32.Parse(word);
                        numbers[counter] = number;
                        counter++;
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }

            return numbers;
        }

        private void ReadParameters(StreamReader reader, ref int width, ref int height, ref int max)
        {
            reader.ReadLine();
            string line;
            int number;

            while (max == -1)
            {
                line = reader.ReadLine();
                line = line.Replace("\t", " ");
                var words = line.Split(' ');
                foreach (var word in words)
                {
                    if (word.Contains("#")) { break; }
                    if (word.Contains(" ") || word == "" || word.Contains("\t") || word.Contains("\0") || word.Contains("\n")) { continue; }

                    try
                    {
                        number = Int32.Parse(word);
                        if (width == -1)
                        {
                            width = number;
                        }
                        else if (height == -1)
                        {
                            height = number;
                        }
                        else if (max == -1)
                        {
                            max = number;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }



        private async Task ReadP6(BinaryReader reader)
        {
            ReadWhitespace(reader);
            int width = ReadValue(reader);
            ReadWhitespace(reader);
            int height = ReadValue(reader);
            ReadWhitespace(reader);
            int max = ReadValue(reader);

            WriteableBitmap bitmap = new WriteableBitmap(width, height);
            byte[] array = new byte[5];
            if (max > 255)
            {
                // array = Read16bit(reader, width, height, max);
            }
            else
            {
                array = Read8bit(reader, width, height, max);
            }
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(array, 0, array.Length);
            }
            Image Image = new Image();
            Image.Source = bitmap;
            Image.DragStarting += Shape_DragStarting;
            Image.CanDrag = true;
            Image.Name = "i" + MyCanvas.Children.Count().ToString();
            MyCanvas.Children.Add(Image);
            Image.PointerPressed += Shape_PointerPressed;
            selectedImage = Image;
            setImageResources();
        }

        private byte[] Read8bit(BinaryReader reader, int width, int height, int max)
        {
            byte[] array = new byte[height * width * 4];
            try
            {
                for (int i = 0; i < array.Length; i += 4)
                {
                    byte[] rgb = reader.ReadBytes(3);
                    array[i] = (byte)((int)rgb[2] * 255 / max);
                    array[i + 1] = (byte)((int)rgb[1] * 255 / max);
                    array[i + 2] = (byte)((int)rgb[0] * 255 / max);
                    array[i + 3] = (byte)255;
                }
            }
            catch (Exception e)
            {
                throw new Exception();
            }

            return array;
        }

        private int ReadValue(BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char c = reader.ReadChar();

            while (c != ' ' && c != '\t' && c != '\n' && c != '\0')
            {
                builder.Append(c);
                c = reader.ReadChar();
            }

            return int.Parse(builder.ToString());
        }

        private void ReadWhitespace(BinaryReader reader)
        {
            char c = reader.ReadChar();

            while (c == ' ' || c == '\t' || c == '\n' || c == '\0' || c == '#')
            {

                // When we encounter a comment, read until the end of it (has to be a newline)
                if (c == '#')
                {
                    reader.ReadChar();
                    while (c != '\n')
                    {
                        try
                        {
                            c = reader.ReadChar();
                        }
                        catch (Exception e)
                        {
                            reader.BaseStream.Seek(1, SeekOrigin.Current);
                        }
                    }
                }
                c = reader.ReadChar();
            }

            // When we encounter a non-whitespace character again we have to go back one byte, so it can be read by other functions
            reader.BaseStream.Seek(-1, SeekOrigin.Current);
        }



        #endregion



        #region ColorPicker

        private Brush brush;
        private Color color;
        public Color colorLighter;
        private bool CMYKSlidersChangingColorFlag = false;
        private bool RGBSlidersChangingColorFlag = false;
        private bool ColorPickerChangingColorFlag = false;

        private void InitColorPicker()
        {
            color = Colors.White;
            this.colorLighter = setLighterColor(color);
            brush = new SolidColorBrush(color);
            ColorButton.Background = brush;
            ColorPicker.Color = color;

            RSlider.Value = GSlider.Value = BSlider.Value = 255;
            RTextBox.Text = GTextBox.Text = BTextBox.Text = 255.ToString();
            KSlider.Value = CSlider.Value = MSlider.Value = YSlider.Value = 0;
            KTextBox.Text = CTextBox.Text = MTextBox.Text = YTextBox.Text = 0.ToString();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerPopup.IsOpen = !ColorPickerPopup.IsOpen;
        }

        private void Colorpick_ColorChanged(object sender, Color color)
        {
            ColorPickerChangingColorFlag = true;
            //update color
            this.color = color;
            this.colorLighter = setLighterColor(color);
            brush = new SolidColorBrush(color);

            if (!CMYKSlidersChangingColorFlag)
            {
                //update RGB section 
                RTextBox.Text = Convert.ToInt32(color.R).ToString();
                GTextBox.Text = Convert.ToInt32(color.G).ToString();
                BTextBox.Text = Convert.ToInt32(color.B).ToString();
                RSlider.Value = Convert.ToInt32(color.R);
                GSlider.Value = Convert.ToInt32(color.G);
                BSlider.Value = Convert.ToInt32(color.B);

                //update CMYK section
                var R2 = RSlider.Value / 255;
                var G2 = GSlider.Value / 255;
                var B2 = BSlider.Value / 255;

                KSlider.Value = Math.Round((1 - Max(R2, G2, B2)), 4);
                CSlider.Value = (KSlider.Value != 1) ? Math.Round((1 - R2 - KSlider.Value) / (1 - KSlider.Value), 4) : 0;
                MSlider.Value = (KSlider.Value != 1) ? Math.Round((1 - G2 - KSlider.Value) / (1 - KSlider.Value), 4) : 0;
                YSlider.Value = (KSlider.Value != 1) ? Math.Round((1 - B2 - KSlider.Value) / (1 - KSlider.Value), 4) : 0;
                KTextBox.Text = KSlider.Value.ToString();
                CTextBox.Text = CSlider.Value.ToString();
                MTextBox.Text = MSlider.Value.ToString();
                YTextBox.Text = YSlider.Value.ToString();
            }
            else
            {
                //update CMYK textboxes
                KTextBox.Text = Math.Round(KSlider.Value, 4).ToString();
                CTextBox.Text = Math.Round(CSlider.Value, 4).ToString();
                MTextBox.Text = Math.Round(MSlider.Value, 4).ToString();
                YTextBox.Text = Math.Round(YSlider.Value, 4).ToString();

                //update RGB section 
                RTextBox.Text = Convert.ToInt32(color.R).ToString();
                GTextBox.Text = Convert.ToInt32(color.G).ToString();
                BTextBox.Text = Convert.ToInt32(color.B).ToString();
                RSlider.Value = Convert.ToInt32(color.R);
                GSlider.Value = Convert.ToInt32(color.G);
                BSlider.Value = Convert.ToInt32(color.B);
            }

            //update colorButton
            ColorButton.Background = brush;
            ColorPickerChangingColorFlag = false;
        }


        private void SliderRGB_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!ColorPickerChangingColorFlag)
            {
                RGBSlidersChangingColorFlag = true;

                //update color
                color = Color.FromArgb(255, (byte)RSlider.Value, (byte)GSlider.Value, (byte)BSlider.Value);
                brush = new SolidColorBrush(color);

                //update ColorPicker
                ColorPicker.Background = brush;
                ColorPicker.Color = color;
                Colorpick_ColorChanged(sender, color);

                RGBSlidersChangingColorFlag = false;
            }
        }

        private void SliderCMYK_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!ColorPickerChangingColorFlag)
            {
                CMYKSlidersChangingColorFlag = true;

                //update color
                var R = Math.Round(255 * (1 - CSlider.Value) * (1 - KSlider.Value));
                var G = Math.Round(255 * (1 - MSlider.Value) * (1 - KSlider.Value));
                var B = Math.Round(255 * (1 - YSlider.Value) * (1 - KSlider.Value));
                color = Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
                brush = new SolidColorBrush(color);

                //update ColorPicker
                ColorPicker.Background = brush;
                ColorPicker.Color = color;
                Colorpick_ColorChanged(sender, color);

                CMYKSlidersChangingColorFlag = false;
            }
        }

        private Color setLighterColor(Color color)
        {
            int r = Convert.ToInt32(color.R) + 17;
            int g = Convert.ToInt32(color.G) + 17;
            int b = Convert.ToInt32(color.B) + 17;

            if (r > 255) { r = 255; }
            if (g > 255) { g = 255; }
            if (b > 255) { b = 255; }

            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        private double Max(double r2, double g2, double b2)
        {
            return Math.Round(Math.Max(Math.Max(r2, g2), b2), 4);
        }

        private void PersonalizeTitleBar()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.BackgroundColor = Colors.Black;
            titleBar.ForegroundColor = Colors.White;

            titleBar.ButtonBackgroundColor = Colors.Black;
            titleBar.ButtonForegroundColor = Colors.White;
        }

        private void ColorButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ColorButton.Background = new SolidColorBrush(colorLighter);
        }

        private void ColorButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ColorButton.Background = new SolidColorBrush(color);
        }

        #endregion


        #region Transformations and Filtrs

        private byte[] originalArray, displayingArray;
        private double[] realBitValuesArray;
        private int bitmapHeight, bitmapWidth;
        private bool changedFromSlider = false;
        private double prevValueR=0, prevValueG=0, prevValueB=0, prevValueBrightness=0,
            prevValueMultiplying=1;

       

        private void RGBBrightnessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            changedFromSlider = true;
            SliderEnum wiesiek = SliderEnum.Brightness;
            double prevSLiderValue = 0;
            string senderName = (sender is Slider) ? (sender as Slider).Name : (sender as TextBox).Name;
            string sliderName = "", textboxName = "";

            switch (senderName)
            {
                case "RSlider2":
                case "RTextBox2":
                    sliderName = "RSlider2";
                    textboxName = "RTextBox2";
                    wiesiek = SliderEnum.R;
                    prevSLiderValue = prevValueR;
                    break;

                case "GSlider2":
                case "GTextBox2":
                    sliderName = "GSlider2";
                    textboxName = "GTextBox2";
                    wiesiek = SliderEnum.G;
                    prevSLiderValue = prevValueG;
                    break;

                case "BSlider2":
                case "BTextBox2":
                    sliderName = "BSlider2";
                    textboxName = "BTextBox2";
                    wiesiek = SliderEnum.B;
                    prevSLiderValue = prevValueB;
                    break;

                case "BrightnessSlider":
                case "BrightnessTextBox":
                    sliderName = "BrightnessSlider";
                    textboxName = "BrightnessTextBox";
                    wiesiek = SliderEnum.Brightness;
                    prevSLiderValue = prevValueBrightness;
                    break;
            }

            double sliderValue = ((Slider)this.FindName(sliderName)).Value;
            double sliderChangeValue = sliderValue - prevSLiderValue;
            double new1, new2, new3;
            ((TextBox)this.FindName(textboxName)).Text = sliderValue.ToString();

            if (selectedImage != null)
            {
                switch(wiesiek)
                {
                    case SliderEnum.R:
                        for (int i = 0; i < displayingArray.Length; i += 4)
                        {
                            new3 = realBitValuesArray[i + 2] + sliderChangeValue;
                            realBitValuesArray[i + 2] = new3;

                            displayingArray[i + 2] = (byte)((new3 < 255) ? ((new3 > 0) ? new3 : 0) : 255);
                        }
                        prevValueR = sliderValue;
                        break;

                    case SliderEnum.G:
                        for (int i = 0; i < displayingArray.Length; i += 4)
                        {
                            new2 = realBitValuesArray[i + 1] + sliderChangeValue;
                            realBitValuesArray[i + 1] = new2;

                            displayingArray[i + 1] = (byte)((new2 < 255) ? ((new2 > 0) ? new2 : 0) : 255);
                        }
                        prevValueG = sliderValue;
                        break;

                    case SliderEnum.B:
                        for (int i = 0; i < displayingArray.Length; i += 4)
                        { 
                            new1 = realBitValuesArray[i] + sliderChangeValue;
                            realBitValuesArray[i] = new1;

                            displayingArray[i] = (byte)((new1 < 255) ? ((new1 > 0) ? new1 : 0) : 255);
                        }
                        prevValueB = sliderValue;
                        break;

                    case SliderEnum.Brightness:
                        for (int i = 0; i < displayingArray.Length; i += 4)
                        {
                            new1 = realBitValuesArray[i] + sliderChangeValue;
                            new2 = realBitValuesArray[i + 1] + sliderChangeValue;
                            new3 = realBitValuesArray[i + 2] + sliderChangeValue;
                            realBitValuesArray[i] = new1;
                            realBitValuesArray[i + 1] = new2;
                            realBitValuesArray[i + 2] = new3;

                            displayingArray[i] = (byte)((new1 < 255) ? ((new1 > 0) ? new1 : 0) : 255);
                            displayingArray[i + 1] = (byte)((new2 < 255) ? ((new2 > 0) ? new2 : 0) : 255);
                            displayingArray[i + 2] = (byte)((new3 < 255) ? ((new3 > 0) ? new3 : 0) : 255);
                        }
                        prevValueBrightness = sliderValue;
                        break;
                }
                

                WriteableBitmap wbitmapNew = new WriteableBitmap(bitmapWidth, bitmapHeight);
                using (Stream stream = wbitmapNew.PixelBuffer.AsStream())
                {
                    stream.Write(displayingArray, 0, displayingArray.Length);
                }

                selectedImage.Source = wbitmapNew;
            }
            changedFromSlider = false;
        }

        private void Brightness_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!changedFromSlider)
                {
                    double value;
                    if (double.TryParse(BrightnessTextBox.Text, out value))
                    {
                        BrightnessSlider.Value = value;
                    }
                }
            }          
        }

        private void G_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!changedFromSlider)
                {
                    double value;
                    if (double.TryParse(GTextBox2.Text, out value))
                    {
                        GSlider2.Value = value;
                    }
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if(selectedImage != null)
            {
                ResetImageManipulator();

                WriteableBitmap wbitmapNew = new WriteableBitmap(bitmapWidth, bitmapHeight);
                using (Stream stream = wbitmapNew.PixelBuffer.AsStream())
                {
                    stream.Write(originalArray, 0, originalArray.Length);
                }
                selectedImage.Source = wbitmapNew;

            }
        }

        private void R_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!changedFromSlider)
                {
                    double value;
                    if (double.TryParse(RTextBox2.Text, out value))
                    {
                        RSlider2.Value = value;
                    }
                }
            }
        }

        private void B_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!changedFromSlider)
                {
                    double value;
                    if (double.TryParse(BTextBox2.Text, out value))
                    {
                        BSlider2.Value = value;
                    }
                }
            }
        }

        private void MultiplySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            changedFromSlider = true;

            double value = MultiplyingSlider.Value;
            if (value != 0)
            {              
                MultiplyTextBox.Text = value.ToString();

                if (value < 0) { value = ((-1) / value); }
                double new1, new2, new3;

                if (selectedImage != null)
                {
                    for (int i = 0; i < displayingArray.Length; i += 4)
                    {
                        new1 = realBitValuesArray[i] * (1/prevValueMultiplying) * value;
                        new2 = realBitValuesArray[i + 1] * (1 / prevValueMultiplying) * value;
                        new3 = realBitValuesArray[i + 2] * (1 / prevValueMultiplying) * value;
                        realBitValuesArray[i] = new1;
                        realBitValuesArray[i + 1] = new2;
                        realBitValuesArray[i + 2] = new3;

                        displayingArray[i] = (byte)((new1 < 255) ? ((new1 > 0) ? new1 : 0) : 255);
                        displayingArray[i + 1] = (byte)((new2 < 255) ? ((new2 > 0) ? new2 : 0) : 255);
                        displayingArray[i + 2] = (byte)((new3 < 255) ? ((new3 > 0) ? new3 : 0) : 255);
                    }

                    WriteableBitmap wbitmapNew = new WriteableBitmap(bitmapWidth, bitmapHeight);
                    using (Stream stream = wbitmapNew.PixelBuffer.AsStream())
                    {
                        stream.Write(displayingArray, 0, displayingArray.Length);
                    }

                    selectedImage.Source = wbitmapNew;
                    prevValueMultiplying = value;
                }
            }
            changedFromSlider = false;
        }

        private void MultiplyTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (!changedFromSlider)
                {
                    double value;
                    if (double.TryParse(MultiplyTextBox.Text, out value))
                    {
                        MultiplyingSlider.Value = value;
                    }
                }
            }
        }


        private void setImageResources()
        {
            WriteableBitmap wbitmap;
            //if ((selectedImage.Source as WriteableBitmap) is null)
            //{
            //    BitmapImage bi = selectedImage.Source as BitmapImage;

            //}
            wbitmap = selectedImage.Source as WriteableBitmap;
            bitmapHeight = wbitmap.PixelHeight;
            bitmapWidth = wbitmap.PixelWidth;
            using (Stream stream = wbitmap.PixelBuffer.AsStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                originalArray = memoryStream.ToArray();
            }           

            ResetImageManipulator();
        }

        private void ResetImageManipulator()
        {
            realBitValuesArray = new double[originalArray.Length];
            displayingArray = new byte[originalArray.Length];
            originalArray.CopyTo(realBitValuesArray, 0);
            originalArray.CopyTo(displayingArray, 0);

            prevValueR = 0;
            prevValueG = 0;
            prevValueB = 0;
            prevValueBrightness = 0;
            prevValueMultiplying = 1;
            RSlider2.Value = 0;
            GSlider2.Value = 0;
            BSlider2.Value = 0;
            BrightnessSlider.Value = 0;
            MultiplyingSlider.Value = 1;

            RTextBox2.Text = "0";
            GTextBox2.Text = "0";
            BTextBox2.Text = "0";
            BrightnessTextBox.Text = "0";
            MultiplyTextBox.Text = "1";           
        }


        #endregion
    }
}

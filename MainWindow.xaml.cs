using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Security.Cryptography.X509Certificates;
using RAMDATAACCESSLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TextBox = System.Windows.Controls.TextBox;
using System.Globalization;
using Microsoft.Win32;
using Point = System.Windows.Point;
using System.Windows.Shapes;

namespace rebarBenderMulti
{
    static class LineExtensions
    {
        public static void GetDirectionAndAngle(this System.Windows.Shapes.Line line, out Vector direction, out double angle)
        {
            Point startPoint = new Point(line.X1, line.Y1);
            Point endPoint = new Point(line.X2, line.Y2);

            //Direction is a vector direction of the line aka, (1,0) is a vector going to the right

            direction = endPoint - startPoint;

            angle = Math.Atan2(direction.Y, direction.X) * (180 / Math.PI);
        }
    }
    public partial class MainWindow : Window
    {
        public UIDocument uidoc {get; }
        public Document doc { get; }

        private RevitInfo revitInfo; // Create an instance of RevitInfo, not sure why I have to do this?

        // Create properties for data binding
        public decimal XValue { get; set; }
        public decimal YValue { get; set; }
        public decimal RotValue { get; set; }

        //Revit Canvas Factors
        private double revitZoomFactor = 1.0;
        private System.Windows.Point revitLastMousePosition;
        private List<System.Windows.Shapes.Line> revitSelectedLines = new List<System.Windows.Shapes.Line>();

        //RAM Canvas Factors
        private double ramZoomFactor = 1.0;
        private System.Windows.Point ramLastMousePosition;
        private List<System.Windows.Shapes.Line> ramSelectedLines = new List<System.Windows.Shapes.Line>();

        public MainWindow(UIDocument UiDoc )
        {
            uidoc = UiDoc;
            doc = UiDoc.Document;
            Title = "R2R Panel";
            InitializeComponent();


            // Attach the PreviewMouseWheel event to handle zoom
            scrollViewer.PreviewMouseWheel += ramScrollViewer_PreviewMouseWheel;
            scrollViewer1.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;

            // Attach mouse events for panning
            ramCanvas.MouseLeftButtonDown += ramCanvas_MouseLeftButtonDown;
            ramCanvas.MouseMove += ramCanvas_MouseMove;
            ramCanvas.MouseLeftButtonUp += ramCanvas_MouseLeftButtonUp;

            revitCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            revitCanvas.MouseMove += Canvas_MouseMove;
            revitCanvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;


            // Initialize the RevitInfo instance
            revitInfo = new RevitInfo();
            GatherGridsButton.Click += GatherGridsButton_Click;
            
        }
        private void GatherGridsButton_Click(object sender, RoutedEventArgs e)
        {
            Plot_Revit_Grids(sender, e);
        }

        public void Plot_Revit_Grids(object sender, RoutedEventArgs e)
        {
            List<CustomLine> revitGridsGathered = new List<CustomLine>();
            // Call the GatherRevitGrids method on the RevitInfo instance
            revitInfo.GatherRevitGrids(sender, e, doc, ref revitGridsGathered);

            //Plot all of the lines on the correct canvas
            foreach (var line in revitGridsGathered)
            {
                //FW - I have no idea why I need to put a negative on the Y VALUES!!! so weird
                System.Windows.Shapes.Line lineX = new System.Windows.Shapes.Line
                {
                    X1 = line.StartPoint.X,
                    Y1 = -line.StartPoint.Y,
                    X2 = line.EndPoint.X,
                    Y2 = -line.EndPoint.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Tag = line // Set the Tag property to the CustomLine object, FW need to read up on Tag
                };
                revitCanvas.Children.Add(lineX);
                lineX.MouseEnter += Line_MouseEnter;
                lineX.MouseLeave += Line_MouseLeave;
                AddCircleWithText(line.EndPoint.X, -line.EndPoint.Y, line.Name);

            }
            //AdjustZoomToFitLines();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; // Prevent standard scrolling

            if (e.Delta > 0)
            {
                // Zoom in
                revitZoomFactor *= 1.2; // You can adjust the zoom factor as needed
            }
            else
            {
                // Zoom out
                revitZoomFactor /= 1.2;
            }
            // Apply the zoom factor to the canvas content
            revitCanvas.LayoutTransform = new ScaleTransform(revitZoomFactor, revitZoomFactor);
        }

        private void ramScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; // Prevent standard scrolling

            if (e.Delta > 0)
            {
                // Zoom in // You can adjust the zoom factor as needed
                ramZoomFactor *= 1.2;
            }
            else
            {
                // Zoom out
                ramZoomFactor /= 1.2;
            }
            // Apply the zoom factor to the canvas content
            ramCanvas.LayoutTransform = new ScaleTransform(ramZoomFactor, ramZoomFactor);
        }



        //TESTING FOR CLICKING ON LINES
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //PANNING FUNCTION
            revitLastMousePosition = e.GetPosition(scrollViewer1);
            revitCanvas.CaptureMouse();


            if (e.OriginalSource is System.Windows.Shapes.Line line)
            {
                // Clicked on a line, add it to the selectedLines list
                if (!revitSelectedLines.Contains(line))
                {
                    line.Stroke = Brushes.Red;
                    revitSelectedLines.Add(line);
                    revitSelectedLinesListBox.Items.Add(((CustomLine)line.Tag).Name); // Add the name to the list, pure chatgpt here
                }
            }
        }

        private void ramCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //PANNING FUNCTION


            ramLastMousePosition = e.GetPosition(scrollViewer);
            ramCanvas.CaptureMouse();

            if (e.OriginalSource is System.Windows.Shapes.Line line)
            {
                // Clicked on a line, add it to the selectedLines list
                if (!ramSelectedLines.Contains(line))
                {
                    line.Stroke = Brushes.Red;
                    ramSelectedLines.Add(line);
                    ramSelectedLinesListBox.Items.Add(((CustomLine)line.Tag).Name); // Add the name to the list, pure chatgpt here
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            revitCanvas.ReleaseMouseCapture();
        }

        private void ramCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ramCanvas.ReleaseMouseCapture();
        }

        //END PANNING FUNCTION

        private void revitClearSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Shapes.Line line in revitSelectedLines)
            {
                line.Stroke = Brushes.Black;
            }
            revitSelectedLines.Clear();
            revitSelectedLinesListBox.Items.Clear(); // Clear the list of selected lines
        }

        private void ramClearSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (System.Windows.Shapes.Line line in revitSelectedLines)
            {
                line.Stroke = Brushes.Black;
            }
            ramSelectedLines.Clear();
            ramSelectedLinesListBox.Items.Clear(); // Clear the list of selected lines
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (revitCanvas.IsMouseCaptured)
            {
                System.Windows.Point position = e.GetPosition(scrollViewer1);
                double offsetX = position.X - revitLastMousePosition.X;
                double offsetY = position.Y - revitLastMousePosition.Y;

                // Update the position of the canvas content
                var transform = revitCanvas.RenderTransform as TranslateTransform ?? new TranslateTransform();
                transform.X += offsetX;
                transform.Y += offsetY;
                revitCanvas.RenderTransform = transform;

                revitLastMousePosition = position;
            }
        }

        private void ramCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ramCanvas.IsMouseCaptured)
            {
                System.Windows.Point position = e.GetPosition(scrollViewer);
                double offsetX = position.X - ramLastMousePosition.X;
                double offsetY = position.Y - ramLastMousePosition.Y;

                // Update the position of the canvas content
                var transform = ramCanvas.RenderTransform as TranslateTransform ?? new TranslateTransform();
                transform.X += offsetX;
                transform.Y += offsetY;
                ramCanvas.RenderTransform = transform;

                ramLastMousePosition = position;
            }
        }

        private void AddCircleWithText(double x, double y, string text)
        {
            System.Windows.Controls.Grid container = new System.Windows.Controls.Grid();

            System.Windows.Shapes.Ellipse circle = new System.Windows.Shapes.Ellipse
            {
                Width = 10, // Diameter = 2 * Radius
                Height = 10, // Diameter = 2 * Radius
                Fill = Brushes.Red,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            container.Children.Add(circle);
            container.Children.Add(textBlock);

            // Position the container at (x, y)
            Canvas.SetLeft(container, x - 5); // Adjust for the radius
            Canvas.SetTop(container, y - 5); // Adjust for the radius

            revitCanvas.Children.Add(container);
        }

        private void ramAddCircleWithText(double x, double y, string text)
        {
            System.Windows.Controls.Grid container = new System.Windows.Controls.Grid();

            System.Windows.Shapes.Ellipse circle = new System.Windows.Shapes.Ellipse
            {
                Width = 10, // Diameter = 2 * Radius
                Height = 10, // Diameter = 2 * Radius
                Fill = Brushes.Red,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };

            TextBlock textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            container.Children.Add(circle);
            container.Children.Add(textBlock);

            // Position the container at (x, y)
            Canvas.SetLeft(container, x - 5); // Adjust for the radius
            Canvas.SetTop(container, y - 5); // Adjust for the radius

            ramCanvas.Children.Add(container);
        }

        private void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Shapes.Line line)
            {
                // Change line color to red when hovered over
                line.Stroke = Brushes.Red;
            }
        }

        private void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Shapes.Line line)
            {
                // Change line color back to black when the mouse leaves
                line.Stroke = Brushes.Black;
            }
        }

        private void AdjustZoomToFitLines()
        {
            // Calculate the maximum X and Y coordinates of your lines
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (System.Windows.Shapes.Line line in revitCanvas.Children.OfType<System.Windows.Shapes.Line>())
            {
                maxX = Math.Max(maxX, Math.Max(line.X1, line.X2));
                maxY = Math.Max(maxY, Math.Max(line.Y1, line.Y2));
            }

            // Calculate the desired center point
            double centerX = maxX / 2;
            double centerY = maxY / 2;

            // Calculate the zoom factor required to fit the entire content within the canvas view
            double canvasWidth = revitCanvas.ActualWidth;
            double canvasHeight = revitCanvas.ActualHeight;

            double zoomX = canvasWidth / maxX;
            double zoomY = canvasHeight / maxY;

            double zoomFactor = Math.Min(zoomX, zoomY);

            // Apply the calculated zoom factor and center point to the canvas
            revitCanvas.LayoutTransform = new ScaleTransform(zoomFactor, zoomFactor);
            revitCanvas.RenderTransform = new TranslateTransform(centerX - canvasWidth / 2, centerY - canvasHeight / 2);
        }

        /// <summary>
        /// Gather RAM ITEMS
        /// </summary>
        

        private void getRAMResults_Click(object sender, RoutedEventArgs e)
        {
           
            MessageBox.Show("You clicked me");
            string RSSFileName = "I:\\Common\\06 Employee Personal Folders\\Austin Guter\\TestRAMModel\\testModel.rss";
            //ramFloorComboBox.Items = RAMInfo.GET_STORY_NAMES(RSSFileName);
            
            List<string> storyNames = RAMInfo.GET_STORY_NAMES(RSSFileName);
            // Set the ItemsSource of the ramFloorComboBox to the list of story names
            ramFloorComboBox.ItemsSource = storyNames;

        }

        private void getGridInfo_Click(object sender, EventArgs e)
        {
            if (ramFloorComboBox.SelectedIndex > -1) //somthing was selected
            {
                string RSSFileName = "I:\\Common\\06 Employee Personal Folders\\Austin Guter\\TestRAMModel\\testModel.rss";
                int storyValue = ramFloorComboBox.SelectedIndex;
                List<string> gridNames = new List<string>();
                List<RAMGrid> Grid_List = new List<RAMGrid>();
                List<r2rPoint> Point_List = new List<r2rPoint>();
                List<CustomLine> ramGridsGathered = new List<CustomLine>();

                gridNames = RAMInfo.GET_GRID_NAMES(RSSFileName, storyValue, ref Grid_List, ref Point_List, ref ramGridsGathered);

                // Extract the Beam Locations from a given Level

                List<RAMBeam> BeamInfo = new List<RAMBeam>();
                RAMInfo.GET_BEAM_INFO(RSSFileName, storyValue, ref BeamInfo);

                //Plot all of the lines on the correct canvas
                foreach (var line in ramGridsGathered)
                {
                    //FW - I have no idea why I need to put a negative on the Y VALUES!!! so weird
                    System.Windows.Shapes.Line lineX = new System.Windows.Shapes.Line
                    {
                        X1 = line.StartPoint.X,
                        Y1 = -line.StartPoint.Y,
                        X2 = line.EndPoint.X,
                        Y2 = -line.EndPoint.Y,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        Tag = line // Set the Tag property to the CustomLine object, FW need to read up on Tag
                    };
                    ramCanvas.Children.Add(lineX);
                    lineX.MouseEnter += Line_MouseEnter;
                    lineX.MouseLeave += Line_MouseLeave;
                    ramAddCircleWithText(line.EndPoint.X, -line.EndPoint.Y, line.Name);

                }
            }

            else //ensure that a user selects a level
            {
                MessageBox.Show("Please select a level, if no levels are shown above, click 'Gather RAM Info'");
            }


        }
        /// <summary>
        //SAVE AND READ PORTION OF THE CODE
        /// </summary>

        private void Calc_R2R(object sender, RoutedEventArgs e)

        {
            Point revit1start, revit1end, revit2start, revit2end;
            Point ram1start, ram1end, ram2start, ram2end;
            if (revitSelectedLines.Count == 2)
            { 
                revit1start = new Point(revitSelectedLines[0].X1, revitSelectedLines[0].Y1);
                revit1end = new Point(revitSelectedLines[0].X2, revitSelectedLines[0].Y2);
                revit2start = new Point(revitSelectedLines[1].X1, revitSelectedLines[1].Y1);
                revit2end = new Point(revitSelectedLines[1].X2, revitSelectedLines[1].Y2);
            }

            else
            {
                MessageBox.Show($"Pick only 2 lines that intersect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Exit the method since the condition is not met
            }
                

            if (ramSelectedLines.Count == 2)
            {
                ram1start = new Point(ramSelectedLines[0].X1, ramSelectedLines[0].Y1);
                ram1end = new Point(ramSelectedLines[0].X2, ramSelectedLines[0].Y2);
                ram2start = new Point(ramSelectedLines[1].X1, ramSelectedLines[1].Y1);
                ram2end = new Point(ramSelectedLines[1].X2, ramSelectedLines[1].Y2);
            }

            else
            {
                MessageBox.Show($"Pick only 2 lines that intersect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Exit the method since the condition is not met
            }

            //intersection of RAM lines and revit lines
            Point intersectionRAM = FindIntersection(ram1start, ram1end, ram2start, ram2end);
            Point intersectionRevit = FindIntersection(revit1start, revit1end, revit2start, revit2end);

            // Calculate offset
            double offsetX = intersectionRevit.X - intersectionRAM.X;
            double offsetY = intersectionRevit.Y - intersectionRAM.Y;

            XTextBox.Text = offsetX.ToString();
            YTextBox.Text = offsetY.ToString();

            // Determine vector direction and angle using the extension method
            Vector directionRAM1;
            double angleRAM1;
            ramSelectedLines[0].GetDirectionAndAngle(out directionRAM1, out angleRAM1);

            Vector directionRAM2;
            double angleRAM2;
            ramSelectedLines[1].GetDirectionAndAngle(out directionRAM2, out angleRAM2);

            // Determine vector direction and angle using the extension method
            Vector directionREVIT1;
            double angleREVIT1;
            revitSelectedLines[0].GetDirectionAndAngle(out directionREVIT1, out angleREVIT1);

            Vector directionREVIT2;
            double angleREVIT2;
            revitSelectedLines[1].GetDirectionAndAngle(out directionREVIT2, out angleREVIT2);

            //the value of x is the global rotation parameter
            double rot = FindRotation(directionRAM1, directionRAM2, directionREVIT1, directionREVIT2, angleRAM1, angleRAM2, angleREVIT1, angleREVIT2, ramSelectedLines[0], ramSelectedLines[1], revitSelectedLines[0], revitSelectedLines[1]);

            RotTextBox.Text = rot.ToString();

            // Bind the event handler to the TextChanged event of the TextBox controls.
            XTextBox.TextChanged += TextBox_TextChanged;
            YTextBox.TextChanged += TextBox_TextChanged;
            RotTextBox.TextChanged += TextBox_TextChanged;
        }



        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string inputText = textBox.Text;

            if (!IsDecimal(inputText))
            {
                // If the input is not a valid negative decimal, display an error message.
                textBox.Background = new SolidColorBrush(Colors.Red);


            }
            else
            {
                textBox.Background = SystemColors.WindowBrush;

                // Update XValue or YValue based on the TextBox that triggered the event
                if (textBox == XTextBox)
                {
                    XValue = decimal.Parse(inputText);
                }
                else if (textBox == YTextBox)
                {
                    YValue = decimal.Parse(inputText);
                }
                else if (textBox == RotTextBox)
                {
                    RotValue = decimal.Parse(inputText);
                }
            }
        }

        // Helper function to check if a string can be parsed as a negative decimal.
        private bool IsDecimal(string input)
        {
            // Attempt to parse the input as a negative decimal.
            if (decimal.TryParse(input, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal result))
            {
                // Check if the result is less than zero, indicating a negative decimal.
                return true;

            }

            return false; // Not a decimal.
        }

        private void SaveJson()
        {

            // Check if both XTextBox and YTextBox contain valid decimal values
            if (!IsDecimal(XTextBox.Text) || !IsDecimal(YTextBox.Text) || !IsDecimal(RotTextBox.Text))
            {
                MessageBox.Show("Please enter valid decimal values for X,Y, and rotation before saving.");
                return;
            }

            // Read the current values of XValue and YValue
            decimal currentXValue = XValue;
            decimal currentYValue = YValue;
            decimal currentRotValue = RotValue;


            // Create an instance of a class to hold XValue and YValue
            var data = new { XValue = currentXValue, YValue = currentYValue, RotValue = currentRotValue };


            // Convert the object to a JSON string
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            // Create a SaveFileDialog
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Save R2R mapping file",
                FileName = "R2R_Map.json"
            };

            // Show the SaveFileDialog
            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the selected file path from the SaveFileDialog
                string filePath = saveFileDialog.FileName;

                // Write the JSON string to the selected file
                System.IO.File.WriteAllText(filePath, json);

                MessageBox.Show("R2R data saved successfully!");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveJson();
        }

        private void LoadJson()
        {
            // Create an OpenFileDialog
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Open JSON File"
            };

            // Show the OpenFileDialog
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Read the JSON file
                    string json = System.IO.File.ReadAllText(openFileDialog.FileName);

                    // Deserialize the JSON string to an anonymous type with XValue and YValue properties
                    var data = JsonConvert.DeserializeAnonymousType(json, new { XValue = default(decimal), YValue = default(decimal), RotValue = default(decimal) });

                    // Set the values to XValue and YValue
                    XValue = data.XValue;
                    YValue = data.YValue;
                    RotValue = data.RotValue;
                    // Update the TextBox controls with the loaded values
                    XTextBox.Text = XValue.ToString();
                    YTextBox.Text = YValue.ToString();
                    RotTextBox.Text = RotValue.ToString();

                    MessageBox.Show("JSON data loaded successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading JSON file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadJson();
        }

        // Function to find the intersection point of two lines
        static System.Windows.Point FindIntersection(System.Windows.Point p1, System.Windows.Point p2, System.Windows.Point p3, System.Windows.Point p4)
        {
            double x1 = p1.X, y1 = p1.Y;
            double x2 = p2.X, y2 = p2.Y;
            double x3 = p3.X, y3 = p3.Y;
            double x4 = p4.X, y4 = p4.Y;

            double ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) /
                        ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            double x = x1 + ua * (x2 - x1);
            double y = y1 + ua * (y2 - y1);

            return new System.Windows.Point(x, y);
        }

        //it would be nice to re-write this function in the future to extend the line class to store all of this data in the line.

        static double FindRotation(Vector RAM1V, Vector RAM2V, Vector REVIT1V, Vector REVIT2V, double RAM1angle, double RAM2angle, double REVIT1angle, double REVIT2angle, System.Windows.Shapes.Line RAM1Line, System.Windows.Shapes.Line RAM2Line, System.Windows.Shapes.Line REVIT1Line, System.Windows.Shapes.Line REVIT2Line)
        {
            double RAMdelta = RAM2angle - RAM1angle;
            double Revitdelta = REVIT2angle - REVIT1angle;

            double rotationR2R = 0;

            if (RAMdelta - Revitdelta <= 1)
            {
                System.Console.WriteLine("rotation between angles is about the same");
                if (RAM1Line.Name == REVIT1Line.Name)
                {
                    rotationR2R = RAM1angle - REVIT1angle;
                }
                else if (RAM1Line.Name == REVIT2Line.Name)
                {
                    rotationR2R = RAM1angle - REVIT2angle;
                }
                else
                {
                    //this could be more elegant, this is a bit sloppy if the gridlines between revit and ram do not match, might have to prompt user for override.
                    rotationR2R = RAM1angle - REVIT1angle;
                }
            }
            else
            {
                System.Console.WriteLine("your angles between ram and revit do not match");
            }
            return rotationR2R;
        }
        ////////////////////////
        //END OF SAVE AND READ PORTION OF THE CODE
        /////////////////////////
        private void GatherBeamsButton_Click(object sender, RoutedEventArgs e)
        {
            List<CustomLine> revitBeamsGathered = new List<CustomLine>();
            revitInfo.GatherRevitBeams(sender, e, doc, uidoc, ref revitBeamsGathered);
        }


    }
}


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


namespace rebarBenderMulti
{
    public partial class MainWindow : Window
    {
        public UIDocument uidoc {get; }
        public Document doc { get; }

        private RevitInfo revitInfo; // Create an instance of RevitInfo, not sure why I have to do this?

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
    }
}


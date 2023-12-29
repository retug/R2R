using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace rebarBenderMulti
{
    /// <summary>
    /// Interaction logic for mapWindow.xaml
    /// </summary>
    public partial class mapWindow : Window
    {
        //Revit Canvas Factors
        private double mapZoomFactor = 1.0;
        private System.Windows.Point mapLastMousePosition;

        public mapWindow()
        {
            InitializeComponent();

            // Attach the PreviewMouseWheel event to handle zoom
            scrollViewer.PreviewMouseWheel += mapScrollViewer_PreviewMouseWheel;
            // Attach mouse events for panning
            mapCanvas.MouseLeftButtonDown += mapCanvas_MouseLeftButtonDown;
            mapCanvas.MouseMove += mapCanvas_MouseMove;
            mapCanvas.MouseLeftButtonUp += mapCanvas_MouseLeftButtonUp;
        }

        // ZOOMING FUNCTION
        private void mapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; // Prevent standard scrolling

            if (e.Delta > 0)
            {
                // Zoom in // You can adjust the zoom factor as needed
                mapZoomFactor *= 1.2;
            }
            else
            {
                // Zoom out
                mapZoomFactor /= 1.2;
            }

            // Apply the zoom factor to the canvas content
            ApplyZoomAndPanTransform();
        }

        // PANNING FUNCTION
        private void mapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mapLastMousePosition = e.GetPosition(scrollViewer);
            mapCanvas.CaptureMouse();
        }

        private void mapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mapCanvas.ReleaseMouseCapture();
        }

        private void mapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mapCanvas.IsMouseCaptured)
            {
                System.Windows.Point position = e.GetPosition(scrollViewer);
                double offsetX = position.X - mapLastMousePosition.X;
                double offsetY = position.Y - mapLastMousePosition.Y;

                // Update the position of the canvas content
                var transform = mapCanvas.RenderTransform as TranslateTransform ?? new TranslateTransform();
                transform.X += offsetX;
                transform.Y += offsetY;
                mapCanvas.RenderTransform = transform;

                mapLastMousePosition = position;
            }
        }

        private void ApplyZoomAndPanTransform()
        {
            // Apply the zoom factor and translation to the canvas content
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(mapZoomFactor, mapZoomFactor));
            transformGroup.Children.Add(new TranslateTransform(mapCanvas.RenderTransform.Value.OffsetX, mapCanvas.RenderTransform.Value.OffsetY));
            mapCanvas.RenderTransform = transformGroup;
        }
    }
}

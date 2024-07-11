using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Gaming.XboxGameBar;
using Microsoft.Gaming.XboxGameBar.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HuntMapOverlay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private XboxGameBarWidget widget = null;
        private HubConnection hubConnection;
        private List<ScreenLine> ScreenLines = new List<ScreenLine>();
        private XboxGameBarHotkeyWatcher hotkeyWatcher = null;
        public bool mapVisable = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            widget = e.Parameter as XboxGameBarWidget;
                
            if (widget != null)
            {
                List<VirtualKey> virtualKeys = new List<VirtualKey> { VirtualKey.Tab };
                hotkeyWatcher = XboxGameBarHotkeyWatcher.CreateWatcher(widget, virtualKeys);
                hotkeyWatcher.HotkeySetStateChanged += HotkeyWatcher_HotkeySetStateChanged;
                hotkeyWatcher.Start();
                widget.CloseRequested += Widget_Closed;
                SetupSignalR();
            }
        }

        private async void HotkeyWatcher_HotkeySetStateChanged(XboxGameBarHotkeyWatcher sender, HotkeySetStateChangedArgs args)
        {
            if (args.HotkeySetDown)
            {
                // Run the UI update code on the UI thread
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    mapCanvas.Visibility = mapCanvas.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                });
            }
        }


        private async void Widget_Closed(XboxGameBarWidget sender, object args)
        {
            // Ensure all resources are properly disposed when the widget is closed
            if (hubConnection != null)
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    await hubConnection.StopAsync();
                }
                hubConnection.Remove("ReceiveLine");
                hubConnection.Remove("ReceiveDeleteLine");
                await hubConnection.DisposeAsync();
                
            }

            // Detach event handlers
            widget.CloseRequested -= Widget_Closed;
            hotkeyWatcher.HotkeySetStateChanged -= HotkeyWatcher_HotkeySetStateChanged;
            hotkeyWatcher = null;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (widget != null)
            {
                widget.WindowBoundsChanged -= Widget_WindowSizeChanged; // Detach event handler
                widget = null; // Clear reference
            }

            if (hubConnection != null)
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    hubConnection.StopAsync(); // Stop connection
                }
                hubConnection.DisposeAsync(); // Dispose object
                hubConnection = null; // Clear reference
            }
        }
        private async void SetupSignalR()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://hunttoolsapi.sardinhunt.com/maphub")
            .Build();

            hubConnection.On<LineData>("ReceiveLine", lines =>
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        Debug.WriteLine("Received lines");
                        if (lines != null)
                        {
                            DrawLines(lines);
                        }
                        else
                        {
                            Debug.WriteLine("No lines data received.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Exception in DrawLines: {ex.Message}");
                    }
                });
            });

            hubConnection.On<string>("ReceiveDeleteLine", id =>
            {
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    EarseLine(id);
                });                                             
            });

            try
            {
                await hubConnection.StartAsync();
                Debug.WriteLine("Connection started");

                // Attempt to join a room
                int roomTier = await JoinRoom("ZGKUM");  // Replace "YD8UN" with your desired room ID
                Debug.WriteLine($"Joined Room with Tier: {roomTier}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting connection: " + ex.Message);
            }
        }
        private async Task<int> JoinRoom(string roomId)
        {
            if (hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    return await hubConnection.InvokeAsync<int>("JoinRoom", roomId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error joining room: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new InvalidOperationException("Connection not started.");
            }
        }

        private void Widget_WindowSizeChanged(XboxGameBarWidget sender, object args)
        {
           // this.Width = sender.WindowBounds.Width;
           // this.Height = sender.WindowBounds.Height;
        }

        private void EarseLine(string id)
        {
            if (ScreenLines.Where(line => line.Id == id).ToList().Any())
            {
                foreach (var line in ScreenLines.Where(line => line.Id == id).ToList()[0].Lines)
                {
                    mapCanvas.Children.Remove(line);
                }
                ScreenLines.Remove(ScreenLines.Where(line => line.Id == id).ToList()[0]);
            }
        }
        private void DrawLines(LineData line)
        {
                    Point lastPoint = null;
                    var localLine = new ScreenLine() { Id = line.Id };
                    foreach (var point in line.Points)
                    {
                        if (lastPoint != null)
                        {
                            localLine.Lines.Add(DrawLine(lastPoint, point));
                        }
                        lastPoint = point;  // Update lastPoint for the next segment
                    }
                    ScreenLines.Add(localLine);
            
        }
        private Line DrawLine(Point start, Point end)
        {
            double startX = (end.Lng * (widget.WindowBounds.Width * .3645833333333333 / 1000))  + (widget.WindowBounds.Width * 0.317578125);
            double startY = mapCanvas.ActualHeight - (end.Lat * (widget.WindowBounds.Width * .3645833333333333 / 1000)) - (((widget.WindowBounds.Height + 100) * .171296) - 100);
            double endX = (start.Lng * (widget.WindowBounds.Width * .3645833333333333 / 1000)) + (widget.WindowBounds.Width * 0.317578125);
            double endY = mapCanvas.ActualHeight - (start.Lat * (widget.WindowBounds.Width * .3645833333333333 / 1000)) - (((widget.WindowBounds.Height + 100) * .171296) - 100);

            var line = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endY,
                Stroke = new SolidColorBrush(Windows.UI.Colors.Red),
                StrokeThickness = 2
            };
            mapCanvas.Children.Add(line);
            return line;
        }
    }
    public class LineData
    {
        public string Id { get; set; }

        public List<Point> Points { get; set; }
    }

    public class Point
    {
        public double Lat { get; set; }

        public double Lng { get; set; }
    }

    public class ScreenLine
    {
        public string Id { get; set; }
        public List<Line> Lines { get; set; } = new List<Line>();
    }
}

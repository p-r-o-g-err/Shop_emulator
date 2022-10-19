using Shop_emulator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;

namespace Shop_emulator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private Random rnd;
		private DispatcherTimer dispTimer;
		private Shop shop;

		public MainWindow()
        {
            InitializeComponent();


			rnd = new Random(0);
			int t = 3;
			int p = 3; 

			shop = new Shop()
			{
				rnd = rnd,
				MaxArriveTime = t,
				MaxArriveCount = p, 
			};

			ObservableCollection<Cashier> cashiers = new ObservableCollection<Cashier>()
			{
					new Cashier(shop) {
						Name = "1",
						MinServiceTime = 2,
						MaxServiceTime = 4,
					},
					new Cashier(shop) {
						Name = "2",
						MinServiceTime = 5,
						MaxServiceTime = 10,
					},
					new Cashier(shop) {
						Name = "3",
						MinServiceTime = 15,
						MaxServiceTime = 20,
					},
			};
			foreach (Cashier cashier in cashiers)
            {
				cashier.EstimatedServiceTime = Math.Round((double)(cashier.MinServiceTime + cashier.MaxServiceTime) / 2, 2);
				cashier.EstimatedQueueServiceTime = 0;
			} 

			shop.Cashiers = cashiers;
			 
			this.DataContext = shop;
			CashiersList.ItemsSource = shop.Cashiers;
			PauseButton.IsEnabled = false;
			StopButton.IsEnabled = false;
			MaxArriveTimeControl.Value = t;
		}

		private void OnTimedEvent(object sender, EventArgs e)
		{ 
			List<string> logs = shop.ShopTick();

			foreach (string log in logs) {
				Log.Inlines.Add(new Run($"{shop.TimeElapsed, 3} с: ") { FontWeight = FontWeights.Bold });
				Log.Inlines.Add(new Run($"{log}\n") { FontWeight = FontWeights.Normal });
			}
			LogBox.ScrollToEnd();
		}
		

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
			dispTimer = new DispatcherTimer();
			dispTimer.Interval = TimeSpan.FromMilliseconds(slValue.Value);
			dispTimer.Tick += OnTimedEvent; 
			dispTimer.Start();
			PauseButton.IsEnabled = true;
			StartButton.IsEnabled = false;
			StopButton.IsEnabled = true;
		}

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        { 
			dispTimer.Stop();
			StartButton.IsEnabled = true;
			PauseButton.IsEnabled = false;
			StopButton.IsEnabled = true;
		}

        private void AddCashier_Click(object sender, RoutedEventArgs e)
        { 
			shop.AddCashier(minServiceTime: 3, maxServiceTime: 5);
			CashiersList.ItemsSource = shop.Cashiers;
			Log.Inlines.Add(new Run($"{shop.TimeElapsed, 3} с: ") { FontWeight = FontWeights.Bold });
			Log.Inlines.Add(new Run($"Добавлен кассир {shop.Cashiers.Last().Name}\n") { FontWeight = FontWeights.Normal });
		}

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
			dispTimer.Stop();
			StartButton.IsEnabled = true;
			PauseButton.IsEnabled = false;
			StopButton.IsEnabled = false;

			shop.TimeElapsed = 0;
            shop.NewCustomers = 0;
            shop.timeNextCustomers = 1;
            shop.CustomersServiced = 0; 
			shop.CustomersInQueues = 0;

			foreach (Cashier cashier in shop.Cashiers)
			{
				cashier.CustomersInQueue = 0;
				cashier.timeNextService = -1;
				cashier.creationTime = 0;
				cashier.CustomersServiced = 0;
				cashier.AvgServiceTime = 0;
				cashier.EstimatedQueueServiceTime = 0;
				cashier.Background = new SolidColorBrush(Color.FromRgb(211, 235, 255)); 
				cashier.EstimatedServiceTime = Math.Round((double)(cashier.MinServiceTime + cashier.MaxServiceTime) / 2, 2); 
			} 
		}																																																																						

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
			Button cmd = (Button)sender;
			if (cmd.DataContext is Cashier)
			{
				Cashier cashier = (Cashier)cmd.DataContext;
				string name = cashier.Name;
				shop.RemoveCashier(cashier);  
				Log.Inlines.Add(new Run($"{shop.TimeElapsed, 3} с: ") { FontWeight = FontWeights.Bold });
				Log.Inlines.Add(new Run($"Удалён кассир {name}\n") { FontWeight = FontWeights.Normal });
			}
		}

        private void DeleteAllCashiers_Click(object sender, RoutedEventArgs e)
        {
			MessageBoxResult answer = System.Windows.MessageBox.Show(
					  "Все кассы будут удалены. Продолжить?",
					  "Предупреждение",
					  MessageBoxButton.YesNo,
					  MessageBoxImage.Warning);
			if (answer == MessageBoxResult.Yes)
            {
				shop.Cashiers.Clear();
				shop.NewCustomers += shop.CustomersInQueues;
				shop.CustomersInQueues = 0;
				Log.Inlines.Add(new Run($"{shop.TimeElapsed, 3} с: ") { FontWeight = FontWeights.Bold });
				Log.Inlines.Add(new Run($"Удалены все кассиры\n") { FontWeight = FontWeights.Normal });
			}
		}

        private void MinServiceTimeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
			IntegerUpDown cmd = (IntegerUpDown)sender;
			if (cmd.DataContext is Cashier)
			{
				Cashier cashier = (Cashier)cmd.DataContext;
				if (cmd.Value > cashier.MaxServiceTime)
				{
					cmd.Value = Convert.ToInt16(e.OldValue);
					return;
				}
				cashier.EstimatedServiceTime = Math.Round((double)(cashier.MinServiceTime + cashier.MaxServiceTime) / 2, 2);
				cashier.EstimatedQueueServiceTime = cashier.EstimatedServiceTime * cashier.CustomersInQueue;
			}  
		}

        private void MaxServiceTimeControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
			IntegerUpDown cmd = (IntegerUpDown)sender;
			if (cmd.DataContext is Cashier)
			{
				Cashier cashier = (Cashier)cmd.DataContext;
				if (cmd.Value < cashier.MinServiceTime)
				{
					cmd.Value = Convert.ToInt16(e.OldValue);
					return;
				}
				cashier.EstimatedServiceTime = Math.Round((double)(cashier.MinServiceTime + cashier.MaxServiceTime) / 2, 2);
				cashier.EstimatedQueueServiceTime = cashier.EstimatedServiceTime * cashier.CustomersInQueue;
			}
		}

        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
			if (dispTimer != null)
				dispTimer.Interval = TimeSpan.FromMilliseconds(slValue.Value); 
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Shop_emulator.Models
{
    // Кассир
    // Хранит информацию о скорости обслуживания и длине очереди
    // Обслуживает покупателей
    internal class Cashier : INotifyPropertyChanged
	{
		public Shop shop;

		private int minServiceTime;    // Минимальное время на обслуживание покупателя
		private int maxServiceTime;    // Максимальное время на обслуживание покупателя
		private int customersInQueue;  // Текущая длина очереди (включая обслуживаемого сейчас покупателя)
		public int timeNextService;   // Время с начала эмуляции, в которое должен быть обслужен текущий покупатель
		public int creationTime;      // Время с начала эмуляции, в которое класс был создан
		private int customersServiced; // Число обслуженных клиентов 
		private double avgServiceTime; // Среднее время обслуживания покупателя
		private double estimatedServiceTime; // Ожидаемое время обслуживания покупателя
		private SolidColorBrush background; // Цвет фона объекта

		public Cashier()
		{
			Name = "";
			minServiceTime = 0;
			maxServiceTime = 1;
			customersInQueue = 0;
			timeNextService = -1;
			customersServiced = 0;
			avgServiceTime = 0;
			estimatedQueueServiceTime = 0;
			Background = new SolidColorBrush(Color.FromRgb(211, 235, 255));
		}
		public Cashier(Shop shop) : this()
		{
			this.shop = shop;
			this.creationTime = shop.TimeElapsed;
		}

		public string Name { get; set; }

		public int CustomersInQueue
		{
			get { return this.customersInQueue; }
			set
			{
				if (value != this.customersInQueue)
				{
					this.customersInQueue = value;
					OnPropertyChanged();
				}
			}
		}

		public int CustomersServiced
		{
			get { return this.customersServiced; }
			set
			{
				if (value != this.customersServiced)
				{
					this.customersServiced = value;
					OnPropertyChanged();
				}
			}
		}

		


		public SolidColorBrush Background
		{
			get { return this.background; }
			set
			{
				if (value != this.background)
				{
					this.background = value;
					OnPropertyChanged();
				}
			}
		}

		public int MinServiceTime
		{
			get { return this.minServiceTime; }
			set
			{
				if (value != this.minServiceTime)
				{
					this.minServiceTime = value;
					OnPropertyChanged();
				}
			}
		}

		public int MaxServiceTime
		{
			get { return this.maxServiceTime; }
			set
			{
				if (value != this.maxServiceTime)
				{
					this.maxServiceTime = value;
					OnPropertyChanged();
				}
			}
		}

		public double AvgServiceTime  
		{
			get { return this.avgServiceTime; }
			set
			{
				if (value != this.avgServiceTime)
				{
					this.avgServiceTime = value;
					OnPropertyChanged();
				}
			} 
		}

		

		public double EstimatedServiceTime
		{
			get { return this.estimatedServiceTime; }
			set
			{
				if (value != this.estimatedServiceTime)
				{
					this.estimatedServiceTime = value;
					OnPropertyChanged();
				}
			}
		}


		private double estimatedQueueServiceTime; // Ожидаемое время обслуживания всей текущей очереди
		public double EstimatedQueueServiceTime
		{
			get { return this.estimatedQueueServiceTime; }
			set
			{
				if (value != this.estimatedQueueServiceTime)
				{
					this.estimatedQueueServiceTime = value;
					OnPropertyChanged();
				}
			}
		}
		 
		


		// Обслуживание текущего покупателя, может продолжаться или окончиться с уменьшением queueLength на 1
		public void ServiceCustomer()
		{
			if (customersServiced == 0) AvgServiceTime = 0;
			else AvgServiceTime = Math.Round((double)(shop.TimeElapsed - creationTime) / customersServiced, 2);


			if (customersInQueue > 0)
			{ 
				
				if (shop.TimeElapsed == timeNextService)
				{
					customersInQueue--;
					customersServiced++;

					shop.CustomersInQueues--;
					shop.CustomersServiced++;

					if (customersInQueue == 0)
						timeNextService = -1;
					else
						timeNextService = shop.TimeElapsed + shop.rnd.Next(minServiceTime, maxServiceTime + 1);
				}
				else if (shop.TimeElapsed > timeNextService)
                {
					timeNextService = shop.TimeElapsed + shop.rnd.Next(minServiceTime, maxServiceTime + 1);
					EstimatedQueueServiceTime = 0; 
				}
				EstimatedQueueServiceTime = EstimatedServiceTime * CustomersInQueue;	
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

	}
}

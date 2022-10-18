using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Shop_emulator.Models
{
    // Магазин
    // Хранит кассиров, информацию о генерации покупателей
    // Генерирует покупателей, распределяет покупателей по кассам, заставляет кассы работать
    internal class Shop : INotifyPropertyChanged
	{
		public Random rnd;

		private int timeElapsed;    // Время с начала эмуляции
		private ObservableCollection<Cashier> cashiers;  // Список всех кассиров
		private int minArriveTime;       // Минимальное время между прибытием покупателей (всегда 1)
		private int maxArriveTime;       // Максимальное время между прибытием покупателей
		private int minArriveCount;      // Минимальное количество прибывающих покупателей (всегда 0)
		private int maxArriveCount;      // Максимальное количество прибывающих покупателей
		private int newCustomers;        // Число покупателей вне очередей
		public int timeNextCustomers;   // Время с начала эмуляции, в которое должны прибыть новые покупатели
		private int customersServiced;  // Число обслуженных на всех кассах покупателей
		private int customersInQueues; // Число покупателей во всех очередях

		public Shop()
		{
			timeElapsed = 0;
			cashiers = new ObservableCollection<Cashier>();
			minArriveTime = 1;
			maxArriveTime = 1;
			minArriveCount = 0;
			maxArriveCount = 1;
			newCustomers = 0;
			timeNextCustomers = 1;
			customersServiced = 0;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}

		public ObservableCollection<Cashier> Cashiers
		{
			get { return this.cashiers; }
			set
			{
				if (value != this.cashiers)
				{
					this.cashiers = value;
					OnPropertyChanged();
				}
			}
		}

		public int MaxArriveTime
		{
			get { return this.maxArriveTime; }
			set
			{
				if (value != this.maxArriveTime)
				{
					this.maxArriveTime = value;
					OnPropertyChanged();
				}
			}
		}

		public int NewCustomers
		{
			get { return this.newCustomers; }
			set
			{
				if (value != this.newCustomers)
				{
					this.newCustomers = value;
					OnPropertyChanged();
				}
			}
		}
		

		public int MaxArriveCount
		{
			get { return this.maxArriveCount; }
			set
			{
				if (value != this.maxArriveCount)
				{
					this.maxArriveCount = value;
					OnPropertyChanged();
				}
			}
		} 
		

		public int CustomersInQueues  
		{
			get { return this.customersInQueues; }
			set
			{
				if (value != this.customersInQueues)
				{
					this.customersInQueues = value;
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

		public int TimeElapsed  
		{
			get { return this.timeElapsed; }
			set
			{
				if (value != this.timeElapsed)
				{
					this.timeElapsed = value;
					OnPropertyChanged();
				}
			}
		}

		public void SetBackgroundForCashiers()
        {
			if (Cashiers.Count > 0)
            {
				int maxValue = Cashiers.Max(p => p.CustomersInQueue);
				int minValue = Cashiers.Min(p => p.CustomersInQueue);

				foreach (Cashier cashier in Cashiers)
				{
					if (cashier.CustomersInQueue == maxValue)
						cashier.Background = new SolidColorBrush(Color.FromRgb(11,218,81));
					else if (cashier.CustomersInQueue == minValue)
					{
						cashier.Background = new SolidColorBrush(Color.FromRgb(255, 138, 102));
					}
					else cashier.Background = new SolidColorBrush(Color.FromRgb(211, 235, 255));
				}
			} 
		}


		// Эмуляция 1 секунды работы магазина
		public void ShopTick()
		{
			SetBackgroundForCashiers();
			TimeElapsed++; 
			CustomersArrive();
			if (newCustomers > 0 && cashiers.Count > 0)
				DistributeNewCustomers();
			foreach (Cashier c in cashiers)
				c.ServiceCustomer(); 

		}

		// Генерация новых покупателей
		public void CustomersArrive()
		{
			if (timeElapsed == timeNextCustomers)
			{
				timeNextCustomers = timeElapsed + rnd.Next(minArriveTime, maxArriveTime + 1);
				NewCustomers += rnd.Next(minArriveCount, maxArriveCount + 1);
			}
			else if (timeElapsed > timeNextCustomers)
				timeNextCustomers = timeElapsed + rnd.Next(minArriveTime, maxArriveTime + 1);
		}

		// Распределение новых покупателей по кассам
		public void DistributeNewCustomers()
		{
			for (; newCustomers > 0; newCustomers--)
			{
				int minQueue = cashiers[0].CustomersInQueue;
				int minQueueIndex = 0;
				for (int i = 0; i < cashiers.Count; ++i)
					if (cashiers[i].CustomersInQueue < minQueue)
					{
						minQueue = cashiers[i].CustomersInQueue;
						minQueueIndex = i;
					}
				cashiers[minQueueIndex].CustomersInQueue++; 
				CustomersInQueues++;
				 
			}
		}

		// Добавить кассира с именем в поле name
		public void AddCashier(string name, int minServiceTime, int maxServiceTime)
		{
			Cashiers.Add(new Cashier()
			{
				shop = this,
				Name = name,
				MinServiceTime = minServiceTime,
				MaxServiceTime = maxServiceTime
			});
		}

		// Добавить кассира с генерируемым номером в поле name
		public void AddCashier(int minServiceTime, int maxServiceTime)
		{
			int maxName = 1;
			for (int i = 0; i < Cashiers.Count; ++i)
				if (int.TryParse(Cashiers[i].Name, out int currentName))
					if (currentName >= maxName)
						maxName = currentName + 1;
			Cashier cashier = new Cashier(this)
			{
				Name = maxName.ToString(),
				MinServiceTime = minServiceTime,
				MaxServiceTime = maxServiceTime
			};
			cashier.EstimatedServiceTime = Math.Round((double)(cashier.MinServiceTime + cashier.MaxServiceTime) / 2, 2);
			cashier.EstimatedQueueServiceTime = 0;
			Cashiers.Add(cashier); 
		}

		// Удалить кассира по его номеру в списке, его очередь отправляется на распределение по другим кассам
		public void RemoveCashier(int i)
		{
			newCustomers += cashiers[i].CustomersInQueue;
			cashiers.RemoveAt(i);
		}

		// Удалить кассира по его полю name, его очередь отправляется на распределение по другим кассам
		public void RemoveCashier(string name)
		{
			int i = 0;
			for (; i < cashiers.Count; ++i)
				if (cashiers[i].Name == name)
					break;
			RemoveCashier(i);
		}
	}
}

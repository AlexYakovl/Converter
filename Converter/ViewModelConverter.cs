using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConverterAPI;

namespace Converter
{
    public class Currency
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Rate { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Code})";
        }
    }

    internal class ViewModelConverter : INotifyPropertyChanged
    {
        private readonly API api;
        private RatesJson ratesData;
        private Dictionary<string, Valute> rates;
        public ObservableCollection<Currency> Currencies { get; private set; }

        private bool loading = false;
        public bool Loading
        {
            get => loading;
            set
            {
                if (loading != value)
                {
                    loading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Ready));
                }
            }
        }

        public bool Ready => !loading;

        private DateTime pickedDate = DateTime.Today;
        public DateTime PickedDate
        {
            get => pickedDate;
            set
            {
                if (pickedDate != value)
                {
                    pickedDate = value;
                    OnPropertyChanged();
                    GetNewRates();
                }
            }
        }

        private Currency fromMoney;
        public Currency FromMoney
        {
            get => fromMoney;
            set
            {
                if (fromMoney != value)
                {
                    fromMoney = value;
                    OnPropertyChanged();
                    DoConversion();
                }
            }
        }

        private Currency toMoney;
        public Currency ToMoney
        {
            get => toMoney;
            set
            {
                if (toMoney != value)
                {
                    toMoney = value;
                    OnPropertyChanged();
                    DoConversion();
                }
            }
        }

        private string moneyAmount = "1,00";
        public string MoneyAmount
        {
            get => moneyAmount;
            set
            {
                if (moneyAmount != value)
                {
                    moneyAmount = value;
                    OnPropertyChanged();
                    DoConversion();
                }
            }
        }

        private string resultMoney = "1,00";
        public string ResultMoney
        {
            get => resultMoney;
            set
            {
                if (resultMoney != value)
                {
                    resultMoney = value;
                    OnPropertyChanged();
                }
            }
        }

        public ViewModelConverter()
        {
            api = new API();
            Currencies = new ObservableCollection<Currency>();
            GetNewRates();
        }

        private async Task GetNewRates()
        {
            try
            {
                Loading = true;

                string oldFromCurrencyCode = FromMoney?.Code;
                string oldToCurrencyCode = ToMoney?.Code;
                string oldMoneyAmount = MoneyAmount;

                ratesData = await api.CallAPI(pickedDate);
                rates = ratesData.Rates;

                Currencies.Clear();
                foreach (var money in rates)
                {
                    Currencies.Add(new Currency
                    {
                        Code = money.Key,
                        Name = money.Value.Name,
                        Rate = (decimal)money.Value.Value
                    });
                }

                FromMoney = Currencies.FirstOrDefault(c => c.Code == oldFromCurrencyCode) ?? Currencies.FirstOrDefault();

                ToMoney = Currencies.FirstOrDefault(c => c.Code == oldToCurrencyCode) ?? Currencies.FirstOrDefault();

                MoneyAmount = oldMoneyAmount;

                PickedDate = ratesData.Date;
                Loading = false;

                DoConversion();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки валют: {ex.Message}");
            }
        }


        private void DoConversion()
        {
            if (decimal.TryParse(MoneyAmount, out decimal amount) &&
                FromMoney != null &&
                ToMoney != null &&
                rates != null)
            {
                double result = API.ConvertCurrencies(
                    rates[FromMoney.Code],
                    rates[ToMoney.Code],
                    (double)amount);

                ResultMoney = result.ToString("F2");
            }
            else
            {
                ResultMoney = string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ConverterAPI;

namespace Converter
{
    internal class ViewModelConverter : INotifyPropertyChanged
    {
        private readonly API api;
        private RatesJson ratesData;
        private Dictionary<string, Valute> rates;
        public ObservableCollection<string> Currencies { get; private set; }

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

        public bool Ready
        {
            get => !loading;
        }

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

        private string fromMoney = "";
        public string FromMoney
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

        private string toMoney = "";
        public string ToMoney
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
            Currencies = [];
            GetNewRates();
        }

        private async Task GetNewRates()
        {
            try
            {
                Loading = true;

                string oldFromMoney = FromMoney;
                string oldToMoney = ToMoney;

                ratesData = await api.CallAPI(pickedDate);
                rates = ratesData.Rates;

                Currencies.Clear();
                foreach (var money in rates)
                {
                    Currencies.Add($"{money.Value.Name} ({money.Key})");
                }

                if (!string.IsNullOrEmpty(oldFromMoney))
                    FromMoney = Currencies.FirstOrDefault(x => x == oldFromMoney);
                else
                    FromMoney = Currencies.FirstOrDefault();

                if (!string.IsNullOrEmpty(oldToMoney))
                    ToMoney = Currencies.FirstOrDefault(x => x == oldToMoney);
                else
                    ToMoney = Currencies.FirstOrDefault();

                PickedDate = ratesData.Date;
                Loading = false;
                DoConversion();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки валют: {ex.Message}");
            }
        }

        private static string GetMoneyCode(string moneyName)
        {
            return moneyName.Split('(', ')')[^2];
        }

        private void DoConversion()
        {
            if (Double.TryParse(MoneyAmount, out double amount) &&
                !string.IsNullOrEmpty(FromMoney) &&
                !string.IsNullOrEmpty(ToMoney) &&
                !string.IsNullOrEmpty(MoneyAmount) &&
                rates != null)
            {
                string fromCode = GetMoneyCode(FromMoney);
                string toCode = GetMoneyCode(ToMoney);

                if (rates.ContainsKey(fromCode) && rates.ContainsKey(toCode))
                {
                    Valute from = rates[fromCode];
                    Valute to = rates[toCode];

                    double result = API.ConvertCurrencies(from, to, amount);
                    ResultMoney = result.ToString("F2");
                }
                else
                {
                    ResultMoney = string.Empty;
                }
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
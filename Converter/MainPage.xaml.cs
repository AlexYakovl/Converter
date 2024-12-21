using ConverterAPI;

namespace Converter
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            DateSelector.MaximumDate = DateTime.Today;
        }
    }
}

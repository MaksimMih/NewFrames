using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Nazv_orgsnizaciy.Frames
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private List<Service> _ServiceList = new List<Service>();
        public List<Service> ServiceList
        {
            get
            {
                var FilteredServiceList = _ServiceList.FindAll(item =>
                item.DiscountFloat >= CurrentDiscountFilter.Item1 &&
                item.DiscountFloat < CurrentDiscountFilter.Item2);

                if (SearchFilter != "")
                    FilteredServiceList = FilteredServiceList.Where(item =>
                        item.Title.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) != -1 ||
                        item.DescriptionString.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) != -1).ToList();

                if (SortPriceAscending)
                    return FilteredServiceList
                        .OrderBy(item => Double.Parse(item.CostWithDiscount))
                        .ToList();
                else
                    return FilteredServiceList
                        .OrderByDescending(item => Double.Parse(item.CostWithDiscount))
                        .ToList();

            }
            set
            {
                _ServiceList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                    // и еще счетчики - их добавьте сами
                }
            }
        }

        public  MainPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += MainPage_Loaded;
            //MainWindow.WindText = "Услуги автосервиса";
        }


        public void DoUpdateMainPage()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                PropertyChanged(this, new PropertyChangedEventArgs("AdminVisibility"));
            }
        }


        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ServiceList = Core.DB.Service.ToList();
            //MessageBox.Show("loaded");
            PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
        }

        private Boolean _SortPriceAscending = true;
        public Boolean SortPriceAscending
        {
            get { return _SortPriceAscending; }
            set
            {
                _SortPriceAscending = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ServicesCount"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FilteredServicesCount"));
                }
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SortPriceAscending = (sender as RadioButton).Tag.ToString() == "1";
        }


        private List<Tuple<string, double, double>> FilterByDiscountValuesList =
        new List<Tuple<string, double, double>>() {
        Tuple.Create("Все записи", 0d, 1d),
        Tuple.Create("от 0% до 5%", 0d, 0.05d),
        Tuple.Create("от 5% до 15%", 0.05d, 0.15d),
        Tuple.Create("от 15% до 30%", 0.15d, 0.3d),
        Tuple.Create("от 30% до 70%", 0.3d, 0.7d),
        Tuple.Create("от 70% до 100%", 0.7d, 1d)
        };

        public List<string> FilterByDiscountNamesList
        {
            get
            {
                return FilterByDiscountValuesList
                    .Select(item => item.Item1)
                    .ToList();
            }
        }

        private Tuple<double, double> _CurrentDiscountFilter = Tuple.Create(double.MinValue, double.MaxValue);

        public Tuple<double, double> CurrentDiscountFilter
        {
            get
            {
                return _CurrentDiscountFilter;
            }
            set
            {
                _CurrentDiscountFilter = value;
                if (PropertyChanged != null)
                {
                    // при изменении фильтра список перерисовывается
                    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ServicesCount"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FilteredServicesCount"));
                }
            }
        }


        private string _SearchFilter = "";
        public string SearchFilter
        {
            get { return _SearchFilter; }
            set
            {
                _SearchFilter = value;
                if (PropertyChanged != null)
                {
                    // при изменении фильтра список перерисовывается
                    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ServicesCount"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FilteredServicesCount"));
                }
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchFilter = SearchFilterTextBox.Text;
        }

        private void DiscountFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DiscountFilterComboBox.SelectedIndex >= 0)
                CurrentDiscountFilter = Tuple.Create(
                FilterByDiscountValuesList[DiscountFilterComboBox.SelectedIndex].Item2,
                FilterByDiscountValuesList[DiscountFilterComboBox.SelectedIndex].Item3
            );
        }

        public int ServicesCount
        {
            get
            {
                return _ServiceList.Count;
            }
        }
        public int FilteredServicesCount
        {
            get
            {
                return ServiceList.Count;
            }
        }




       

        private void EditButon_Click(object sender, RoutedEventArgs e)
        {
            var SelectedService = MainDataGrid.SelectedItem as Service;
            //var EditServiceWindow = new ServiceWindow(SelectedService);
            NavigationService.Navigate(new Frames.ServiceFrame(SelectedService));
            PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
            //if ((bool)EditServiceWindow.ShowDialog())
            //{
                // при успешном завершении не забываем перерисовать список услуг
            //    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                // и еще счетчики - их добавьте сами
            //}
        }


        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var item = MainDataGrid.SelectedItem as Service;

            if (item.ClientService.Count > 0)
            {
                MessageBox.Show("Нельзя удалять услугу, она уже оказана");
                return;
            }

            Core.DB.Service.Remove(item);

            Core.DB.SaveChanges();

            ServiceList = Core.DB.Service.ToList();
        }

        private void SubscrideButton_Click(object sender, RoutedEventArgs e)
        {
            var SelectedService = MainDataGrid.SelectedItem as Service;
            NavigationService.Navigate(new Frames.ClientServiceFrame(SelectedService));
            //var SubscrideServiceWindow = new windows.ClientServiceWindow(SelectedService);
            //SubscrideServiceWindow.ShowDialog();

        }



        public Boolean IsAdminMode
        {
            get { return MainWindow._IsAdminMode; }
            set
            {
                MainWindow._IsAdminMode = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AdminModeCaption"));
                    PropertyChanged(this, new PropertyChangedEventArgs("AdminVisibility"));
                }
            }
        }

        public string AdminVisibility
        {
            get
            {
                if (IsAdminMode) return "Visible";
                return "Collapsed";
            }
        }

    }

}

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
using ProizvPR.DBConn;

namespace ProizvPR.Pages
{
    /// <summary>
    /// Логика взаимодействия для StatisticsPage.xaml
    /// </summary>
    public partial class StatisticsPage : Page
    {
        public StatisticsPage()
        {
            InitializeComponent();
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var allRequests = Conn.db.Requests.ToList();
            int total = allRequests.Count;
            txtTotalRequests.Text = "ВСЕГО ЗАЯВОК: " + total;

            int completed = 0;
            var completedStatus = Conn.db.Statuses.FirstOrDefault(s => s.status_name == "Выполнено");
            if (completedStatus != null)
            {
                completed = allRequests.Count(r => r.id_status == completedStatus.id_status);
            }
            txtCompletedRequests.Text = "ВЫПОЛНЕННЫХ ЗАЯВОК: " + completed;

            var failureTypes = Conn.db.Failure_types.ToList();
            var stats = new List<dynamic>();

            foreach (var ft in failureTypes)
            {
                int count = allRequests.Count(r => r.id_failure_type == ft.id_failure_type);
                int completedCount = 0;
                if (completedStatus != null)
                {
                    completedCount = allRequests.Count(r => r.id_failure_type == ft.id_failure_type && r.id_status == completedStatus.id_status);
                }

                stats.Add(new
                {
                    ft.type_name,
                    count = count,
                    completed = completedCount
                });
            }

            lvFailureStats.ItemsSource = stats;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
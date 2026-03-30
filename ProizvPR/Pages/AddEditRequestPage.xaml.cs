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
    /// Логика взаимодействия для AddEditRequestPage.xaml
    /// </summary>
    public partial class AddEditRequestPage : Page
    {
        private int? editRequestId;
        private Users currentUser;
        private string roleName;

        public AddEditRequestPage(int? requestId, Users user, string role)
        {
            InitializeComponent();
            editRequestId = requestId;
            currentUser = user;
            roleName = role;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var equipment = Conn.db.Equipment.ToList();
            cmbEquipment.ItemsSource = equipment;

            var clients = Conn.db.Clients.ToList();
            cmbClient.ItemsSource = clients;

            var failureTypes = Conn.db.Failure_types.ToList();
            cmbFailureType.ItemsSource = failureTypes;

            var executors = Conn.db.Users.Where(u => u.id_role == 3).ToList();
            cmbExecutor.ItemsSource = executors;

            if (editRequestId.HasValue)
            {
                var request = Conn.db.Requests.FirstOrDefault(r => r.id_request == editRequestId.Value);
                if (request != null)
                {
                    txtNumber.Text = request.request_number;
                    cmbEquipment.SelectedValue = request.id_equipment;
                    cmbClient.SelectedValue = request.id_client;
                    cmbFailureType.SelectedValue = request.id_failure_type;
                    cmbExecutor.SelectedValue = request.assigned_to;
                    dpDeadline.SelectedDate = request.deadline_date;
                    txtDescription.Text = request.description;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNumber.Text))
            {
                MessageBox.Show("ВВЕДИ НОМЕР ЗАЯВКИ!");
                return;
            }

            if (cmbEquipment.SelectedItem == null)
            {
                MessageBox.Show("ВЫБЕРИ ОБОРУДОВАНИЕ!");
                return;
            }

            if (cmbClient.SelectedItem == null)
            {
                MessageBox.Show("ВЫБЕРИ КЛИЕНТА!");
                return;
            }

            if (cmbFailureType.SelectedItem == null)
            {
                MessageBox.Show("ВЫБЕРИ ТИП НЕИСПРАВНОСТИ!");
                return;
            }

            try
            {
                if (editRequestId.HasValue)
                {
                    var request = Conn.db.Requests.FirstOrDefault(r => r.id_request == editRequestId.Value);
                    if (request != null)
                    {
                        request.request_number = txtNumber.Text;
                        request.id_equipment = (int)cmbEquipment.SelectedValue;
                        request.id_client = (int)cmbClient.SelectedValue;
                        request.id_failure_type = (int)cmbFailureType.SelectedValue;

                        if (cmbExecutor.SelectedItem != null)
                        {
                            request.assigned_to = (int)cmbExecutor.SelectedValue;
                        }
                        else
                        {
                            request.assigned_to = null;
                        }

                        request.deadline_date = dpDeadline.SelectedDate;
                        request.description = txtDescription.Text;

                        Conn.db.SaveChanges();
                        MessageBox.Show("ЗАЯВКА ИЗМЕНЕНА!");
                    }
                }
                else
                {
                    int maxId = 0;
                    var allRequests = Conn.db.Requests.ToList();
                    foreach (var r in allRequests)
                    {
                        if (r.id_request > maxId) maxId = r.id_request;
                    }

                    var newRequest = new Requests();
                    newRequest.id_request = maxId + 1;
                    newRequest.request_number = txtNumber.Text;
                    newRequest.creation_date = DateTime.Now;
                    newRequest.id_equipment = (int)cmbEquipment.SelectedValue;
                    newRequest.id_client = (int)cmbClient.SelectedValue;
                    newRequest.id_failure_type = (int)cmbFailureType.SelectedValue;

                    if (cmbExecutor.SelectedItem != null)
                    {
                        newRequest.assigned_to = (int)cmbExecutor.SelectedValue;
                    }

                    newRequest.deadline_date = dpDeadline.SelectedDate;
                    newRequest.description = txtDescription.Text;

                    var status = Conn.db.Statuses.FirstOrDefault(s => s.status_name == "В ожидании");
                    if (status != null)
                    {
                        newRequest.id_status = status.id_status;
                    }

                    newRequest.client_agreement = false;
                    newRequest.qr_code_generated = false;

                    Conn.db.Requests.Add(newRequest);
                    Conn.db.SaveChanges();
                    MessageBox.Show("ЗАЯВКА ДОБАВЛЕНА!");
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА: " + ex.Message);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}


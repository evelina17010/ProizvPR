using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProizvPR.DBConn;

namespace ProizvPR.Pages
{
    public partial class RequestsPage : Page
    {
        private Users currentUser;
        private string roleName;
        private object selectedRequest;
        public RequestsPage(Users user, string role)
        {
            InitializeComponent();
            currentUser = user;
            roleName = role;

            if (roleName == "Менеджер" || roleName == "Администратор")
            {
                btnAdd.Visibility = Visibility.Visible;
            }
            var statuses = Conn.db.Statuses.ToList();
            var statusesWithAll = new List<object>();
            statusesWithAll.Add(new { id_status = 0, status_name = "ВСЕ" });
            foreach (var s in statuses)
            {
                statusesWithAll.Add(new { s.id_status, s.status_name });
            }
            cmbStatusFilter.ItemsSource = statusesWithAll;
            cmbStatusFilter.DisplayMemberPath = "status_name";
            cmbStatusFilter.SelectedValuePath = "id_status";
            cmbStatusFilter.SelectedIndex = 0;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }
        private void LoadRequests()
        {
            var allRequests = Conn.db.Requests.ToList();
            var result = new List<object>();
            foreach (var r in allRequests)
            {
                var equipment = Conn.db.Equipment.FirstOrDefault(eq => eq.id_equipment == r.id_equipment);
                var client = Conn.db.Clients.FirstOrDefault(cl => cl.id_client == r.id_client);
                var status = Conn.db.Statuses.FirstOrDefault(st => st.id_status == r.id_status);
                var executor = Conn.db.Users.FirstOrDefault(u => u.id_user == r.assigned_to);
                string equipmentName = "Неизвестно";
                string clientName = "Неизвестно";
                string statusName = "Неизвестно";
                string executorName = "Не назначен";
                if (equipment != null)
                {
                    equipmentName = equipment.equipment_name;
                }
                if (client != null)
                {
                    clientName = client.client_name;
                }
                if (status != null)
                {
                    statusName = status.status_name;
                }
                if (executor != null)
                {
                    executorName = executor.full_name;
                }
                int statusId = r.id_status;

                if (cmbStatusFilter.SelectedItem != null)
                {
                    int filterId = (int)cmbStatusFilter.SelectedValue;
                    if (filterId != 0 && filterId != statusId)
                    {
                        continue;
                    }
                }
                if (string.IsNullOrEmpty(txtSearch.Text) == false)
                {
                    string search = txtSearch.Text.ToLower();
                    if (r.request_number.ToLower().Contains(search) == false && clientName.ToLower().Contains(search) == false && equipmentName.ToLower().Contains(search) == false)
                    {
                        continue;
                    }
                }
                var item = new
                {
                    r.id_request,
                    r.request_number,
                    creation_date = r.creation_date.ToString("dd.MM.yyyy"),
                    equipment_name = equipmentName,
                    client_name = clientName,
                    status_name = statusName,
                    executor_name = executorName,
                    deadline_date = r.deadline_date.HasValue ? r.deadline_date.Value.ToString("dd.MM.yyyy") : "Не указан"
                };

                result.Add(item);
            }

            lvRequests.ItemsSource = result;
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadRequests();
        }
        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRequests();
        }
        private void lvRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRequest = lvRequests.SelectedItem;
        }
        private void btnDetails_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRequest == null)
            {
                MessageBox.Show("Выберите заявку для просмотра деталей", "Внимание",MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int id = (int)selectedRequest.GetType().GetProperty("id_request").GetValue(selectedRequest, null);
            NavigationService.Navigate(new RequestDetailsPage(id, currentUser, roleName));
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRequest == null)
            {
                MessageBox.Show("Выберите заявку для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int id = (int)selectedRequest.GetType().GetProperty("id_request").GetValue(selectedRequest, null);
            NavigationService.Navigate(new AddEditRequestPage(id, currentUser, roleName));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditRequestPage(null, currentUser, roleName));
        }

        private void btnQR_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRequest == null)
            {
                MessageBox.Show("ПОЖАЛУЙСТА, ВЫБЕРИТЕ ЗАЯВКУ ДЛЯ ГЕНЕРАЦИИ ОТЗЫВА!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string feedbackUrl = "https://docs.google.com/forms/d/1CjHun-cIMZVOT6GpQLT1ysQdYJX9z9OWvbshampfagI/edit";
            NavigationService.Navigate(new QRCodePage(feedbackUrl));
            try
            {
                var propertyInfo = selectedRequest.GetType().GetProperty("id_request");
                if (propertyInfo != null)
                {
                    int id = (int)propertyInfo.GetValue(selectedRequest, null);

                    var requestToUpdate = Conn.db.Requests.FirstOrDefault(r => r.id_request == id);
                    if (requestToUpdate != null)
                    {
                        requestToUpdate.qr_code_generated = true;
                        Conn.db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении статуса в БД: " + ex.Message);
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenuPage(currentUser, roleName));
        }
    }
}
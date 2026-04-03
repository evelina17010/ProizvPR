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
    /// Логика взаимодействия для RequestDetailsPage.xaml
    /// </summary>
    public partial class RequestDetailsPage : Page
    {
        private int requestId;
        private Users currentUser;
        private string roleName;
        private Requests currentRequest;

        public RequestDetailsPage(int id, Users user, string role)
        {
            InitializeComponent();
            requestId = id;
            currentUser = user;
            roleName = role;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentRequest = Conn.db.Requests.FirstOrDefault(r => r.id_request == requestId);

            if (currentRequest == null)
            {
                MessageBox.Show("ЗАЯВКА НЕ НАЙДЕНА");
                return;
            }
            var equipment = Conn.db.Equipment.FirstOrDefault(eq => eq.id_equipment == currentRequest.id_equipment);
            var client = Conn.db.Clients.FirstOrDefault(c => c.id_client == currentRequest.id_client);
            txtNumber.Text = currentRequest.request_number;
            txtDate.Text = currentRequest.creation_date.ToString("dd.MM.yyyy");
            if (equipment != null) txtEquipment.Text = equipment.equipment_name;
            else txtEquipment.Text = "Неизвестно";
            if (client != null) txtClient.Text = client.client_name;
            else txtClient.Text = "Неизвестно";
            txtDescription.Text = currentRequest.description;
            var statuses = Conn.db.Statuses.ToList();
            cmbStatus.ItemsSource = statuses;
            cmbStatus.DisplayMemberPath = "status_name";
            cmbStatus.SelectedValuePath = "id_status";
            cmbStatus.SelectedValue = currentRequest.id_status;
            var executors = Conn.db.Users.Where(u => u.id_role == 3).ToList();
            var executorList = new List<dynamic>();
            executorList.Add(new { id_user = 0, full_name = "Не назначен" });
            foreach (var ex in executors)
            {
                executorList.Add(new { ex.id_user, ex.full_name });
            }
            cmbExecutor.ItemsSource = executorList;
            cmbExecutor.DisplayMemberPath = "full_name";
            cmbExecutor.SelectedValuePath = "id_user";
            if (currentRequest.assigned_to.HasValue)
            {
                cmbExecutor.SelectedValue = currentRequest.assigned_to.Value;
            }
            else
            {
                cmbExecutor.SelectedValue = 0;
            }

            if (currentRequest.deadline_date.HasValue)
            {
                dpDeadline.SelectedDate = currentRequest.deadline_date.Value;
            }
            if (roleName == "Менеджер" || roleName == "Администратор")
            {
                borderManagerActions.Visibility = Visibility.Visible;

                var specialists = Conn.db.Users.Where(u => u.id_role == 3).ToList();
                cmbSpecialist.ItemsSource = specialists;
                cmbSpecialist.DisplayMemberPath = "full_name";
                cmbSpecialist.SelectedValuePath = "id_user";
            }
            LoadComments();
        }
        private void LoadComments()
        {
            var allComments = Conn.db.Comments.Where(c => c.id_request == requestId).OrderByDescending(c => c.comment_date).ToList();
            var result = new List<dynamic>();
            foreach (var c in allComments)
            {
                var user = Conn.db.Users.FirstOrDefault(u => u.id_user == c.id_user);
                string userName = "Неизвестно";
                if (user != null) userName = user.full_name;
                result.Add(new
                {
                    c.comment_text,
                    comment_date = c.comment_date.HasValue ? c.comment_date.Value.ToString("dd.MM.yyyy HH:mm") : "",
                    user_name = userName
                });
            }
            lvComments.ItemsSource = result;
        }
        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void cmbExecutor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentRequest.id_status = (int)cmbStatus.SelectedValue;

                int executorId = (int)cmbExecutor.SelectedValue;
                if (executorId == 0)
                {
                    currentRequest.assigned_to = null;
                }
                else
                {
                    currentRequest.assigned_to = executorId;
                }
                currentRequest.deadline_date = dpDeadline.SelectedDate;
                currentRequest.description = txtDescription.Text;
                Conn.db.SaveChanges();
                MessageBox.Show("ИЗМЕНЕНИЯ СОХРАНЕНЫ!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА: " + ex.Message);
            }
        }
        private void btnAddSpecialist_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSpecialist.SelectedItem == null)
            {
                MessageBox.Show("ВЫБЕРИ СПЕЦИАЛИСТА!");
                return;
            }
           try
            {
                var selectedSpecialist = (Users)cmbSpecialist.SelectedItem;
                var action = new Repair_actions();
                action.id_request = requestId;
                action.id_manager = currentUser.id_user;
                action.additional_specialist_id = selectedSpecialist.id_user;
                action.action_reason = "ПРИВЛЕЧЕН ПО ЗАПРОСУ ИСПОЛНИТЕЛЯ";
                action.action_date = DateTime.Now;
                Conn.db.Repair_actions.Add(action);
                Conn.db.SaveChanges();
                MessageBox.Show("СПЕЦИАЛИСТ " + selectedSpecialist.full_name + " ПРИВЛЕЧЁН!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА: " + ex.Message);
            }
        }
        private void btnExtendDeadline_Click(object sender, RoutedEventArgs e)
        {
            if (dpNewDeadline.SelectedDate == null)
            {
                MessageBox.Show("ВЫБЕРИ НОВУЮ ДАТУ!");
                return;
            }
            if (chkClientAgreement.IsChecked != true)
            {
                MessageBox.Show("НЕОБХОДИМО СОГЛАСИЕ КЛИЕНТА!");
                return;
           }
            try
            {
                currentRequest.deadline_date = dpNewDeadline.SelectedDate;
                currentRequest.extension_reason = "СРОК ПРОДЛЁН ДО " + dpNewDeadline.SelectedDate.Value.ToString("dd.MM.yyyy");
                currentRequest.client_agreement = true;
                var status = Conn.db.Statuses.FirstOrDefault(s => s.status_name == "Продлен срок");
                if (status != null)
                {
                    currentRequest.id_status = status.id_status;
                }
                Conn.db.SaveChanges();
                MessageBox.Show("СРОК ВЫПОЛНЕНИЯ ПРОДЛЁН!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА: " + ex.Message);
            }
        }
        private void btnAddComment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewComment.Text))
            {
                MessageBox.Show("ВВЕДИ КОММЕНТАРИЙ!");
                return;
            }
            try
            {
                var comment = new Comments();
                comment.id_request = requestId;
                comment.id_user = currentUser.id_user;
                comment.comment_text = txtNewComment.Text;
                comment.comment_date = DateTime.Now;
                Conn.db.Comments.Add(comment);
                Conn.db.SaveChanges();
                txtNewComment.Text = "";
                LoadComments();
                MessageBox.Show("КОММЕНТАРИЙ ДОБАВЛЕН!");
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


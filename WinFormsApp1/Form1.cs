using KeyAuthLib;
namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        KeyAuth auth = new KeyAuth(
        "1",
        "XhtEKeNKCn",
        "e4847239f7bce4850fc5d9763608e6a65fae03a4db4a67b1c75d9bc1d776f585",
        "1.0"
    );
        public Form1()
        {
            InitializeComponent();
            

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private async void InitializeAsync()
        {
            label1.Visible = true;
            var initResult = await auth.InitializeAsync();
            label1.Visible = false;

            if (!initResult.Success)
            {
                MessageBox.Show($"Initialization failed: {initResult.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        private async void  button1_Click(object sender, EventArgs e)
        {

            string username = usern.Text;
            string password = pass.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            button1.Enabled = false;
            button1.Visible = true;

            try
            {
                var result = await auth.LoginAsync(username, password);

                if (result.Success)
                {
                    MessageBox.Show($"Login successful!\nWelcome {result.Username}", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Open main application form
                    MessageBox.Show("Login");
                }
                else if (result.Message == "VALID_USER_NO_SUBSCRIPTION")
                {
                    var dialogResult = MessageBox.Show("You don't have an active subscription. Add one now?",
                        "Subscription Required", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                    if (dialogResult == DialogResult.Yes)
                    {
                        //ShowSubscriptionForm(username);
                    }
                }
                else
                {
                    MessageBox.Show(result.Message, "Login Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button1.Enabled = true;
                label1.Visible = false;
            }

        }
    }
}

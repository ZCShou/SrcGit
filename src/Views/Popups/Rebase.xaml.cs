using System.Threading.Tasks;
using System.Windows.Media;

namespace SrcGit.Views.Popups
{
    /// <summary>
    ///     变基操作面板
    /// </summary>
    public partial class Rebase : Controls.PopupWidget
    {
        private string repo = null;
        private string on = null;

        public Rebase(string repo, string current, Models.Branch branch)
        {
            this.repo = repo;
            this.on = branch.Head;

            InitializeComponent();

            txtCurrent.Text = current;
            txtOn.Text = !string.IsNullOrEmpty(branch.Remote) ? $"{branch.Remote}/{branch.Name}" : branch.Name;
            iconBased.Data = FindResource("Icon.Branch") as Geometry;
        }

        public Rebase(string repo, string current, Models.Commit commit)
        {
            this.repo = repo;
            this.on = commit.SHA;

            InitializeComponent();

            txtCurrent.Text = current;
            txtSHA.Text = commit.ShortSHA;
            txtOn.Text = commit.Subject;
            badgeSHA.Visibility = System.Windows.Visibility.Visible;
            iconBased.Data = FindResource("Icon.Commit") as Geometry;
        }

        public override string GetTitle()
        {
            return App.Text("Rebase");
        }

        public override Task<bool> Start()
        {
            var autoStash = chkAutoStash.IsChecked == true;
            return Task.Run(() =>
            {
                Models.Watcher.SetEnabled(repo, false);
                new Commands.Rebase(repo, on, autoStash).Exec();
                Models.Watcher.SetEnabled(repo, true);
                return true;
            });
        }
    }
}

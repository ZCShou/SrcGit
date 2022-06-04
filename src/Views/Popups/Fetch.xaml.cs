using System.Threading.Tasks;

namespace SrcGit.Views.Popups
{

    /// <summary>
    ///     拉取更新
    /// </summary>
    public partial class Fetch : Controls.PopupWidget
    {
        private string repo = null;

        public Fetch(Models.Repository repo, string preferRemote)
        {
            this.repo = repo.Path;
            InitializeComponent();
            remotes.ItemsSource = repo.Remotes;
            if (preferRemote != null)
            {
                remotes.SelectedIndex = repo.Remotes.FindIndex(x => x.Name == preferRemote);
                chkFetchAll.IsChecked = false;
            }
            else
            {
                remotes.SelectedIndex = 0;
                chkFetchAll.IsChecked = true;
            }
        }

        public override string GetTitle()
        {
            return App.Text("Fetch.Title");
        }

        public override Task<bool> Start()
        {
            var prune = chkPrune.IsChecked == true;
            var remote = (remotes.SelectedItem as Models.Remote).Name;
            var all = chkFetchAll.IsChecked == true;

            return Task.Run(() =>
            {
                Models.Watcher.SetEnabled(repo, false);

                if (all)
                {
                    foreach (var r in remotes.ItemsSource)
                    {
                        new Commands.Fetch(repo, (r as Models.Remote).Name, prune, UpdateProgress).Exec();
                    }
                }
                else
                {
                    new Commands.Fetch(repo, remote, prune, UpdateProgress).Exec();
                }

                Models.Watcher.SetEnabled(repo, true);
                return true;
            });
        }
    }
}

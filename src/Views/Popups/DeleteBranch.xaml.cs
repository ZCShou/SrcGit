using System;
using System.Threading.Tasks;

namespace SrcGit.Views.Popups
{
    /// <summary>
    ///     删除分支确认
    /// </summary>
    public partial class DeleteBranch : Controls.PopupWidget
    {
        private string repo = null;
        private string branch = null;
        private string remote = null;
        private Action finishHandler = null;

        public DeleteBranch(string repo, string branch, string remote = null)
        {
            this.repo = repo;
            this.branch = branch;
            this.remote = remote;
            InitializeComponent();

            if (string.IsNullOrEmpty(remote))
            {
                txtTarget.Text = branch;
            }
            else
            {
                txtTarget.Text = $"{remote}/{branch}";
            }
        }

        public DeleteBranch Then(Action handler)
        {
            this.finishHandler = handler;
            return this;
        }

        public override string GetTitle()
        {
            return App.Text("DeleteBranch");
        }

        public override Task<bool> Start()
        {
            return Task.Run(() =>
            {
                Models.Watcher.SetEnabled(repo, false);
                var full = branch;

                if (string.IsNullOrEmpty(remote))
                {
                    full = $"refs/heads/{branch}";
                    new Commands.Branch(repo, branch).Delete();
                }
                else
                {
                    full = $"refs/remotes/{remote}/{branch}";
                    new Commands.Push(repo, remote, branch).Exec();
                }

                var exists = Models.Preference.Instance.FindRepository(repo);

                if (exists != null && exists.Filters.Contains(full))
                {
                    exists.Filters.Remove(full);
                }

                finishHandler?.Invoke();
                Models.Watcher.SetEnabled(repo, true);
                return true;
            });
        }
    }
}

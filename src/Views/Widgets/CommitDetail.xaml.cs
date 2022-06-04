using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SrcGit.Views.Widgets
{

    /// <summary>
    ///     提交详情
    /// </summary>
    public partial class CommitDetail : UserControl
    {
        private string repo = null;
        private Models.Commit commit = null;
        private Commands.Context cancelToken = new Commands.Context();

        public CommitDetail()
        {
            InitializeComponent();

            Unloaded += (o, e) =>
            {
                changeList.ItemsSource = new List<Models.Change>();
                changeContainer.CleanUp();
                revisionFiles.Cleanup();
            };
        }

        public void SetData(string repo, Models.Commit commit)
        {
            cancelToken.IsCancelRequested = true;
            cancelToken = new Commands.Context();

            this.repo = repo;
            this.commit = commit;

            revisionFiles.SetData(repo, commit.SHA, cancelToken);
            UpdateInformation(commit);
            UpdateChanges();
        }

        #region DATA
        private void UpdateInformation(Models.Commit commit)
        {
            txtSHA.Text = commit.SHA;
            txtMessage.Text = (commit.Subject + "\n\n" + commit.Message.Trim()).Trim();

            avatarAuthor.Email = commit.Author.Email;
            avatarAuthor.FallbackLabel = commit.Author.Name;
            txtAuthorName.Text = commit.Author.Name;
            txtAuthorEmail.Text = commit.Author.Email;
            txtAuthorTime.Text = commit.Author.Time;

            avatarCommitter.Email = commit.Committer.Email;
            avatarCommitter.FallbackLabel = commit.Committer.Name;
            txtCommitterName.Text = commit.Committer.Name;
            txtCommitterEmail.Text = commit.Committer.Email;
            txtCommitterTime.Text = commit.Committer.Time;

            if (commit.Committer.Email == commit.Author.Email && commit.Committer.Time == commit.Author.Time)
            {
                avatarCommitter.Visibility = Visibility.Hidden;
                committerInfoPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                avatarCommitter.Visibility = Visibility.Visible;
                committerInfoPanel.Visibility = Visibility.Visible;
            }

            if (commit.Parents.Count == 0)
            {
                rowParents.Height = new GridLength(0);
            }
            else
            {
                rowParents.Height = GridLength.Auto;
                var shortPIDs = new List<string>();
                foreach (var p in commit.Parents) shortPIDs.Add(p.Substring(0, 10));
                listParents.ItemsSource = shortPIDs;
            }

            if (!commit.HasDecorators)
            {
                rowRefs.Height = new GridLength(0);
            }
            else
            {
                rowRefs.Height = GridLength.Auto;
                listRefs.ItemsSource = commit.Decorators;
            }
        }

        private void UpdateChanges()
        {
            var cmd = new Commands.CommitChanges(repo, commit.SHA) { Ctx = cancelToken };
            Task.Run(() =>
            {
                var changes = cmd.Result();
                if (cmd.Ctx.IsCancelRequested) return;

                Dispatcher.Invoke(() =>
                {
                    changeList.ItemsSource = changes;
                    changeContainer.SetData(repo, new List<Models.Commit>() { commit }, changes);
                });
            });
        }
        #endregion

        #region EVENTS
        private void OnNavigateParent(object sender, RequestNavigateEventArgs e)
        {
            Models.Watcher.Get(repo)?.NavigateTo(e.Uri.OriginalString);
        }

        private void OnChangeListMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null) return;

            var change = row.DataContext as Models.Change;
            if (change == null) return;

            body.SelectedIndex = 1;
            changeContainer.Select(change);
            e.Handled = true;
        }

        private void OnChangeListContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null) return;

            if (!row.IsSelected)
            {
                changeList.UnselectAll();
                row.IsSelected = true;
            }

            var selectedCount = changeList.SelectedItems.Count;
            var menu = new ContextMenu();
            if (selectedCount == 1)
            {
                var change = changeList.SelectedItems[0] as Models.Change;
                if (change == null) return;

                if (change.Index != Models.Change.Status.Deleted)
                {
                    var history = new MenuItem();
                    history.Header = App.Text("FileHistory");
                    history.IsEnabled = change.Index != Models.Change.Status.Deleted;
                    history.Click += (_, ev) =>
                    {
                        var viewer = new FileHistories(repo, change.Path);
                        viewer.Show();
                        ev.Handled = true;
                    };

                    var blame = new MenuItem();
                    blame.Header = App.Text("Blame");
                    blame.IsEnabled = change.Index != Models.Change.Status.Deleted;
                    blame.Click += (_, ev) =>
                    {
                        var viewer = new Blame(repo, change.Path, commit.SHA);
                        viewer.Show();
                        ev.Handled = true;
                    };

                    var explore = new MenuItem();
                    explore.Header = App.Text("RevealFile");
                    explore.IsEnabled = change.Index != Models.Change.Status.Deleted;
                    explore.Click += (_, ev) =>
                    {
                        var full = Path.GetFullPath(repo + "\\" + change.Path);
                        Process.Start("explorer", $"/select,{full}");
                        ev.Handled = true;
                    };

                    menu.Items.Add(history);
                    menu.Items.Add(blame);
                    menu.Items.Add(explore);
                }

                var copyPath = new MenuItem();
                copyPath.Header = App.Text("CopyPath");
                copyPath.Click += (_, ev) =>
                {
                    Clipboard.SetDataObject(change.Path, true);
                    ev.Handled = true;
                };

                menu.Items.Add(copyPath);
            }
            else
            {
                var copyPath = new MenuItem();
                copyPath.Header = App.Text("CopyPath");
                copyPath.Click += (_, ev) =>
                {
                    var builder = new StringBuilder();
                    foreach (var obj in changeList.SelectedItems)
                    {
                        builder.Append((obj as Models.Change).Path);
                        builder.Append("\n");
                    }
                    Clipboard.SetDataObject(builder.ToString(), true);
                    ev.Handled = true;
                };

                menu.Items.Add(copyPath);
            }

            menu.IsOpen = true;
            e.Handled = true;
        }
        #endregion
    }
}

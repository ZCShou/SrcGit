using System.Threading.Tasks;

namespace SrcGit.Views.Popups
{
    /// <summary>
    ///     仓库配置
    /// </summary>
    public partial class Configure : Controls.PopupWidget
    {
        private string repo = null;

        public string UserName
        {
            get;
            set;
        }
        public string UserEmail
        {
            get;
            set;
        }
        public string Proxy
        {
            get;
            set;
        }

        public Configure(string repo)
        {
            this.repo = repo;
            var cmd = new Commands.Config(repo);
            UserName = cmd.Get("user.name");
            UserEmail = cmd.Get("user.email");
            Proxy = cmd.Get("http.proxy");
            InitializeComponent();
        }

        public override string GetTitle()
        {
            return App.Text("Configure");
        }

        public override Task<bool> Start()
        {
            return Task.Run(() =>
            {
                var cmd = new Commands.Config(repo);
                var oldUser = cmd.Get("user.name");

                if (oldUser != UserName)
                {
                    cmd.Set("user.name", UserName);
                }

                var oldEmail = cmd.Get("user.email");

                if (oldEmail != UserEmail)
                {
                    cmd.Set("user.email", UserEmail);
                }

                var oldProxy = cmd.Get("http.proxy");

                if (oldProxy != Proxy)
                {
                    cmd.Set("http.proxy", Proxy);
                }

                return true;
            });
        }
    }
}

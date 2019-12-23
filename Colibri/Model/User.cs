using System.Diagnostics;
using Jupiter.Mvvm;
using VkLib.Core.Users;

namespace Colibri.Model
{
    public class User : BindableBase
    {
        private VkProfile _profile;

        public VkProfile Profile
        {
            get { return _profile; }
            set { Set(ref _profile, value); }
        }

        public bool IsOnline
        {
            get { return _profile.IsOnline == true; }
            set
            {
                if (_profile.IsOnline == value)
                    return;

                _profile.IsOnline = value;
                RaisePropertyChanged();
            }
        }

        public User(VkProfile profile)
        {
            if (profile == null)
                Debugger.Break();
            _profile = profile;
        }
    }
}

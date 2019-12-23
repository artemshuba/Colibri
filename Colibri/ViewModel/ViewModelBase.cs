//using System.Collections.Generic;
//using Windows.UI.Xaml.Navigation;
//using Neptune.Universal.Helpers;

//namespace ColibriUniversal.ViewModel
//{
//    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
//    {
//        public Dictionary<string, LongRunningTask> Tasks { get; } = new Dictionary<string, LongRunningTask>();

//        public virtual void Activate(NavigationMode navigationMode)
//        {

//        }

//        public virtual void Deactivate()
//        {

//        }

//        #region Long Running Tasks helpers

//        protected void RegisterTasks(params string[] ids)
//        {
//            foreach (var id in ids)
//            {
//                Tasks.Add(id, new LongRunningTask());
//            }
//        }

//        protected void TaskStarted(string id)
//        {
//            Tasks[id].Error = null;
//            Tasks[id].IsWorking = true;
//        }

//        protected void TaskFinished(string id)
//        {
//            Tasks[id].IsWorking = false;
//        }

//        protected void TaskError(string id, string error)
//        {
//            Tasks[id].Error = error;
//            Tasks[id].IsWorking = false;
//        }


//        #endregion
//    }
//}

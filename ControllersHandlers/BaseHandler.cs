namespace Controllers
{
    public class BaseHandler : IControllerHandlerGS
    {
        public IControllerHandlerGS handler;
        public void setNext(IControllerHandlerGS handler)
        {
            this.handler = handler;
        }
        public bool handle(ref Newtonsoft.Json.Linq.JObject json, ref Instasoft.Model.TaskGS task, ref string message)
        {
            return true;
        }
    }
}
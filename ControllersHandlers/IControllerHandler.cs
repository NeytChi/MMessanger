namespace Controllers
{
    public interface IControllerHandlerGS
    {
        void setNext(IControllerHandlerGS handler);
        bool handle(ref Newtonsoft.Json.Linq.JObject json, ref Instasoft.Model.TaskGS task, ref string message);
    }
}
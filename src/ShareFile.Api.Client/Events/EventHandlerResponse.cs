using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Events
{
    public enum EventHandlerResponseAction { Ignore, Throw, Retry, Redirect }
    
    public class EventHandlerResponse
    {
        public EventHandlerResponseAction Action { get; set; }
        public Redirection Redirection { get; set; }

        public static EventHandlerResponse Throw = new EventHandlerResponse {Action = EventHandlerResponseAction.Throw};
        public static EventHandlerResponse Ignore = new EventHandlerResponse { Action = EventHandlerResponseAction.Ignore };

        public static EventHandlerResponse Redirect(Redirection redirection)
        {
            return new EventHandlerResponse
            {
                Action = EventHandlerResponseAction.Redirect,
                Redirection = redirection
            };
        }
    }
}

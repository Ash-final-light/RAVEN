using Framework.Core.Event;

namespace Events
{
    public static class GameEvents
    {
        public static readonly EventIdentity NotificationGet = new("NotificationGet");
        public static readonly EventIdentity NotificationLost = new("NotificationLost");
        public static readonly EventIdentity NotificationNotGetMore = new("NotificationNotGetMore");
        
    }
}
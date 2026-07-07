using Tools.Action.Running;
using Tools.Buff;
using Tools.InputHandle;

namespace Tools.Action.Data.Event
{
    [System.Serializable]
    public class EventAbility
    {
        private readonly ActionEventDispatcher dispatcher;

        public EventAbility(ActionStateController stateController, BuffManager buffManager)
        {
            dispatcher = new ActionEventDispatcher(buffManager);
        }

        public void Dispatch(
            ActionStateController controller,
            ActionPlayer player,
            ActionClip clip,
            int tick,
            ActionEventClipData data)
        {
            var context = new ActionEventContext(controller, player, clip, tick);
            dispatcher.Dispatch(context, data);
        }
    }
}

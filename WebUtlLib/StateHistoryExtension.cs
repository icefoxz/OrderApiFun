using OrderDbLib.Entities;
using OrderHelperLib.Contracts;
using Utls;

namespace WebUtlLib;

public static class StateHistoryExtension
{
    public static void AddStateHistory(this DeliveryOrder order, DoSubState state) =>
        order.AddStateHistory(state.StateId, StateSegments.Type.None);
    public static void AddStateHistory(this DeliveryOrder order, DoSubState state, string remark) =>
        order.AddStateHistory(state.StateId, StateSegments.Type.Remark, remark);
    public static void AddStateHistory(this DeliveryOrder order, DoSubState state, StateSegments.Type type, string data) =>
        order.AddStateHistory(state.StateId, type, data);

    public static void AddStateHistory(this DeliveryOrder order, DoSubState state, string[] images) =>
        order.AddStateHistory(state.StateId, images);
    public static void AddStateHistory(this DeliveryOrder order, string subState) =>
        order.AddStateHistory(subState, StateSegments.Type.None);
    public static void AddStateHistory(this DeliveryOrder order, string subState, string remark) =>
        order.AddStateHistory(subState, StateSegments.Type.Remark, remark);
    public static void AddStateHistory(this DeliveryOrder order, string subState, string[] images) =>
        order.AddStateHistory(subState, StateSegments.Type.Images, Json.Serialize(images));
    public static void AddStateHistory(this DeliveryOrder order, string subState,
        StateSegments.Type type, string? data = null)
    {
        var state = DoStateMap.GetState(subState);
        var ss = new StateSegment
        {
            StateName = state?.StateName ?? string.Empty,
            SubState = subState,
            Timestamp = DateTime.Now,
            Type = type.Text(),
            Data = data,
        };
        order.StateHistory = order.StateHistory.Append(ss).ToArray();
    }
}
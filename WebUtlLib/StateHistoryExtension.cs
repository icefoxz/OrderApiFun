using OrderDbLib.Entities;
using OrderHelperLib.Contracts;

namespace WebUtlLib;

public static class StateHistoryExtension
{
    public static void AddStateHistory(this DeliveryOrder order, DoSubState state, string? remark = null) =>
        order.AddStateHistory(state.StateId, remark);
    public static void AddStateHistory(this DeliveryOrder order, int subState, string? remark) =>
        order.AddStateHistory(subState, null, remark);
    public static void AddStateHistory(this DeliveryOrder order, int subState, string? imageUrl, string? remark)
    {
        var ss = new StateSegment
        {
            SubState = subState,
            Timestamp = DateTime.Now,
            ImageUrl = imageUrl,
            Remark = remark
        };
        order.StateHistory = order.StateHistory.Append(ss).ToArray();
    }
}
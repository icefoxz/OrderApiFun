using Mapster;
using OrderDbLib.Entities;
using OrderHelperLib.Dtos.DeliveryOrders;

namespace Q_DoApi.DtoMapping;

public class EntityMapper
{
    public static TypeAdapterConfig Config { get; private set; } = ConfigSetup();

    private static TypeAdapterConfig ConfigSetup()
    {
        Config = new TypeAdapterConfig();
        Config.NewConfig<DeliverOrderModel, DeliveryOrder>()
            .Ignore(d => d.User)
            .Ignore(d => d.UserId)
            .Ignore(d => d.SenderInfo)
            .Ignore(d => d.ReceiverInfo.User)
            .Ignore(d => d.ReceiverInfo.UserId);
            
        return Config;
    }
}
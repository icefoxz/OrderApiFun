using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using OrderApiFun.Core.Middlewares;

namespace Q_DoApi.Core.Extensions
{
    public static class FunctionContextExtension
    {
        public static string? GetUserId(this FunctionContext context) => context.Items[Auth.UserId].ToString();

        public static long? GetRiderId(this FunctionContext context) =>
            long.TryParse(context.Items[Auth.RiderId].ToString(), out var id) ? id : null;
        public static string? GetRole(this FunctionContext context) => context.Items[Auth.ContextRole].ToString();
        public static bool IsUser(this FunctionContext context) => context.GetRole() == Auth.Role_User;
        public static bool IsRider(this FunctionContext context) => context.GetRole() == Auth.Role_Rider;
    }
}